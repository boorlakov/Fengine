using Fengine.Backend.DataModels;
using Fengine.Backend.Differentiation;
using Fengine.Backend.Fem.Mesh;
using Fengine.Backend.Integration;
using Fengine.Backend.LinearAlgebra.Matrix;
using Fengine.Backend.LinearAlgebra.SlaeSolver;
using Sprache.Calc;

namespace Fengine.Backend.Fem.Slae.NonlinearTask.Elliptic.OneDim;

public class Linear : ISlae
{
    private readonly IIntegrator _integrator;
    private readonly ISlaeSolver _slaeSolver;
    private readonly IDerivative? _derivative;

    public IMatrix Matrix { get; set; }
    public IMatrix NonLinearMatrix { get; set; }

    public double[] RhsVec { get; set; }
    public double[] NonLinearRhsVec { get; set; }

    public double[] ResVec { get; set; }

    public Linear()
    {
        Matrix = null!;
        ResVec = null!;
        RhsVec = null!;
        _integrator = null!;
        _slaeSolver = null!;
        _derivative = null!;
    }

    public Linear
    (
        Mesh.Cartesian.OneDim mesh,
        InputFuncs inputFuncs,
        double[] initApprox,
        ISlaeSolver slaeSolver,
        IIntegrator integrator,
        IMatrix matrix,
        IDerivative? derivative = null
    )
    {
        _slaeSolver = slaeSolver;
        _integrator = integrator;
        Matrix = matrix;

        _derivative = derivative;
        var withLinearization = _derivative is not null;

        ResVec = new double[mesh.Nodes.Length];
        initApprox.AsSpan().CopyTo(ResVec);
        RhsVec = new double[mesh.Nodes.Length];

        var localStiffness = BuildLocalStiffness();
        var localMass = BuildLocalMass();

        var upper = new double[mesh.Nodes.Length - 1];
        var center = new double[mesh.Nodes.Length];
        var lower = new double[mesh.Nodes.Length - 1];

        var funcCalc = new XtensibleCalculator();
        var evalRhsFunc = funcCalc.ParseFunction(inputFuncs.RhsFunc).Compile();
        var evalLambda = funcCalc.ParseFunction(inputFuncs.Lambda).Compile();
        var evalGamma = funcCalc.ParseFunction(inputFuncs.Gamma).Compile();

        for (var i = 0; i < mesh.Nodes.Length - 1; i++)
        {
            var step = mesh.Nodes[i + 1].Coordinates[Axis.X] - mesh.Nodes[i].Coordinates[Axis.X];

            BuildMatrix(i, step, mesh, localStiffness, localMass, evalLambda, evalGamma, upper, center, lower);

            BuildRhs(i, step, mesh, evalRhsFunc, localMass);
        }

        NonLinearMatrix = new ThreeDiagonal(upper, center, lower);
        NonLinearRhsVec = new double[RhsVec.Length];
        RhsVec.AsSpan().CopyTo(NonLinearRhsVec);

        if (withLinearization)
        {
            Linearize(mesh, localStiffness, localMass, evalRhsFunc, upper, center, lower);
        }

        Matrix = new ThreeDiagonal(upper, center, lower);
    }

