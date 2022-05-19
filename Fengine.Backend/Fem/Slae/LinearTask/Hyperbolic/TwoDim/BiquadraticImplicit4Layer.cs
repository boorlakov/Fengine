using Fengine.Backend.DataModels;
using Fengine.Backend.Fem.Mesh;
using Fengine.Backend.Integration;
using Fengine.Backend.LinearAlgebra.Matrix;
using Sprache.Calc;

namespace Fengine.Backend.Fem.Slae.LinearTask.Hyperbolic.TwoDim;

public class BiquadraticImplicit4Layer : ISlae
{
    private readonly FiniteElement[] _finiteElements;
    private SortedSet<int>[] _boundsList;

    private Func<Dictionary<string, double>, double> _evalRhsFuncAt;
    private Func<Dictionary<string, double>, double> _evalLambdaFuncAt;
    private Func<Dictionary<string, double>, double> _evalGammaFuncAt;

    private Func<Dictionary<string, double>, double> _evalBoundaryFuncLeftAt;
    private Func<Dictionary<string, double>, double> _evalBoundaryFuncRightAt;
    private Func<Dictionary<string, double>, double> _evalBoundaryFuncLowerAt;
    private Func<Dictionary<string, double>, double> _evalBoundaryFuncUpperAt;
    private Func<Dictionary<string, double>, double> _evalSigmaFuncAt;

    private readonly double[,] _localMatrixForThirdBoundaryCondition =
    {
        {
            2.0 / 15.0, 1.0 / 15.0, -1.0 / 30.0
        },
        {
            1.0 / 15.0, 8.0 / 15.0, 1.0 / 15.0
        },
        {
            -1.0 / 30.0, 1.0 / 15.0, 2.0 / 15.0
        }
    };

    private IIntegrator _integrator;
    private LinearAlgebra.SlaeSolver.ISlaeSolver _slaeSolver;
    private Func<Dictionary<string, double>, double> _evalInitialFuncUpperAt;

    public BiquadraticImplicit4Layer(IMatrix matrix, double[] rhsVec, double[] resVec)
    {
        Matrix = matrix;
        RhsVec = rhsVec;
        ResVec = resVec;
    }

    public BiquadraticImplicit4Layer(IMatrix matrix, double[] rhsVec)
    {
        Matrix = matrix;
        RhsVec = rhsVec;
        ResVec = new double[rhsVec.Length];
    }

    private IMatrix _stiffnesses;

    private IMatrix _masses;
    private IMatrix _massesChi;
    private IMatrix _massesSigma;
    private Func<Dictionary<string, double>, double> _evalChiFuncAt;

    public BiquadraticImplicit4Layer
    (
        DataModels.Area.TwoDim area,
        Mesh.Cylindrical.TwoDim meshSpatial,
        Mesh.Time.OneDim meshTime,
        InputFuncs inputFuncs,
        DataModels.Conditions.Boundary.TwoDim boundaryConditions,
        LinearAlgebra.SlaeSolver.ISlaeSolver slaeSolver,
        IIntegrator integrator,
        DataModels.Conditions.Initial initialCondition
    )
    {
        ConfigureServices(slaeSolver, integrator);

        CompileInputFunctions(inputFuncs, boundaryConditions, initialCondition);

        _finiteElements = GetFiniteElementsFrom(meshSpatial);

        (Matrix, ResVec, RhsVec) = GetPortraitFrom(meshSpatial);

        (_stiffnesses, _, _) = GetPortraitFrom(meshSpatial);

        (_masses, _, _) = GetPortraitFrom(meshSpatial);

        Weights = new double[meshTime.Nodes.Length][];

        for (var i = 0; i < Weights.Length; i++)
        {
            Weights[i] = new double[ResVec.Length];
        }

        GetInitialWeights(meshTime);

        AssemblyGlobally(area, meshSpatial);

        for (var timeStamp = 3; timeStamp < meshTime.Nodes.Length; timeStamp++)
        {
            ApplyScheme(meshTime, meshSpatial, timeStamp);
            ApplyBoundaryConditions(boundaryConditions, area, meshSpatial);
        }
    }

    private void GetInitialWeights(Mesh.Time.OneDim meshTime)
    {
        var k = 0;

        foreach (var finiteElement in _finiteElements)
        {
            var kCopy = k;
            var points = new[]
            {
                GetAllPointsIn(finiteElement, meshTime.Nodes[0].Coordinates[Axis.T]),
                GetAllPointsIn(finiteElement, meshTime.Nodes[1].Coordinates[Axis.T]),
                GetAllPointsIn(finiteElement, meshTime.Nodes[2].Coordinates[Axis.T])
            };

            foreach (var p in points[0])
            {
                Weights[0][k] = _evalInitialFuncUpperAt(p);
                k++;
            }

            k = kCopy;

            foreach (var p in points[1])
            {
                Weights[1][k] = _evalInitialFuncUpperAt(p);
                k++;
            }

            k = kCopy;

            foreach (var p in points[2])
            {
                Weights[1][k] = _evalInitialFuncUpperAt(p);
                k++;
            }
        }
    }

    private void ApplyScheme(Mesh.Time.OneDim meshTime, Mesh.Cylindrical.TwoDim meshSpatial, int timeStamp)
    {
        var t = new[]
        {
            meshTime.Nodes[timeStamp].Coordinates[Axis.T],
            meshTime.Nodes[timeStamp - 1].Coordinates[Axis.T],
            meshTime.Nodes[timeStamp - 2].Coordinates[Axis.T],
            meshTime.Nodes[timeStamp - 3].Coordinates[Axis.T],
        };

        var t0 = new[]
        {
            t[0] - t[1],
            t[0] - t[2],
            t[0] - t[3]
        };

        var t1 = new[]
        {
            t[1] - t[2],
            t[1] - t[3]
        };

        var t2 = t[2] - t[3];

        var schemeWeightsChi = new[]
        {
            2 * (t0[0] + t0[1] + t0[2]) / (t0[2] * t0[1] * t0[0]),
            2 * (t0[0] + t0[1]) / (t0[2] * t1[1] * t2),
            2 * (t0[0] + t0[2]) / (t0[1] * t1[0] * t2),
            2 * (t0[1] + t0[2]) / (t0[0] * t1[0] * t1[1])
        };

        var schemeWeightsSigma = new[]
        {
            (t0[0] * t0[1] + t0[0] * t0[2] + t0[1] * t0[2]) / (t0[2] * t0[1] * t0[0]),
            t0[0] * t0[1] / (t0[2] * t1[1] * t2),
            t0[0] * t0[2] / (t0[1] * t1[0] * t2),
            (t0[1] + t0[2]) / (t0[0] * t1[0] * t1[1]),
        };

        for (var i = 0; i < Matrix.Data["di"].Length; i++)
        {
            Matrix.Data["di"][i] = _stiffnesses.Data["di"][i] +
                                   schemeWeightsChi[0] * _massesChi.Data["di"][i] +
                                   schemeWeightsSigma[0] * _massesSigma.Data["di"][i];
        }

        for (var i = 0; i < Matrix.Data["ggl"].Length; i++)
        {
            Matrix.Data["ggl"][i] = _stiffnesses.Data["ggl"][i] +
                                    schemeWeightsChi[0] * _massesChi.Data["ggl"][i] +
                                    schemeWeightsSigma[0] * _massesSigma.Data["ggl"][i];
        }

        for (var i = 0; i < Matrix.Data["ggu"].Length; i++)
        {
            Matrix.Data["ggu"][i] = _stiffnesses.Data["ggu"][i] +
                                    schemeWeightsChi[0] * _massesChi.Data["ggu"][i] +
                                    schemeWeightsSigma[0] * _massesSigma.Data["ggu"][i];
        }

        EvalTaskRhs(t[0], meshSpatial);

        var rhsWeights = new[]
        {
            LinearAlgebra.GeneralOperations.MatrixMultiply(_masses, Weights[timeStamp - 3]),
            LinearAlgebra.GeneralOperations.MatrixMultiply(_masses, Weights[timeStamp - 2]),
            LinearAlgebra.GeneralOperations.MatrixMultiply(_masses, Weights[timeStamp - 1])
        };

        for (var i = 0; i < RhsVec.Length; i++)
        {
            RhsVec[i] += schemeWeightsSigma[1] * rhsWeights[0][i];
            RhsVec[i] += schemeWeightsSigma[2] * rhsWeights[1][i];
            RhsVec[i] += schemeWeightsSigma[3] * rhsWeights[2][i];
        }
    }

