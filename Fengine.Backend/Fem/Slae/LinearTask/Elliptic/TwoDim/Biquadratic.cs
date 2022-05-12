using Fengine.Backend.DataModels;
using Fengine.Backend.Fem.Basis;
using Fengine.Backend.Integration;
using Fengine.Backend.LinearAlgebra.Matrix;
using Sprache.Calc;

namespace Fengine.Backend.Fem.Slae.LinearTask.Elliptic.TwoDim;

public class Biquadratic : ISlae
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

    private double[,] _localMatrixForThirdBoundaryCondition =
    {
        {2.0 / 15.0, 1.0 / 15.0, -1.0 / 30.0},
        {1.0 / 15.0, 8.0 / 15.0, 1.0 / 15.0},
        {-1.0 / 30.0, 1.0 / 15.0, 2.0 / 15.0}
    };

    private IIntegrator _integrator;
    private LinearAlgebra.SlaeSolver.ISlaeSolver _slaeSolver;

    public Biquadratic(IMatrix matrix, double[] rhsVec, double[] resVec)
    {
        Matrix = matrix;
        RhsVec = rhsVec;
        ResVec = resVec;
    }

    public Biquadratic(IMatrix matrix, double[] rhsVec)
    {
        Matrix = matrix;
        RhsVec = rhsVec;
        ResVec = new double[rhsVec.Length];
    }

    public Biquadratic
    (
        DataModels.Area.TwoDim area,
        Mesh.Cylindrical.TwoDim mesh,
        InputFuncs inputFuncs,
        DataModels.Conditions.Boundary.TwoDim boundaryConditions,
        LinearAlgebra.SlaeSolver.ISlaeSolver slaeSolver,
        IIntegrator integrator
    )
    {
        ConfigureServices(slaeSolver, integrator);

        CompileInputFunctions(inputFuncs, boundaryConditions);

        _finiteElements = GetFiniteElementsFrom(mesh);

        (Matrix, ResVec, RhsVec) = GetPortraitFrom(mesh);

        AssemblyGlobally(inputFuncs);

        var bim = 9;
        ApplyBoundaryConditions(boundaryConditions, area, mesh);
    }

    private void CompileInputFunctions(InputFuncs inputFuncs, DataModels.Conditions.Boundary.TwoDim boundaryConditions)
    {
        var calculator = new XtensibleCalculator();
        _evalRhsFuncAt = calculator.ParseFunction(inputFuncs.RhsFunc).Compile();
        _evalLambdaFuncAt = calculator.ParseFunction(inputFuncs.Lambda).Compile();
        _evalGammaFuncAt = calculator.ParseFunction(inputFuncs.Gamma).Compile();
        _evalBoundaryFuncLeftAt = calculator.ParseFunction(boundaryConditions.LeftFunc).Compile();
        _evalBoundaryFuncRightAt = calculator.ParseFunction(boundaryConditions.RightFunc).Compile();
        _evalBoundaryFuncLowerAt = calculator.ParseFunction(boundaryConditions.LowerFunc).Compile();
        _evalBoundaryFuncUpperAt = calculator.ParseFunction(boundaryConditions.UpperFunc).Compile();
    }

    public double[] Solve(Accuracy accuracy)
    {
        return _slaeSolver.Solve(this, accuracy);
    }

    public IMatrix Matrix { get; set; }
    public double[] RhsVec { get; set; }
    public double[] ResVec { get; set; }

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

                    Matrix.Data["di"][leftBorder[0]] = 1.0;
                    Matrix.Data["di"][leftBorder[1]] = 1.0;
                    Matrix.Data["di"][leftBorder[2]] = 1.0;

                    Nullify(leftBorder[0], mesh);
                    Nullify(leftBorder[1], mesh);
                    Nullify(leftBorder[2], mesh);

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

                    RhsVec[leftBorder[0]] = _evalBoundaryFuncLeftAt(leftBorderPoints[0]);
                    RhsVec[leftBorder[1]] = _evalBoundaryFuncLeftAt(leftBorderPoints[1]);
                    RhsVec[leftBorder[2]] = _evalBoundaryFuncLeftAt(leftBorderPoints[2]);
                }

                break;
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

                    RhsVec[leftBorder[0]] += _evalBoundaryFuncLeftAt(leftBorderPoints[0]);
                    RhsVec[leftBorder[1]] += _evalBoundaryFuncLeftAt(leftBorderPoints[1]);
                    RhsVec[leftBorder[2]] += _evalBoundaryFuncLeftAt(leftBorderPoints[2]);
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
            case "First":
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

                    Matrix.Data["di"][rightBorder[0]] = 1.0;
                    Matrix.Data["di"][rightBorder[1]] = 1.0;
                    Matrix.Data["di"][rightBorder[2]] = 1.0;

                    Nullify(rightBorder[0], mesh);
                    Nullify(rightBorder[1], mesh);
                    Nullify(rightBorder[2], mesh);

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

                    RhsVec[rightBorder[0]] = _evalBoundaryFuncRightAt(rightBorderPoints[0]);
                    RhsVec[rightBorder[1]] = _evalBoundaryFuncRightAt(rightBorderPoints[1]);
                    RhsVec[rightBorder[2]] = _evalBoundaryFuncRightAt(rightBorderPoints[2]);
                }

                break;
            case "Second":
                throw new NotImplementedException();
                break;
            case "Third":
                throw new NotImplementedException();
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

                    Matrix.Data["di"][lowerBorder[0]] = 1.0;
                    Matrix.Data["di"][lowerBorder[1]] = 1.0;
                    Matrix.Data["di"][lowerBorder[2]] = 1.0;

                    Nullify(lowerBorder[0], mesh);
                    Nullify(lowerBorder[1], mesh);
                    Nullify(lowerBorder[2], mesh);

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

                    RhsVec[lowerBorder[0]] = _evalBoundaryFuncLowerAt(lowerBorderPoints[0]);
                    RhsVec[lowerBorder[1]] = _evalBoundaryFuncLowerAt(lowerBorderPoints[1]);
                    RhsVec[lowerBorder[2]] = _evalBoundaryFuncLowerAt(lowerBorderPoints[2]);
                }

                break;
            case "Second":
                throw new NotImplementedException();
                break;
            case "Third":
                throw new NotImplementedException();
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

                    Matrix.Data["di"][upperBorder[0]] = 1.0;
                    Matrix.Data["di"][upperBorder[1]] = 1.0;
                    Matrix.Data["di"][upperBorder[2]] = 1.0;

                    Nullify(upperBorder[0], mesh);
                    Nullify(upperBorder[1], mesh);
                    Nullify(upperBorder[2], mesh);

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

                    RhsVec[upperBorder[0]] = _evalBoundaryFuncUpperAt(upperBorderPoints[0]);
                    RhsVec[upperBorder[1]] = _evalBoundaryFuncUpperAt(upperBorderPoints[1]);
                    RhsVec[upperBorder[2]] = _evalBoundaryFuncUpperAt(upperBorderPoints[2]);
                }

                break;
            case "Second":
                throw new NotImplementedException();
                break;
            case "Third":
                throw new NotImplementedException();
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

    private void AssemblyGlobally(InputFuncs inputFunctions)
    {
        foreach (var finiteElement in _finiteElements)
        {
            AssemblyLocally(finiteElement, inputFunctions);
        }
    }

    private void AssemblyLocally
    (
        FiniteElement finiteElement,
        InputFuncs inputFuncs
    )
    {
        var points = GetAllPointsIn(finiteElement);
        var localLambdaValue = GetLocalLambdaValueFrom(finiteElement);

        var hR = finiteElement.Nodes[1].r - finiteElement.Nodes[0].r;
        var hZ = finiteElement.Nodes[2].z - finiteElement.Nodes[0].z;

        var localStiffness = new double[9, 9];
        var localC = new double[9, 9];
        var localMass = new double[9, 9];

        var localF = new double[9];

        var localNumber = 0;

        var integralStiffnessValues = GetIntegralStiffnessValues();
        var integralMassValues = GetIntegralMassValues();
        var integralCValues = GetIntegralCValues();

        foreach (var point in points)
        {
            localF[localNumber] = _evalRhsFuncAt(point);
            localNumber++;

            for (var i = 0; i < 9; i++)
            {
                for (var j = 0; j < 9; j++)
                {
                    localStiffness[i, j] += hR * hZ * localLambdaValue * integralStiffnessValues[i, j];

                    for (var k = 0; k < 4; k++)
                    {
                        localMass[i, j] += _evalGammaFuncAt(point) * integralMassValues[i, j];
                    }

                    localMass[i, j] *= hR * hZ;
                    localC[i, j] += hR * hZ * integralCValues[i, j];
                }
            }
        }

        var localMatrix = AssemblyLocalMatrixFrom(localMass, localStiffness);
        var localB = LinearAlgebra.GeneralOperations.MatrixMultiply(localC, localF);

        AddToGlobal(finiteElement, localMatrix);
        AddToGlobal(finiteElement, localB);
    }

    private double[,] GetIntegralCValues()
    {
        var integrationMesh = Utils.Create1DIntegrationMesh(0, 1);
        var integralCValues = new double[9, 9];

        for (var i = 0; i < 9; i++)
        {
            for (var j = 0; j < 9; j++)
            {
                var indXi = i % 3;
                var indYi = i / 3;

                var indXj = j % 3;
                var indYj = j / 3;

                var cIntegrand = (double r, double z) =>
                    r *
                    QuadraticBasis.Func[indXi](r) *
                    QuadraticBasis.Func[indYi](z) *
                    QuadraticBasis.Func[indXj](r) *
                    QuadraticBasis.Func[indYj](z);

                integralCValues[i, j] += _integrator.Integrate2D(integrationMesh, cIntegrand);
            }
        }

        return integralCValues;
    }

    private double[,] GetIntegralMassValues()
    {
        var integrationMesh = Utils.Create1DIntegrationMesh(0, 1);
        var integralMassValues = new double[9, 9];

        for (var i = 0; i < 9; i++)
        {
            for (var j = 0; j < 9; j++)
            {
                var indXi = i % 3;
                var indYi = i / 3;

                var indXj = j % 3;
                var indYj = j / 3;

                var basisPartMassIntegrand = (double x, double y) =>
                    QuadraticBasis.Func[indXi](x) *
                    QuadraticBasis.Func[indYi](y) *
                    QuadraticBasis.Func[indXj](x) *
                    QuadraticBasis.Func[indYj](y);

                for (var k = 0; k < 4; k++)
                {
                    var k1 = k;
                    integralMassValues[i, j] += _integrator.Integrate2D
                    (
                        integrationMesh,
                        (r, z) => r * basisPartMassIntegrand(r, z) * BilinearBasis.Func[k1](r, z)
                    );
                }
            }
        }

        return integralMassValues;
    }

    private double[,] GetIntegralStiffnessValues()
    {
        var integrationMesh = Utils.Create1DIntegrationMesh(0, 1);
        var integralStiffnessValues = new double[9, 9];

        for (var i = 0; i < 9; i++)
        {
            for (var j = 0; j < 9; j++)
            {
                var indXi = i % 3;
                var indYi = i / 3;

                var indXj = j % 3;
                var indYj = j / 3;

                var dR = (double r, double z) =>
                    QuadraticBasis.FirstDerivative[indXi](r) * QuadraticBasis.Func[indYi](z) *
                    QuadraticBasis.FirstDerivative[indXj](r) * QuadraticBasis.Func[indYj](z);

                var dZ = (double r, double z) =>
                    QuadraticBasis.Func[indXi](r) * QuadraticBasis.FirstDerivative[indYi](z) *
                    QuadraticBasis.Func[indXj](r) * QuadraticBasis.FirstDerivative[indYj](z);

                var stiffnessIntegrand = (double r, double z) =>
                    r * (dR(r, z) + dZ(r, z));

                integralStiffnessValues[i, j] += _integrator.Integrate2D(integrationMesh, stiffnessIntegrand);
            }
        }

        return integralStiffnessValues;
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
                    break;
                }

                Matrix.Data["ggu"][ind] += a;
            }
        }
        else
        {
            for (var ind = Matrix.Profile["ig"][j]; ind < Matrix.Profile["ig"][j + 1]; ind++)
            {
                if (Matrix.Profile["jg"][ind] == j)
                {
                    break;
                }

                Matrix.Data["ggl"][ind] += a;
            }
        }
    }

    private double GetLocalLambdaValueFrom(FiniteElement finiteElement)
    {
        var points = GetAllPointsIn(finiteElement);

        return points.Sum(point => _evalLambdaFuncAt(point)) / 9.0;
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