    private void Linearize
    (
        Mesh.Cartesian.OneDim mesh,
        double[][][] localStiffness, double[][][] localMass,
        Func<Dictionary<string, double>, double> evalRhsFunc,
        double[] upper, double[] center, double[] lower
    )
    {
        for (var i = 0; i < mesh.Nodes.Length - 1; i++)
        {
            var step = mesh.Nodes[i + 1].Coordinates[Axis.X] - mesh.Nodes[i].Coordinates[Axis.X];

            var locNewton = new double[2][];
            locNewton[0] = new double[2];
            locNewton[1] = new double[2];
            locNewton[0][0] = step * localMass[2][0][0] *
                              _derivative!.FindFirst2DAt2Point
                              (
                                  evalRhsFunc,
                                  mesh.Nodes[i].Coordinates[Axis.X],
                                  ResVec[i],
                                  1e-7
                              );

            locNewton[0][1] = step * localMass[2][0][1] *
                              _derivative.FindFirst2DAt2Point
                              (
                                  evalRhsFunc,
                                  mesh.Nodes[i + 1].Coordinates[Axis.X],
                                  ResVec[i],
                                  1e-7
                              );

            locNewton[1][0] = step * localMass[2][1][0] *
                              _derivative.FindFirst2DAt2Point
                              (
                                  evalRhsFunc,
                                  mesh.Nodes[i].Coordinates[Axis.X],
                                  ResVec[i],
                                  1e-7
                              );

            locNewton[1][1] = step * localMass[2][1][0] *
                              _derivative.FindFirst2DAt2Point
                              (
                                  evalRhsFunc,
                                  mesh.Nodes[i + 1].Coordinates[Axis.X],
                                  ResVec[i],
                                  1e-7
                              );

            center[i] -= locNewton[0][0];
            center[i + 1] -= locNewton[1][1];
            upper[i] -= locNewton[0][1];
            lower[i] -= locNewton[1][0];

            RhsVec[i] -= ResVec[i] * locNewton[0][0] + ResVec[i + 1] * locNewton[0][1];
            RhsVec[i + 1] -= ResVec[i] * locNewton[1][0] + ResVec[i + 1] * locNewton[1][1];
        }
    }

    private static void BuildMatrix(
        int i,
        double step,
        IMesh mesh,
        double[][][] localStiffness,
        double[][][] localMass,
        Func<Dictionary<string, double>, double> evalLambda,
        Func<Dictionary<string, double>, double> evalGamma,
        double[] upper,
        double[] center,
        double[] lower
    )
    {
        var point = Utils.MakeDict1D(mesh.Nodes[i].Coordinates[Axis.X]);
        var nextPoint = Utils.MakeDict1D(mesh.Nodes[i + 1].Coordinates[Axis.X]);

        center[i] +=
            (evalLambda(point) * localStiffness[0][0][0] + evalLambda(nextPoint) * localStiffness[1][0][0]) /
            step + (evalGamma(point) * localMass[0][0][0] + evalGamma(nextPoint) * localMass[1][0][0]) * step;

        center[i + 1] +=
            (evalLambda(point) * localStiffness[0][1][1] + evalLambda(nextPoint) * localStiffness[1][1][1]) / step +
            (evalGamma(point) * localMass[0][1][1] + evalGamma(nextPoint) * localMass[1][1][1]) * step;

        upper[i] +=
            (evalLambda(point) * localStiffness[0][0][1] + evalLambda(nextPoint) * localStiffness[1][0][1]) / step +
            (evalGamma(point) * localMass[0][0][1] + evalGamma(nextPoint) * localMass[1][0][1]) * step;

        lower[i] +=
            (evalLambda(point) * localStiffness[0][1][0] + evalGamma(nextPoint) * localStiffness[1][1][0]) / step +
            (evalGamma(point) * localMass[0][1][0] + evalGamma(nextPoint) * localMass[1][1][0]) * step;
    }

    private void BuildRhs
    (
        int i,
        double step,
        IMesh mesh,
        Func<Dictionary<string, double>, double> evalRhsFunc,
        double[][][] localMass
    )
    {
        var point = Utils.MakeDict2DCartesian(mesh.Nodes[i].Coordinates[Axis.X], ResVec[i]);
        var nextPoint = Utils.MakeDict2DCartesian(mesh.Nodes[i + 1].Coordinates[Axis.X], ResVec[i + 1]);

        RhsVec[i] += step * (evalRhsFunc(point) * localMass[2][0][0] + evalRhsFunc(nextPoint) * localMass[2][0][1]);
        RhsVec[i + 1] += step * (evalRhsFunc(point) * localMass[2][1][0] + evalRhsFunc(nextPoint) * localMass[2][1][1]);
    }