    private void EvalTaskRhs(double time, Mesh.Cylindrical.TwoDim meshSpatial)
    {
        var f = new double[(2 * meshSpatial.R.Length - 1) * (2 * meshSpatial.Z.Length - 1)];

        var k = 0;

        foreach (var finiteElement in _finiteElements)
        {
            var points = GetAllPointsIn(finiteElement, time);

            foreach (var point in points)
            {
                f[k] = _evalRhsFuncAt(point);
                k++;
            }
        }

        var b = LinearAlgebra.GeneralOperations.MatrixMultiply(Matrix, f);
        b.AsSpan().CopyTo(RhsVec);
    }

    private void CompileInputFunctions
    (
        InputFuncs inputFuncs,
        DataModels.Conditions.Boundary.TwoDim boundaryConditions,
        DataModels.Conditions.Initial initialCondition
    )
    {
        var calculator = new XtensibleCalculator();
        _evalRhsFuncAt = calculator.ParseFunction(inputFuncs.RhsFunc).Compile();
        _evalLambdaFuncAt = calculator.ParseFunction(inputFuncs.Lambda).Compile();
        _evalGammaFuncAt = calculator.ParseFunction(inputFuncs.Gamma).Compile();
        _evalSigmaFuncAt = calculator.ParseFunction(inputFuncs.Sigma).Compile();
        _evalChiFuncAt = calculator.ParseFunction(inputFuncs.Chi).Compile();
        _evalBoundaryFuncLeftAt = calculator.ParseFunction(boundaryConditions.LeftFunc).Compile();
        _evalBoundaryFuncRightAt = calculator.ParseFunction(boundaryConditions.RightFunc).Compile();
        _evalBoundaryFuncLowerAt = calculator.ParseFunction(boundaryConditions.LowerFunc).Compile();
        _evalBoundaryFuncUpperAt = calculator.ParseFunction(boundaryConditions.UpperFunc).Compile();
        _evalInitialFuncUpperAt = calculator.ParseFunction(initialCondition.T0).Compile();
    }

    public double[] Solve(Accuracy accuracy)
    {
        return _slaeSolver.Solve(this, accuracy);
    }

    public IMatrix Matrix { get; set; }
    public double[] RhsVec { get; set; }
    public double[] ResVec { get; set; }

    public double[][] Weights { get; set; }

    private void ConfigureServices
    (
        LinearAlgebra.SlaeSolver.ISlaeSolver slaeSolver,
        IIntegrator integrator
    )
    {
        _slaeSolver = slaeSolver;
        _integrator = integrator;
    }

    private void ApplyBoundaryConditions
    (
        DataModels.Conditions.Boundary.TwoDim boundaryConditions,
        DataModels.Area.TwoDim area,
        Mesh.Cylindrical.TwoDim mesh
    )
    {
        var numberOfFiniteElementsAtAxisR = mesh.R.Length - 1;
        var numberOfFiniteElementsAtAxisZ = mesh.Z.Length - 1;

        switch (boundaryConditions.LeftType)
        {
            case "Second":
                for (var i = 0; i < numberOfFiniteElementsAtAxisZ; i++)
                {
                    var numberOfCurrentFiniteElement = i * numberOfFiniteElementsAtAxisR;

                    var leftBorder = new[]
                    {
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[0],
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[3],
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[6]
                    };


                    var hZ =
                    (
                        _finiteElements[numberOfCurrentFiniteElement].Nodes[2].z -
                        _finiteElements[numberOfCurrentFiniteElement].Nodes[0].z
                    ) / 2.0;

                    var leftBorderPoints = new[]
                    {
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[0].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[0].z
                        ),
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[0].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[0].z + hZ
                        ),
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[2].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[2].z
                        )
                    };

                    var derivativeValues = new[]
                    {
                        _evalBoundaryFuncLeftAt(leftBorderPoints[0]),
                        _evalBoundaryFuncLeftAt(leftBorderPoints[1]),
                        _evalBoundaryFuncLeftAt(leftBorderPoints[2])
                    };

                    RhsVec[leftBorder[0]] += hZ / 30.0 *
                                             (
                                                 4.0 * derivativeValues[0]
                                                 + 2.0 * derivativeValues[1]
                                                 - 1.0 * derivativeValues[2]
                                             );

                    RhsVec[leftBorder[1]] += hZ / 30.0 *
                                             (
                                                 2 * derivativeValues[0]
                                                 + 16 * derivativeValues[1]
                                                 + 2 * derivativeValues[2]
                                             );

                    RhsVec[leftBorder[2]] += hZ / 30.0 *
                                             (
                                                 -1 * derivativeValues[0]
                                                 + 2 * derivativeValues[1]
                                                 + 4 * derivativeValues[2]
                                             );
                }

                break;
            case "Third":
                for (var i = 0; i < numberOfFiniteElementsAtAxisZ; i++)
                {
                    var numberOfCurrentFiniteElement = i * numberOfFiniteElementsAtAxisR;

                    var leftBorder = new[]
                    {
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[0],
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[3],
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[6]
                    };


                    var hZ =
                    (
                        _finiteElements[numberOfCurrentFiniteElement].Nodes[2].z -
                        _finiteElements[numberOfCurrentFiniteElement].Nodes[0].z
                    ) / 2.0;

                    var leftBorderPoints = new[]
                    {
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[0].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[0].z
                        ),
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[0].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[0].z + hZ
                        ),
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[2].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[2].z
                        )
                    };

                    var localUBetaVec = new[]
                    {
                        _evalBoundaryFuncLeftAt(leftBorderPoints[0]),
                        _evalBoundaryFuncLeftAt(leftBorderPoints[1]),
                        _evalBoundaryFuncLeftAt(leftBorderPoints[2])
                    };

                    var borderPartToInsert = LinearAlgebra.GeneralOperations.MatrixMultiply
                    (
                        _localMatrixForThirdBoundaryCondition,
                        localUBetaVec
                    );

                    Matrix.Data["di"][leftBorder[0]] += hZ * boundaryConditions.Beta *
                                                        _localMatrixForThirdBoundaryCondition[0, 0];

                    Matrix.Data["di"][leftBorder[1]] += hZ * boundaryConditions.Beta *
                                                        _localMatrixForThirdBoundaryCondition[1, 1];

                    Matrix.Data["di"][leftBorder[2]] += hZ * boundaryConditions.Beta *
                                                        _localMatrixForThirdBoundaryCondition[2, 2];

                    // (3, 0) || (0, 3)
                    Matrix.Data["ggl"][Matrix.Profile["ig"][leftBorder[1]]] +=
                        hZ * boundaryConditions.Beta
                           * _localMatrixForThirdBoundaryCondition[1, 0];
                    Matrix.Data["ggu"][Matrix.Profile["ig"][leftBorder[1]]] +=
                        hZ * boundaryConditions.Beta
                           * _localMatrixForThirdBoundaryCondition[1, 0];

                    // (6, 0) || (0, 6)
                    Matrix.Data["ggl"][Matrix.Profile["ig"][leftBorder[2]]] +=
                        hZ * boundaryConditions.Beta
                           * _localMatrixForThirdBoundaryCondition[2, 0];
                    Matrix.Data["ggu"][Matrix.Profile["ig"][leftBorder[2]]] +=
                        hZ * boundaryConditions.Beta
                           * _localMatrixForThirdBoundaryCondition[2, 0];

                    // (6, 3)
                    var indent = 0;

                    while (Matrix.Profile["jg"][Matrix.Profile["ig"][leftBorder[2]] + indent] != leftBorder[1])
                    {
                        indent++;
                    }

                    Matrix.Data["ggl"][Matrix.Profile["ig"][leftBorder[2]] + indent] +=
                        hZ * boundaryConditions.Beta
                           * _localMatrixForThirdBoundaryCondition[2, 1];
                    Matrix.Data["ggu"][Matrix.Profile["ig"][leftBorder[2]] + indent] +=
                        hZ * boundaryConditions.Beta
                           * _localMatrixForThirdBoundaryCondition[2, 1];

                    RhsVec[leftBorder[0]] +=
                        hZ * boundaryConditions.Beta *
                        (4.0 * borderPartToInsert[0] + 2.0 * borderPartToInsert[1] - borderPartToInsert[2]) /
                        30.0;

                    RhsVec[leftBorder[1]] +=
                        hZ * boundaryConditions.Beta *
                        (2.0 * borderPartToInsert[0] + 16.0 * borderPartToInsert[1] + 2.0 * borderPartToInsert[2]) /
                        30.0;

                    RhsVec[leftBorder[2]] +=
                        hZ * boundaryConditions.Beta *
                        (-1.0 * borderPartToInsert[0] + 2.0 * borderPartToInsert[1] + 4.0 * borderPartToInsert[2]) /
                        30.0;
                }

                break;
        }

        switch (boundaryConditions.RightType)
        {
            case "Second":
                for (var i = 0; i < numberOfFiniteElementsAtAxisZ; i++)
                {
                    var numberOfCurrentFiniteElement =
                        i * numberOfFiniteElementsAtAxisR + numberOfFiniteElementsAtAxisR - 1;

                    var rightBorder = new[]
                    {
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[2],
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[5],
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[8]
                    };

                    var hZ =
                    (
                        _finiteElements[numberOfCurrentFiniteElement].Nodes[3].z -
                        _finiteElements[numberOfCurrentFiniteElement].Nodes[1].z
                    ) / 2.0;

                    var rightBorderPoints = new[]
                    {
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[1].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[1].z
                        ),
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[1].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[1].z + hZ
                        ),
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[3].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[3].z
                        )
                    };

                    var derivativeValues = new[]
                    {
                        _evalBoundaryFuncLeftAt(rightBorderPoints[0]),
                        _evalBoundaryFuncLeftAt(rightBorderPoints[1]),
                        _evalBoundaryFuncLeftAt(rightBorderPoints[2])
                    };

                    RhsVec[rightBorder[0]] += hZ / 30.0 *
                                              (
                                                  4.0 * derivativeValues[0]
                                                  + 2.0 * derivativeValues[1]
                                                  - 1.0 * derivativeValues[2]
                                              );

                    RhsVec[rightBorder[1]] += hZ / 30.0 *
                                              (
                                                  2 * derivativeValues[0]
                                                  + 16 * derivativeValues[1]
                                                  + 2 * derivativeValues[2]
                                              );

                    RhsVec[rightBorder[2]] += hZ / 30.0 *
                                              (
                                                  -1 * derivativeValues[0]
                                                  + 2 * derivativeValues[1]
                                                  + 4 * derivativeValues[2]
                                              );
                }

                break;
            case "Third":
                for (var i = 0; i < numberOfFiniteElementsAtAxisZ; i++)
                {
                    var numberOfCurrentFiniteElement =
                        i * numberOfFiniteElementsAtAxisR + numberOfFiniteElementsAtAxisR;

                    var rightBorder = new[]
                    {
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[2],
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[5],
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[8]
                    };

                    var hZ =
                    (
                        _finiteElements[numberOfCurrentFiniteElement].Nodes[3].z -
                        _finiteElements[numberOfCurrentFiniteElement].Nodes[1].z
                    ) / 2.0;

                    var rightBorderPoints = new[]
                    {
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[1].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[1].z
                        ),
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[1].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[1].z + hZ
                        ),
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[3].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[3].z
                        )
                    };

                    var localUBetaVec = new[]
                    {
                        _evalBoundaryFuncLeftAt(rightBorderPoints[0]),
                        _evalBoundaryFuncLeftAt(rightBorderPoints[1]),
                        _evalBoundaryFuncLeftAt(rightBorderPoints[2])
                    };

                    var borderPartToInsert = LinearAlgebra.GeneralOperations.MatrixMultiply
                    (
                        _localMatrixForThirdBoundaryCondition,
                        localUBetaVec
                    );

                    Matrix.Data["di"][rightBorder[0]] += hZ * boundaryConditions.Beta *
                                                         _localMatrixForThirdBoundaryCondition[0, 0];

                    Matrix.Data["di"][rightBorder[1]] += hZ * boundaryConditions.Beta *
                                                         _localMatrixForThirdBoundaryCondition[1, 1];

                    Matrix.Data["di"][rightBorder[2]] += hZ * boundaryConditions.Beta *
                                                         _localMatrixForThirdBoundaryCondition[2, 2];

                    // (3, 0) || (0, 3)
                    Matrix.Data["ggl"][Matrix.Profile["ig"][rightBorder[1]]] +=
                        hZ * boundaryConditions.Beta
                           * _localMatrixForThirdBoundaryCondition[1, 0];
                    Matrix.Data["ggu"][Matrix.Profile["ig"][rightBorder[1]]] +=
                        hZ * boundaryConditions.Beta
                           * _localMatrixForThirdBoundaryCondition[1, 0];

                    // (6, 0) || (0, 6)
                    Matrix.Data["ggl"][Matrix.Profile["ig"][rightBorder[2]]] +=
                        hZ * boundaryConditions.Beta
                           * _localMatrixForThirdBoundaryCondition[2, 0];
                    Matrix.Data["ggu"][Matrix.Profile["ig"][rightBorder[2]]] +=
                        hZ * boundaryConditions.Beta
                           * _localMatrixForThirdBoundaryCondition[2, 0];

                    // (6, 3)
                    var indent = 0;

                    while (Matrix.Profile["jg"][Matrix.Profile["ig"][rightBorder[2]] + indent] != rightBorder[1])
                    {
                        indent++;
                    }

                    Matrix.Data["ggl"][Matrix.Profile["ig"][rightBorder[2]] + indent] +=
                        hZ * boundaryConditions.Beta
                           * _localMatrixForThirdBoundaryCondition[2, 1];
                    Matrix.Data["ggu"][Matrix.Profile["ig"][rightBorder[2]] + indent] +=
                        hZ * boundaryConditions.Beta
                           * _localMatrixForThirdBoundaryCondition[2, 1];

                    RhsVec[rightBorder[0]] +=
                        hZ * boundaryConditions.Beta *
                        (4.0 * borderPartToInsert[0] + 2.0 * borderPartToInsert[1] - borderPartToInsert[2]) /
                        30.0;

                    RhsVec[rightBorder[1]] +=
                        hZ * boundaryConditions.Beta *
                        (2.0 * borderPartToInsert[0] + 16.0 * borderPartToInsert[1] + 2.0 * borderPartToInsert[2]) /
                        30.0;

                    RhsVec[rightBorder[2]] +=
                        hZ * boundaryConditions.Beta *
                        (-1.0 * borderPartToInsert[0] + 2.0 * borderPartToInsert[1] + 4.0 * borderPartToInsert[2]) /
                        30.0;
                }

                break;
        }

        switch (boundaryConditions.LowerType)
        {
            case "Second":
                for (var i = 0; i < numberOfFiniteElementsAtAxisZ; i++)
                {
                    var lowerBorder = new[]
                    {
                        _finiteElements[i].FictiveNumeration[0],
                        _finiteElements[i].FictiveNumeration[1],
                        _finiteElements[i].FictiveNumeration[2]
                    };

                    var hR =
                    (
                        _finiteElements[i].Nodes[1].r -
                        _finiteElements[i].Nodes[0].r
                    ) / 2.0;

                    var lowerBorderPoints = new[]
                    {
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[i].Nodes[0].r,
                            _finiteElements[i].Nodes[0].z
                        ),
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[i].Nodes[0].r + hR,
                            _finiteElements[i].Nodes[0].z
                        ),
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[i].Nodes[1].r,
                            _finiteElements[i].Nodes[1].z
                        )
                    };

                    var derivativeValues = new[]
                    {
                        _evalBoundaryFuncLeftAt(lowerBorderPoints[0]),
                        _evalBoundaryFuncLeftAt(lowerBorderPoints[1]),
                        _evalBoundaryFuncLeftAt(lowerBorderPoints[2])
                    };

                    RhsVec[lowerBorder[0]] += hR / 30.0 *
                                              (
                                                  4.0 * derivativeValues[0]
                                                  + 2.0 * derivativeValues[1]
                                                  - 1.0 * derivativeValues[2]
                                              );

                    RhsVec[lowerBorder[1]] += hR / 30.0 *
                                              (
                                                  2 * derivativeValues[0]
                                                  + 16 * derivativeValues[1]
                                                  + 2 * derivativeValues[2]
                                              );

                    RhsVec[lowerBorder[2]] += hR / 30.0 *
                                              (
                                                  -1 * derivativeValues[0]
                                                  + 2 * derivativeValues[1]
                                                  + 4 * derivativeValues[2]
                                              );
                }

                break;
            case "Third":
                throw new NotImplementedException();
                break;
        }

        switch (boundaryConditions.UpperType)
        {
            case "Second":
                for (var i = 0; i < numberOfFiniteElementsAtAxisZ; i++)
                {
                    var numberOfCurrentFiniteElement =
                        (numberOfFiniteElementsAtAxisZ - 1) * numberOfFiniteElementsAtAxisR + i;

                    var upperBorder = new[]
                    {
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[6],
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[7],
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[8]
                    };

                    var hR =
                    (
                        _finiteElements[numberOfCurrentFiniteElement].Nodes[3].z -
                        _finiteElements[numberOfCurrentFiniteElement].Nodes[2].z
                    ) / 2.0;

                    var upperBorderPoints = new[]
                    {
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[2].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[2].z
                        ),
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[2].r + hR,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[2].z
                        ),
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[3].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[3].z
                        )
                    };

                    var derivativeValues = new[]
                    {
                        _evalBoundaryFuncLeftAt(upperBorderPoints[0]),
                        _evalBoundaryFuncLeftAt(upperBorderPoints[1]),
                        _evalBoundaryFuncLeftAt(upperBorderPoints[2])
                    };

                    RhsVec[upperBorder[0]] += hR / 30.0 *
                                              (
                                                  4.0 * derivativeValues[0]
                                                  + 2.0 * derivativeValues[1]
                                                  - 1.0 * derivativeValues[2]
                                              );

                    RhsVec[upperBorder[1]] += hR / 30.0 *
                                              (
                                                  2 * derivativeValues[0]
                                                  + 16 * derivativeValues[1]
                                                  + 2 * derivativeValues[2]
                                              );

                    RhsVec[upperBorder[2]] += hR / 30.0 *
                                              (
                                                  -1 * derivativeValues[0]
                                                  + 2 * derivativeValues[1]
                                                  + 4 * derivativeValues[2]
                                              );
                }

                break;
            case "Third":
                throw new NotImplementedException();
                break;
        }

        ApplyFirstBoundaryConditions
        (
            boundaryConditions,
            mesh,
            numberOfFiniteElementsAtAxisZ, numberOfFiniteElementsAtAxisR
        );
    }

    private void ApplyFirstBoundaryConditions
    (
        DataModels.Conditions.Boundary.TwoDim boundaryConditions,
        Mesh.Cylindrical.TwoDim mesh,
        int numberOfFiniteElementsAtAxisZ, int numberOfFiniteElementsAtAxisR
    )
    {
        const double bigConst = 1e+10;

        switch (boundaryConditions.LeftType)
        {
            case "First":
                for (var i = 0; i < numberOfFiniteElementsAtAxisZ; i++)
                {
                    var numberOfCurrentFiniteElement = i * numberOfFiniteElementsAtAxisR;

                    var leftBorder = new[]
                    {
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[0],
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[3],
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[6]
                    };

                    Matrix.Data["di"][leftBorder[0]] = bigConst;
                    Matrix.Data["di"][leftBorder[1]] = bigConst;
                    Matrix.Data["di"][leftBorder[2]] = bigConst;

                    // Nullify(leftBorder[0], mesh);
                    // Nullify(leftBorder[1], mesh);
                    // Nullify(leftBorder[2], mesh);

                    var hZ =
                    (
                        _finiteElements[numberOfCurrentFiniteElement].Nodes[2].z -
                        _finiteElements[numberOfCurrentFiniteElement].Nodes[0].z
                    ) / 2.0;

                    var leftBorderPoints = new[]
                    {
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[0].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[0].z
                        ),
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[0].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[0].z + hZ
                        ),
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[2].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[2].z
                        )
                    };

                    RhsVec[leftBorder[0]] = bigConst * _evalBoundaryFuncLeftAt(leftBorderPoints[0]);
                    RhsVec[leftBorder[1]] = bigConst * _evalBoundaryFuncLeftAt(leftBorderPoints[1]);
                    RhsVec[leftBorder[2]] = bigConst * _evalBoundaryFuncLeftAt(leftBorderPoints[2]);
                }

                break;
        }

        switch (boundaryConditions.RightType)
        {
            case "First":
                for (var i = 0; i < numberOfFiniteElementsAtAxisZ; i++)
                {
                    var numberOfCurrentFiniteElement =
                        i * numberOfFiniteElementsAtAxisR + numberOfFiniteElementsAtAxisR - 1;

                    var rightBorder = new[]
                    {
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[2],
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[5],
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[8]
                    };

                    Matrix.Data["di"][rightBorder[0]] = bigConst;
                    Matrix.Data["di"][rightBorder[1]] = bigConst;
                    Matrix.Data["di"][rightBorder[2]] = bigConst;

                    // Nullify(rightBorder[0], mesh);
                    // Nullify(rightBorder[1], mesh);
                    // Nullify(rightBorder[2], mesh);

                    var hZ =
                    (
                        _finiteElements[numberOfCurrentFiniteElement].Nodes[3].z -
                        _finiteElements[numberOfCurrentFiniteElement].Nodes[1].z
                    ) / 2.0;

                    var rightBorderPoints = new[]
                    {
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[1].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[1].z
                        ),
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[1].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[1].z + hZ
                        ),
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[3].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[3].z
                        )
                    };

                    RhsVec[rightBorder[0]] = bigConst * _evalBoundaryFuncRightAt(rightBorderPoints[0]);
                    RhsVec[rightBorder[1]] = bigConst * _evalBoundaryFuncRightAt(rightBorderPoints[1]);
                    RhsVec[rightBorder[2]] = bigConst * _evalBoundaryFuncRightAt(rightBorderPoints[2]);
                }

                break;
        }

        switch (boundaryConditions.LowerType)
        {
            case "First":
                for (var i = 0; i < numberOfFiniteElementsAtAxisR; i++)
                {
                    var lowerBorder = new[]
                    {
                        _finiteElements[i].FictiveNumeration[0],
                        _finiteElements[i].FictiveNumeration[1],
                        _finiteElements[i].FictiveNumeration[2]
                    };

                    Matrix.Data["di"][lowerBorder[0]] = bigConst;
                    Matrix.Data["di"][lowerBorder[1]] = bigConst;
                    Matrix.Data["di"][lowerBorder[2]] = bigConst;

                    // Nullify(lowerBorder[0], mesh);
                    // Nullify(lowerBorder[1], mesh);
                    // Nullify(lowerBorder[2], mesh);

                    var hR =
                    (
                        _finiteElements[i].Nodes[1].r -
                        _finiteElements[i].Nodes[0].r
                    ) / 2.0;

                    var lowerBorderPoints = new[]
                    {
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[i].Nodes[0].r,
                            _finiteElements[i].Nodes[0].z
                        ),
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[i].Nodes[0].r + hR,
                            _finiteElements[i].Nodes[0].z
                        ),
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[i].Nodes[1].r,
                            _finiteElements[i].Nodes[1].z
                        )
                    };

                    RhsVec[lowerBorder[0]] = bigConst * _evalBoundaryFuncLowerAt(lowerBorderPoints[0]);
                    RhsVec[lowerBorder[1]] = bigConst * _evalBoundaryFuncLowerAt(lowerBorderPoints[1]);
                    RhsVec[lowerBorder[2]] = bigConst * _evalBoundaryFuncLowerAt(lowerBorderPoints[2]);
                }

                break;
        }

        switch (boundaryConditions.UpperType)
        {
            case "First":
                for (var i = 0; i < numberOfFiniteElementsAtAxisZ; i++)
                {
                    var numberOfCurrentFiniteElement =
                        (numberOfFiniteElementsAtAxisZ - 1) * numberOfFiniteElementsAtAxisR + i;

                    var upperBorder = new[]
                    {
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[6],
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[7],
                        _finiteElements[numberOfCurrentFiniteElement].FictiveNumeration[8]
                    };

                    Matrix.Data["di"][upperBorder[0]] = bigConst;
                    Matrix.Data["di"][upperBorder[1]] = bigConst;
                    Matrix.Data["di"][upperBorder[2]] = bigConst;

                    // Nullify(upperBorder[0], mesh);
                    // Nullify(upperBorder[1], mesh);
                    // Nullify(upperBorder[2], mesh);

                    var hR =
                    (
                        _finiteElements[numberOfCurrentFiniteElement].Nodes[3].r -
                        _finiteElements[numberOfCurrentFiniteElement].Nodes[2].r
                    ) / 2.0;

                    var upperBorderPoints = new[]
                    {
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[2].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[2].z
                        ),
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[2].r + hR,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[2].z
                        ),
                        Utils.MakeDict2DCylindrical
                        (
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[3].r,
                            _finiteElements[numberOfCurrentFiniteElement].Nodes[3].z
                        )
                    };

                    RhsVec[upperBorder[0]] = bigConst * _evalBoundaryFuncUpperAt(upperBorderPoints[0]);
                    RhsVec[upperBorder[1]] = bigConst * _evalBoundaryFuncUpperAt(upperBorderPoints[1]);
                    RhsVec[upperBorder[2]] = bigConst * _evalBoundaryFuncUpperAt(upperBorderPoints[2]);
                }

                break;
        }
    }

    private void Nullify(int node, Mesh.Cylindrical.TwoDim mesh)
    {
        for (var i = Matrix.Profile["ig"][node]; i < Matrix.Profile["ig"][node + 1]; i++)
        {
            Matrix.Data["ggl"][i] = 0.0;
        }

        for (var i = node; i < (2 * mesh.R.Length - 1) * (2 * mesh.Z.Length - 1); i++)
        {
            var curNode = Matrix.Profile["jg"][Matrix.Profile["ig"][i]];
            var k = 0;

            while (curNode <= node && k < Matrix.Profile["ig"][i + 1] - Matrix.Profile["ig"][i])
            {
                if (curNode == node)
                {
                    Matrix.Data["ggu"][Matrix.Profile["jg"][i] + k] = 0.0;
                }

                k++;

                if (k < Matrix.Profile["ig"][i + 1] - Matrix.Profile["ig"][i])
                {
                    curNode = Matrix.Profile["jg"][Matrix.Profile["ig"][i] + k];
                }
            }
        }
    }

    private (IMatrix Matrix, double[] ResVec, double[] RhsVec) GetPortraitFrom(Mesh.Cylindrical.TwoDim mesh)
    {
        _boundsList = GetBoundList(mesh);
        var di = new double[(2 * mesh.R.Length - 1) * (2 * mesh.Z.Length - 1)];
        var ig = new int[di.Length + 1];

        ig[0] = 0;
        ig[1] = 0;

        var jg = new List<int>();

        for (var i = 0; i < di.Length; i++)
        {
            var k = 0;

            for (var j = 0; j < _boundsList[i].Count; j++, k++)
            {
                jg.Add(_boundsList[i].ElementAt(j));
            }

            ig[i + 1] = ig[i] + k;
        }

        var ggl = new double[jg.Count];
        var ggu = new double[jg.Count];

        var matrixPortrait = new Sparse(ggl, ggu, di, ig, jg.ToArray());
        var rhsPortrait = new double[di.Length];
        var resPortrait = new double[di.Length];

        return (matrixPortrait, resPortrait, rhsPortrait);
    }

    private SortedSet<int>[] GetBoundList(Mesh.Cylindrical.TwoDim mesh)
    {
        var boundsList = new SortedSet<int>[(2 * mesh.R.Length - 1) * (2 * mesh.R.Length - 1)];

        for (var i = 0; i < boundsList.Length; i++)
        {
            boundsList[i] = new SortedSet<int>();
        }

        foreach (var finiteElement in _finiteElements)
        {
            MapBoundsIn(finiteElement, boundsList);
        }

        return boundsList;
    }

    private static void MapBoundsIn(FiniteElement finiteElement, SortedSet<int>[] boundsList)
    {
        for (var j = 0; j < 9; j++)
        {
            for (var k = 0; k < 9; k++)
            {
                TryAddToBoundaryList(finiteElement, boundsList, j, k);
            }
        }
    }

    private static void TryAddToBoundaryList
    (
        FiniteElement finiteElement,
        SortedSet<int>[] boundsList,
        int j,
        int k
    )
    {
        if (finiteElement.FictiveNumeration[j] > finiteElement.FictiveNumeration[k])
        {
            boundsList[finiteElement.FictiveNumeration[j]].Add(finiteElement.FictiveNumeration[k]);
        }
    }

    private FiniteElement[] GetFiniteElementsFrom(Mesh.Cylindrical.TwoDim mesh)
    {
        var finiteElements = new FiniteElement[(mesh.R.Length - 1) * (mesh.Z.Length - 1)];
        GetNodesFor(finiteElements, mesh);
        GetFictiveNumerationFor(finiteElements, mesh);
        return finiteElements;
    }

    private static void GetNodesFor(FiniteElement[] finiteElements, Mesh.Cylindrical.TwoDim mesh)
    {
        var k = 0;
        var startPosition = 0;
        var lineLenght = mesh.R.Length;

        for (var i = 0; i < mesh.Z.Length - 1; i++, startPosition = i * lineLenght)
        {
            for (var j = 0; j < lineLenght - 1; j++, startPosition += 1, k++)
            {
                var localNodes = GetLocalNodesFrom(mesh, startPosition, lineLenght);
                finiteElements[k] = new FiniteElement(localNodes);
            }
        }
    }

    private static void GetFictiveNumerationFor(FiniteElement[] finiteElements, Mesh.Cylindrical.TwoDim mesh)
    {
        var k = 0;
        var startPosition = 0;
        var lineLenght = mesh.R.Length * 2 - 1;

        for (var i = 0; i < mesh.Z.Length - 1; i++, startPosition = 2 * i * lineLenght)
        {
            for (var j = 0; j < mesh.R.Length - 1; j++, startPosition += 2, k++)
            {
                var fictiveNumeration = new[]
                {
                    startPosition,
                    startPosition + 1,
                    startPosition + 2,

                    startPosition + lineLenght,
                    startPosition + lineLenght + 1,
                    startPosition + lineLenght + 2,

                    startPosition + 2 * lineLenght,
                    startPosition + 2 * lineLenght + 1,
                    startPosition + 2 * lineLenght + 2
                };
                fictiveNumeration.AsSpan().CopyTo(finiteElements[k].FictiveNumeration);
            }
        }
    }

    private static (int, double, double)[] GetLocalNodesFrom
    (
        Mesh.Cylindrical.TwoDim mesh,
        int startPosition,
        int lineLenght
    )
    {
        return new[]
        {
            (
                startPosition,
                mesh.Nodes[startPosition].Coordinates[Mesh.Axis.R],
                mesh.Nodes[startPosition].Coordinates[Mesh.Axis.Z]
            ),
            (
                startPosition + 1,
                mesh.Nodes[startPosition + 1].Coordinates[Mesh.Axis.R],
                mesh.Nodes[startPosition + 1].Coordinates[Mesh.Axis.Z]
            ),

            (
                startPosition + lineLenght,
                mesh.Nodes[startPosition + lineLenght].Coordinates[Mesh.Axis.R],
                mesh.Nodes[startPosition + lineLenght].Coordinates[Mesh.Axis.Z]
            ),
            (
                startPosition + lineLenght + 1,
                mesh.Nodes[startPosition + lineLenght + 1].Coordinates[Mesh.Axis.R],
                mesh.Nodes[startPosition + lineLenght + 1].Coordinates[Mesh.Axis.Z]
            )
        };
    }

    private void AssemblyGlobally
    (
        DataModels.Area.TwoDim area,
        Mesh.Cylindrical.TwoDim mesh
    )
    {
        double sigma = 0;
        var locB = new double[9];
        var localStiffness = new double[][] { };
        var localMasses = new double[][] { };
        var locStiffness = new double[][][] { };

        for (var i = 0; i < _finiteElements.Length; i++)
        {
            var startPos = _finiteElements[i].Nodes[0].r;
            var hR = _finiteElements[i].Nodes[1].r - startPos;
            var hZ = _finiteElements[i].Nodes[2].z - _finiteElements[i].Nodes[0].z;

            var chi = EvalAverageValueOn(_finiteElements[i], _evalChiFuncAt);

            AssemblyLocalMatrices(startPos, hR, hZ, ref localMasses, ref locStiffness);
            AssemblySigma(i, ref sigma, mesh, 0);
            AssemblyRhs(ref locB, i, mesh, 0);
            Multiply(ref locB, localMasses);

            for (var k = 0; k < 9; k++)
            {
                RhsVec[_finiteElements[i].FictiveNumeration[k]] += locB[k];
            }

            AssemblyStiffness(i, mesh, ref localStiffness, locStiffness, 0);

            for (var k = 0; k < 9; k++)
            {
                _stiffnesses.Data["di"][_finiteElements[i].FictiveNumeration[k]] += localStiffness[k][k];

                _masses.Data["di"][_finiteElements[i].FictiveNumeration[k]] += localMasses[k][k];
                _massesSigma.Data["di"][_finiteElements[i].FictiveNumeration[k]] += sigma * localMasses[k][k];
                _massesChi.Data["di"][_finiteElements[i].FictiveNumeration[k]] += chi * localMasses[k][k];
            }

            for (var k = 1; k < 9; k++)
            {
                for (var j = 0; j < k; j++)
                {
                    int ind;

                    for (ind = Matrix.Profile["ig"][_finiteElements[i].FictiveNumeration[k]];
                         Matrix.Profile["jg"][ind] != _finiteElements[i].FictiveNumeration[j];)
                    {
                        ind++;
                    }

                    _stiffnesses.Data["ggl"][ind] += localStiffness[k][j];

                    _masses.Data["ggl"][ind] += localMasses[k][j];
                    _massesSigma.Data["ggl"][ind] += sigma * localMasses[k][j];
                    _massesChi.Data["ggl"][ind] += chi * localMasses[k][j];
                }
            }
        }

        _stiffnesses.Data["ggl"].AsSpan().CopyTo(_stiffnesses.Data["ggu"]);

        _masses.Data["ggl"].AsSpan().CopyTo(_masses.Data["ggu"]);
        _massesSigma.Data["ggl"].AsSpan().CopyTo(_massesSigma.Data["ggu"]);
        _massesChi.Data["ggl"].AsSpan().CopyTo(_massesChi.Data["ggu"]);
    }

    private void AssemblyStiffness
    (
        int num,
        Mesh.Cylindrical.TwoDim mesh,
        ref double[][] localStiffness,
        double[][][] locStiffness,
        double time
    )
    {
        var localPhiLambda = new double[4];
        EvalLocalLambda(num, ref localPhiLambda, mesh, time);
        localStiffness = new double[9][];

        for (var i = 0; i < 9; i++)
        {
            localStiffness[i] = new double[9];

            for (var j = 0; j < 9; j++)
            {
                localStiffness[i][j] = localPhiLambda[0] * locStiffness[0][i][j]
                                       + localPhiLambda[1] * locStiffness[1][i][j]
                                       + localPhiLambda[2] * locStiffness[2][i][j]
                                       + localPhiLambda[3] * locStiffness[3][i][j];
            }
        }
    }

    private void EvalLocalLambda
    (
        int num,
        ref double[] localPhiLambda,
        Mesh.Cylindrical.TwoDim mesh,
        double time
    )
    {
        for (var i = 0; i < 4; i++)
        {
            var point = Utils.MakeDict2DCylindricalTime
            (
                _finiteElements[num].Nodes[i].r,
                _finiteElements[num].Nodes[i].z,
                time
            );
            localPhiLambda[i] = _evalLambdaFuncAt(point);
        }
    }

    private static void Multiply(ref double[] f, double[][] locMass)
    {
        var res = new double[9];

        for (var i = 0; i < 9; i++)
        {
            var sum = 0.0;

            for (var j = 0; j < 9; j++)
            {
                sum += f[i] * locMass[i][j];
            }

            res[i] = sum;
        }

        f = res;
    }

    private void AssemblyRhs
    (
        ref double[] localRhs,
        int num,
        Mesh.Cylindrical.TwoDim mesh,
        double time
    )
    {
        var hR = (_finiteElements[num].Nodes[1].r - _finiteElements[num].Nodes[0].r) / 2.0;
        var hZ = (_finiteElements[num].Nodes[2].z - _finiteElements[num].Nodes[0].z) / 2.0;
        var res = new double[9];

        for (var i = 0; i < 9; i++)
        {
            var point = Utils.MakeDict2DCylindricalTime
            (
                _finiteElements[num].Nodes[0].r + hR * (i % 3),
                _finiteElements[num].Nodes[1].z + hZ * (i / 3),
                time
            );
            res[i] = _evalRhsFuncAt(point);
        }

        localRhs = res;
    }

    private void loc_sigma(int num, ref double sigma, Mesh.Cylindrical.TwoDim mesh, double time)
    {
        double g = 0;
        var hR = (_finiteElements[num].Nodes[1].r - _finiteElements[num].Nodes[0].r) / 2.0;
        var hZ = (_finiteElements[num].Nodes[2].z - _finiteElements[num].Nodes[0].z) / 2.0;

        for (var i = 0; i < 9; i++)
        {
            var point = Utils.MakeDict2DCylindricalTime
            (
                _finiteElements[num].Nodes[0].r + hR * (i % 3),
                _finiteElements[num].Nodes[1].z + hZ * (i / 3),
                time
            );

            // TODO: Parse new func
            g += _evalSigmaFuncAt(point);
        }

        sigma = g / 9.0;
    }

    private void AssemblySigma
    (
        int num,
        ref double gamma,
        Mesh.Cylindrical.TwoDim mesh,
        double time
    )
    {
        double g = 0;
        var hR = (_finiteElements[num].Nodes[1].r - _finiteElements[num].Nodes[0].r) / 2.0;
        var hZ = (_finiteElements[num].Nodes[2].z - _finiteElements[num].Nodes[0].z) / 2.0;

        for (var i = 0; i < 9; i++)
        {
            var point = Utils.MakeDict2DCylindricalTime
            (
                _finiteElements[num].Nodes[0].r + hR * (i % 3),
                _finiteElements[num].Nodes[1].z + hZ * (i / 3),
                time
            );

            g += _evalGammaFuncAt(point);
        }

        gamma = g / 9.0;
    }

    private static void AssemblyLocalMatrices
    (
        double startPos,
        double hR, double hZ,
        ref double[][] localMasses,
        ref double[][][] localStiffnesses
    )
    {
        var localMassesIntegral = new double[3][][];
        localMassesIntegral[0] = new double[3][];
        localMassesIntegral[0][0] = new double[3];
        localMassesIntegral[0][1] = new double[3];
        localMassesIntegral[0][2] = new double[3];
        localMassesIntegral[0][0][0] = 2.0 / 15.0;
        localMassesIntegral[0][0][1] = 1.0 / 15.0;
        localMassesIntegral[0][0][2] = -1.0 / 30.0;
        localMassesIntegral[0][1][0] = 1.0 / 15.0;
        localMassesIntegral[0][1][1] = 8.0 / 15.0;
        localMassesIntegral[0][1][2] = 1.0 / 15.0;
        localMassesIntegral[0][2][0] = -1.0 / 30.0;
        localMassesIntegral[0][2][1] = 1.0 / 15.0;
        localMassesIntegral[0][2][2] = 2.0 / 15.0;

        localMassesIntegral[1] = new double[3][];
        localMassesIntegral[1][0] = new double[3];
        localMassesIntegral[1][1] = new double[3];
        localMassesIntegral[1][2] = new double[3];
        localMassesIntegral[1][0][0] = 1.0 / 60.0;
        localMassesIntegral[1][0][1] = 0.0;
        localMassesIntegral[1][0][2] = -1.0 / 60.0;
        localMassesIntegral[1][1][0] = 0.0;
        localMassesIntegral[1][1][1] = 4.0 / 15.0;
        localMassesIntegral[1][1][2] = 1.0 / 15.0;
        localMassesIntegral[1][2][0] = -1.0 / 60.0;
        localMassesIntegral[1][2][1] = 1.0 / 15.0;
        localMassesIntegral[1][2][2] = 7.0 / 60.0;

        localMassesIntegral[2] = new double[3][];
        localMassesIntegral[2][0] = new double[3];
        localMassesIntegral[2][1] = new double[3];
        localMassesIntegral[2][2] = new double[3];
        localMassesIntegral[2][0][0] = 7.0 / 60.0;
        localMassesIntegral[2][0][1] = 1.0 / 15.0;
        localMassesIntegral[2][0][2] = -1.0 / 60.0;
        localMassesIntegral[2][1][0] = 1.0 / 15.0;
        localMassesIntegral[2][1][1] = 4.0 / 15.0;
        localMassesIntegral[2][1][2] = 0.0;
        localMassesIntegral[2][2][0] = -1.0 / 60.0;
        localMassesIntegral[2][2][1] = 0.0;
        localMassesIntegral[2][2][2] = 1.0 / 60.0;

        var localMassesIntegralsWithJacobian = new double[3][][];
        localMassesIntegralsWithJacobian[0] = new double[3][];
        localMassesIntegralsWithJacobian[0][0] = new double[3];
        localMassesIntegralsWithJacobian[0][1] = new double[3];
        localMassesIntegralsWithJacobian[0][2] = new double[3];
        localMassesIntegralsWithJacobian[0][0][0] = 1.0 / 60.0;
        localMassesIntegralsWithJacobian[0][0][1] = 0.0;
        localMassesIntegralsWithJacobian[0][0][2] = -1.0 / 60.0;
        localMassesIntegralsWithJacobian[0][1][0] = 0.0;
        localMassesIntegralsWithJacobian[0][1][1] = 4.0 / 15.0;
        localMassesIntegralsWithJacobian[0][1][2] = 1.0 / 15.0;
        localMassesIntegralsWithJacobian[0][2][0] = -1.0 / 60.0;
        localMassesIntegralsWithJacobian[0][2][1] = 1.0 / 15.0;
        localMassesIntegralsWithJacobian[0][2][2] = 7.0 / 60.0;

        localMassesIntegralsWithJacobian[1] = new double[3][];
        localMassesIntegralsWithJacobian[1][0] = new double[3];
        localMassesIntegralsWithJacobian[1][1] = new double[3];
        localMassesIntegralsWithJacobian[1][2] = new double[3];
        localMassesIntegralsWithJacobian[1][0][0] = 1.0 / 210.0;
        localMassesIntegralsWithJacobian[1][0][1] = -1.0 / 105.0;
        localMassesIntegralsWithJacobian[1][0][2] = -1.0 / 84.0;
        localMassesIntegralsWithJacobian[1][1][0] = -1.0 / 105.0;
        localMassesIntegralsWithJacobian[1][1][1] = 16.0 / 105.0;
        localMassesIntegralsWithJacobian[1][1][2] = 2.0 / 35.0;
        localMassesIntegralsWithJacobian[1][2][0] = -1.0 / 84.0;
        localMassesIntegralsWithJacobian[1][2][1] = 2.0 / 35.0;
        localMassesIntegralsWithJacobian[1][2][2] = 11.0 / 105.0;

        localMassesIntegralsWithJacobian[2] = new double[3][];
        localMassesIntegralsWithJacobian[2][0] = new double[3];
        localMassesIntegralsWithJacobian[2][1] = new double[3];
        localMassesIntegralsWithJacobian[2][2] = new double[3];
        localMassesIntegralsWithJacobian[2][0][0] = 1.0 / 84.0;
        localMassesIntegralsWithJacobian[2][0][1] = 1.0 / 105.0;
        localMassesIntegralsWithJacobian[2][0][2] = -1.0 / 210.0;
        localMassesIntegralsWithJacobian[2][1][0] = 1.0 / 105.0;
        localMassesIntegralsWithJacobian[2][1][1] = 23.0 / 140.0;
        localMassesIntegralsWithJacobian[2][1][2] = -17.0 / 420.0;
        localMassesIntegralsWithJacobian[2][2][0] = -1.0 / 210.0;
        localMassesIntegralsWithJacobian[2][2][1] = -17.0 / 420.0;
        localMassesIntegralsWithJacobian[2][2][2] = 1.0 / 84.0;

        var localStiffnessesIntegrals = new double[3][][];

        for (var i = 0; i < 3; i++)
        {
            localStiffnessesIntegrals[i] = new double[3][];
        }

        localStiffnessesIntegrals[0][0] = new double[3];
        localStiffnessesIntegrals[0][1] = new double[3];
        localStiffnessesIntegrals[0][2] = new double[3];
        localStiffnessesIntegrals[0][0][0] = 7.0 / 3.0;
        localStiffnessesIntegrals[0][0][1] = -8.0 / 3.0;
        localStiffnessesIntegrals[0][0][2] = 1.0 / 3.0;
        localStiffnessesIntegrals[0][1][0] = -8.0 / 3.0;
        localStiffnessesIntegrals[0][1][1] = 16.0 / 3.0;
        localStiffnessesIntegrals[0][1][2] = -8.0 / 3.0;
        localStiffnessesIntegrals[0][2][0] = 1.0 / 3.0;
        localStiffnessesIntegrals[0][2][1] = -8.0 / 3.0;
        localStiffnessesIntegrals[0][2][2] = 7.0 / 3.0;

        localStiffnessesIntegrals[1][0] = new double[3];
        localStiffnessesIntegrals[1][1] = new double[3];
        localStiffnessesIntegrals[1][2] = new double[3];
        localStiffnessesIntegrals[1][0][0] = 1.0 / 2.0;
        localStiffnessesIntegrals[1][0][1] = -2.0 / 3.0;
        localStiffnessesIntegrals[1][0][2] = 1.0 / 6.0;
        localStiffnessesIntegrals[1][1][0] = -2.0 / 3.0;
        localStiffnessesIntegrals[1][1][1] = 8.0 / 3.0;
        localStiffnessesIntegrals[1][1][2] = -2.0;
        localStiffnessesIntegrals[1][2][0] = 1.0 / 6.0;
        localStiffnessesIntegrals[1][2][1] = -2.0;
        localStiffnessesIntegrals[1][2][2] = 11.0 / 6.0;

        localStiffnessesIntegrals[2][0] = new double[3];
        localStiffnessesIntegrals[2][1] = new double[3];
        localStiffnessesIntegrals[2][2] = new double[3];
        localStiffnessesIntegrals[2][0][0] = 11.0 / 6.0;
        localStiffnessesIntegrals[2][0][1] = -2.0;
        localStiffnessesIntegrals[2][0][2] = 1.0 / 6.0;
        localStiffnessesIntegrals[2][1][0] = -2.0;
        localStiffnessesIntegrals[2][1][1] = 8.0 / 3.0;
        localStiffnessesIntegrals[2][1][2] = -2.0 / 3.0;
        localStiffnessesIntegrals[2][2][0] = 1.0 / 6.0;
        localStiffnessesIntegrals[2][2][1] = -2.0 / 3.0;
        localStiffnessesIntegrals[2][2][2] = 1.0 / 2.0;

        var localStiffnessesIntegralsWithJacobian = new double[3][][];

        for (var i = 0; i < 3; i++)
        {
            localStiffnessesIntegralsWithJacobian[i] = new double[3][];
        }

        localStiffnessesIntegralsWithJacobian[0][0] = new double[3];
        localStiffnessesIntegralsWithJacobian[0][1] = new double[3];
        localStiffnessesIntegralsWithJacobian[0][2] = new double[3];
        localStiffnessesIntegralsWithJacobian[0][0][0] = 1.0 / 2.0;
        localStiffnessesIntegralsWithJacobian[0][0][1] = -2.0 / 3.0;
        localStiffnessesIntegralsWithJacobian[0][0][2] = 1.0 / 6.0;
        localStiffnessesIntegralsWithJacobian[0][1][0] = -2.0 / 3.0;
        localStiffnessesIntegralsWithJacobian[0][1][1] = 8.0 / 3.0;
        localStiffnessesIntegralsWithJacobian[0][1][2] = -2.0;
        localStiffnessesIntegralsWithJacobian[0][2][0] = 1.0 / 6.0;
        localStiffnessesIntegralsWithJacobian[0][2][1] = -2.0;
        localStiffnessesIntegralsWithJacobian[0][2][2] = 11.0 / 6.0;

        localStiffnessesIntegralsWithJacobian[1][0] = new double[3];
        localStiffnessesIntegralsWithJacobian[1][1] = new double[3];
        localStiffnessesIntegralsWithJacobian[1][2] = new double[3];
        localStiffnessesIntegralsWithJacobian[1][0][0] = 1.0 / 5.0;
        localStiffnessesIntegralsWithJacobian[1][0][1] = -2.0 / 5.0;
        localStiffnessesIntegralsWithJacobian[1][0][2] = 1.0 / 5.0;
        localStiffnessesIntegralsWithJacobian[1][1][0] = -2.0 / 5.0;
        localStiffnessesIntegralsWithJacobian[1][1][1] = 32.0 / 15.0;
        localStiffnessesIntegralsWithJacobian[1][1][2] = -26.0 / 15.0;
        localStiffnessesIntegralsWithJacobian[1][2][0] = 1.0 / 5.0;
        localStiffnessesIntegralsWithJacobian[1][2][1] = -26.0 / 15.0;
        localStiffnessesIntegralsWithJacobian[1][2][2] = 23.0 / 15.0;

        localStiffnessesIntegralsWithJacobian[2][0] = new double[3];
        localStiffnessesIntegralsWithJacobian[2][1] = new double[3];
        localStiffnessesIntegralsWithJacobian[2][2] = new double[3];
        localStiffnessesIntegralsWithJacobian[2][0][0] = 3.0 / 10.0;
        localStiffnessesIntegralsWithJacobian[2][0][1] = -4.0 / 15.0;
        localStiffnessesIntegralsWithJacobian[2][0][2] = -1.0 / 30.0;
        localStiffnessesIntegralsWithJacobian[2][1][0] = -4.0 / 15.0;
        localStiffnessesIntegralsWithJacobian[2][1][1] = 8.0 / 15.0;
        localStiffnessesIntegralsWithJacobian[2][1][2] = -4.0 / 15.0;
        localStiffnessesIntegralsWithJacobian[2][2][0] = -1.0 / 30.0;
        localStiffnessesIntegralsWithJacobian[2][2][1] = -4.0 / 15.0;
        localStiffnessesIntegralsWithJacobian[2][2][2] = 3.0 / 10.0;

        localMasses = new double[9][];

        for (var i = 0; i < 9; i++)
        {
            localMasses[i] = new double[9];

            for (var j = 0; j < 9; j++)
            {
                localMasses[i][j] = hR * hZ *
                                    (
                                        hR * localMassesIntegralsWithJacobian[0][i % 3][j % 3] +
                                        startPos * localMassesIntegral[0][i % 3][j % 3]
                                    ) *
                                    localMassesIntegral[0][i / 3][j / 3];
            }
        }

        localStiffnesses = new double[4][][];

        for (var k = 0; k < 4; k++)
        {
            localStiffnesses[k] = new double[9][];
        }

        for (var i = 0; i < 9; i++)
        {
            localStiffnesses[0][i] = new double[9];
            localStiffnesses[1][i] = new double[9];
            localStiffnesses[2][i] = new double[9];
            localStiffnesses[3][i] = new double[9];

            for (var j = 0; j < 9; j++)
            {
                localStiffnesses[0][i][j] = hZ *
                                            (
                                                hR * localStiffnessesIntegralsWithJacobian[1][i % 3][j % 3] +
                                                startPos * localStiffnessesIntegrals[1][i % 3][j % 3]
                                            ) *
                                            localMassesIntegral[1][i / 3][j / 3] / hR +
                                            hR *
                                            (
                                                hR * localMassesIntegralsWithJacobian[1][i % 3][j % 3] +
                                                startPos * localMassesIntegral[1][i % 3][j % 3]
                                            )
                                            * localStiffnessesIntegrals[1][i / 3][j / 3] / hZ;

                localStiffnesses[1][i][j] = hZ *
                                            (
                                                hR * localStiffnessesIntegralsWithJacobian[2][i % 3][j % 3] +
                                                startPos * localStiffnessesIntegrals[2][i % 3][j % 3]
                                            )
                                            * localMassesIntegral[1][i / 3][j / 3] / hR
                                            + hR *
                                            (
                                                hR * localMassesIntegralsWithJacobian[2][i % 3][j % 3] +
                                                startPos * localMassesIntegral[2][i % 3][j % 3]
                                            )
                                            * localStiffnessesIntegrals[1][i / 3][j / 3] / hZ;

                localStiffnesses[2][i][j] = hZ *
                                            (
                                                hR * localStiffnessesIntegralsWithJacobian[1][i % 3][j % 3] +
                                                startPos * localStiffnessesIntegrals[1][i % 3][j % 3]
                                            )
                                            * localMassesIntegral[2][i / 3][j / 3] / hR
                                            + hR *
                                            (
                                                hR * localMassesIntegralsWithJacobian[1][i % 3][j % 3] +
                                                startPos * localMassesIntegral[1][i % 3][j % 3]
                                            )
                                            * localStiffnessesIntegrals[2][i / 3][j / 3] / hZ;

                localStiffnesses[3][i][j] = hZ *
                                            (
                                                hR * localStiffnessesIntegralsWithJacobian[2][i % 3][j % 3] +
                                                startPos * localStiffnessesIntegrals[2][i % 3][j % 3]
                                            )
                                            * localMassesIntegral[2][i / 3][j / 3] / hR
                                            + hR *
                                            (
                                                hR * localMassesIntegralsWithJacobian[2][i % 3][j % 3] +
                                                startPos * localMassesIntegral[2][i % 3][j % 3]
                                            )
                                            * localStiffnessesIntegrals[2][i / 3][j / 3] / hZ;
            }
        }
    }

    private static double[,] AssemblyLocalMatrixFrom(double[,] localMass, double[,] localStiffness)
    {
        var localMatrix = new double[9, 9];

        for (var i = 0; i < localMatrix.GetLength(1); i++)
        {
            for (var j = 0; j < localMatrix.GetLength(0); j++)
            {
                localMatrix[i, j] = localMass[i, j] + localStiffness[i, j];
            }
        }

        return localMatrix;
    }

    private void AddToGlobal(FiniteElement finiteElement, double[] localB)
    {
        foreach (var globalNumber in finiteElement.FictiveNumeration)
        {
            for (var k = 0; k < 9; k++)
            {
                RhsVec[globalNumber] += localB[k];
            }
        }
    }

    private void AddToGlobal(FiniteElement finiteElement, double[,] localMatrix)
    {
        for (var i = 0; i < localMatrix.GetLength(1); i++)
        {
            for (var j = 0; j < localMatrix.GetLength(0); j++)
            {
                var globalI = finiteElement.FictiveNumeration[i];
                var globalJ = finiteElement.FictiveNumeration[j];
                AddToGlobalAtPos(globalI, globalJ, localMatrix[i, j]);
            }
        }
    }

    private void AddToGlobalAtPos(int i, int j, double a)
    {
        if (i == j)
        {
            Matrix.Data["di"][i] += a;
        }

        if (i < j)
        {
            for (var ind = Matrix.Profile["ig"][j]; ind < Matrix.Profile["ig"][j + 1]; ind++)
            {
                if (Matrix.Profile["jg"][ind] == i)
                {
                    Matrix.Data["ggu"][ind] += a;
                    break;
                }
            }
        }
        else
        {
            for (var ind = Matrix.Profile["ig"][j]; ind < Matrix.Profile["ig"][j + 1]; ind++)
            {
                if (Matrix.Profile["jg"][ind] == j)
                {
                    Matrix.Data["ggl"][ind] += a;
                    break;
                }
            }
        }
    }

    private double EvalAverageValueOn
    (
        FiniteElement finiteElement,
        Func<Dictionary<string, double>, double> func
    )
    {
        var points = GetAllPointsIn(finiteElement);

        return points.Sum(func) / 9.0;
    }

    private static Dictionary<string, double>[] GetAllPointsIn(FiniteElement finiteElement)
    {
        var hR = (finiteElement.Nodes[1].r - finiteElement.Nodes[0].r) / 2.0;
        var hZ = (finiteElement.Nodes[1].z - finiteElement.Nodes[0].z) / 2.0;
        var points = new[]
        {
            Utils.MakeDict2DCylindrical(finiteElement.Nodes[0].r, finiteElement.Nodes[0].z),
            Utils.MakeDict2DCylindrical(finiteElement.Nodes[0].r + hR, finiteElement.Nodes[0].z),
            Utils.MakeDict2DCylindrical(finiteElement.Nodes[1].r, finiteElement.Nodes[0].z),

            Utils.MakeDict2DCylindrical(finiteElement.Nodes[0].r, finiteElement.Nodes[0].z + hZ),
            Utils.MakeDict2DCylindrical(finiteElement.Nodes[0].r + hR, finiteElement.Nodes[0].z + hZ),
            Utils.MakeDict2DCylindrical(finiteElement.Nodes[1].r, finiteElement.Nodes[0].z + hZ),

            Utils.MakeDict2DCylindrical(finiteElement.Nodes[0].r, finiteElement.Nodes[1].z),
            Utils.MakeDict2DCylindrical(finiteElement.Nodes[0].r + hR, finiteElement.Nodes[1].z),
            Utils.MakeDict2DCylindrical(finiteElement.Nodes[0].r, finiteElement.Nodes[1].z),
        };
        return points;
    }

    private static Dictionary<string, double>[] GetAllPointsIn(FiniteElement finiteElement, double time)
    {
        var points = GetAllPointsIn(finiteElement);

        foreach (var point in points)
        {
            point.Add("t", time);
        }

        return points;
    }

    private class FiniteElement
    {
        public readonly (int globalNumber, double r, double z)[] Nodes;

        public int[] FictiveNumeration { get; } = new int[9];

        public FiniteElement((int, double, double)[] nodes)
        {
            Nodes = nodes;
        }
    }
}