    public Linear
    (
        IMatrix matrix,
        double[] rhsVec,
        ISlaeSolver slaeSolver,
        IIntegrator integrator,
        IDerivative? derivative = null
    )
    {
        Matrix = matrix;
        ResVec = new double[rhsVec.Length];
        RhsVec = rhsVec;
        _slaeSolver = slaeSolver;
        _integrator = integrator;
        _derivative = derivative;
    }

    public double[] Solve(Accuracy accuracy)
    {
        return _slaeSolver.Solve(this, accuracy);
    }

    private double[][][] BuildLocalStiffness()
    {
        var mesh = Utils.Create1DIntegrationMesh(0.0, 1.0);

        var localStiffness = new double[2][][];
        localStiffness[0] = new double[2][];
        localStiffness[1] = new double[2][];

        var integralValues = new[]
        {
            _integrator.Integrate1D(mesh, Basis.LinearBasis.Func[0]),
            _integrator.Integrate1D(mesh, Basis.LinearBasis.Func[1])
        };

        for (var i = 0; i < 2; i++)
        {
            localStiffness[0][i] = new double[2];
            localStiffness[1][i] = new double[2];

            for (var j = 0; j < 2; j++)
            {
                if (i == j)
                {
                    localStiffness[0][i][j] = integralValues[0];
                    localStiffness[1][i][j] = integralValues[1];
                }
                else
                {
                    localStiffness[0][i][j] = -integralValues[0];
                    localStiffness[1][i][j] = -integralValues[1];
                }
            }
        }

        return localStiffness;
    }

    private double[][][] BuildLocalMass()
    {
        var mesh = Utils.Create1DIntegrationMesh(0.0, 1.0);

        var localMass = new double[3][][];
        localMass[0] = new double[2][];
        localMass[1] = new double[2][];
        localMass[2] = new double[2][];

        var integralValues = new[]
        {
            _integrator.Integrate1D(mesh,
                x => Basis.LinearBasis.Func[0](x) * Basis.LinearBasis.Func[0](x) * Basis.LinearBasis.Func[0](x)),
            _integrator.Integrate1D(mesh,
                x => Basis.LinearBasis.Func[0](x) * Basis.LinearBasis.Func[0](x) * Basis.LinearBasis.Func[1](x)),
            _integrator.Integrate1D(mesh,
                x => Basis.LinearBasis.Func[0](x) * Basis.LinearBasis.Func[1](x) * Basis.LinearBasis.Func[1](x)),
            _integrator.Integrate1D(mesh,
                x => Basis.LinearBasis.Func[1](x) * Basis.LinearBasis.Func[1](x) * Basis.LinearBasis.Func[1](x)),

            _integrator.Integrate1D(mesh, x => Basis.LinearBasis.Func[0](x) * Basis.LinearBasis.Func[0](x)),
            _integrator.Integrate1D(mesh, x => Basis.LinearBasis.Func[0](x) * Basis.LinearBasis.Func[1](x)),
            _integrator.Integrate1D(mesh, x => Basis.LinearBasis.Func[1](x) * Basis.LinearBasis.Func[1](x))
        };

        for (var i = 0; i < 2; i++)
        {
            localMass[0][i] = new double[2];
            localMass[1][i] = new double[2];
            localMass[2][i] = new double[2];

            for (var j = 0; j <= i; j++)
            {
                if (i == j)
                {
                    localMass[0][i][j] = integralValues[2 * i];
                    localMass[1][i][j] = integralValues[2 * i + 1];
                    localMass[2][i][j] = integralValues[4 + 2 * i];
                }
                else
                {
                    localMass[0][i][j] = integralValues[i];
                    localMass[0][j][i] = localMass[0][i][j];

                    localMass[1][i][j] = integralValues[2 * i];
                    localMass[1][j][i] = localMass[1][i][j];

                    localMass[2][i][j] = integralValues[4 + i];
                    localMass[2][j][i] = localMass[2][i][j];
                }
            }
        }

        return localMass;
    }
}