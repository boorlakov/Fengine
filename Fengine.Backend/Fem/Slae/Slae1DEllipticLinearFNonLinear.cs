using Fengine.Backend.DataModels;
using Fengine.Backend.Fem.Basis;
using Fengine.Backend.Fem.Mesh;
using Fengine.Backend.Integration;
using Fengine.Backend.LinAlg.Matrix;
using Fengine.Backend.LinAlg.SlaeSolver;
using Sprache.Calc;

namespace Fengine.Backend.Fem.Slae;

public class Slae1DEllipticLinearFNonLinear : ISlae
{
    private readonly IIntegrator _integrator;
    private readonly ISlaeSolver _slaeSolver;

    public IMatrix Matrix { get; set; }
    public double[] RhsVec { get; set; }
    public double[] ResVec { get; set; }

    public Slae1DEllipticLinearFNonLinear()
    {
        Matrix = null!;
        ResVec = null!;
        RhsVec = null!;
        _integrator = null!;
        _slaeSolver = null!;
    }

    public Slae1DEllipticLinearFNonLinear(
        IMesh mesh,
        InputFuncs inputFuncs,
        double[] initApprox,
        ISlaeSolver slaeSolver,
        IIntegrator integrator,
        IMatrix matrix
    )
    {
        _slaeSolver = slaeSolver;
        _integrator = integrator;
        Matrix = matrix;

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
            var step = mesh.Nodes[i + 1].Coordinates[IMesh.Axis.X] - mesh.Nodes[i].Coordinates[IMesh.Axis.X];

            BuildMatrix(i, step, mesh, localStiffness, localMass, evalLambda, evalGamma, upper, center, lower);

            BuildRhs(i, step, mesh, evalRhsFunc, localMass);
        }

        Matrix = new Matrix3Diagonal(upper, center, lower);
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
        var point = Utils.MakeDict1D(mesh.Nodes[i].Coordinates[IMesh.Axis.X]);
        var nextPoint = Utils.MakeDict1D(mesh.Nodes[i + 1].Coordinates[IMesh.Axis.X]);

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

    private void BuildRhs(
        int i,
        double step,
        IMesh mesh,
        Func<Dictionary<string, double>, double> evalRhsFunc,
        double[][][] localMass
    )
    {
        var point = Utils.MakeDict2D(mesh.Nodes[i].Coordinates[IMesh.Axis.X], ResVec[i]);
        var nextPoint = Utils.MakeDict2D(mesh.Nodes[i + 1].Coordinates[IMesh.Axis.X], ResVec[i + 1]);

        RhsVec[i] += step * (evalRhsFunc(point) * localMass[2][0][0] + evalRhsFunc(nextPoint) * localMass[2][0][1]);
        RhsVec[i + 1] += step * (evalRhsFunc(point) * localMass[2][1][0] + evalRhsFunc(nextPoint) * localMass[2][1][1]);
    }

    public Slae1DEllipticLinearFNonLinear(
        IMatrix matrix,
        double[] rhsVec,
        ISlaeSolver slaeSolver,
        IIntegrator integrator
    )
    {
        Matrix = matrix;
        ResVec = new double[rhsVec.Length];
        RhsVec = rhsVec;
        _slaeSolver = slaeSolver;
        _integrator = integrator;
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
            _integrator.Integrate1D(mesh, LinearBasis.Func[0]),
            _integrator.Integrate1D(mesh, LinearBasis.Func[1])
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
                x => LinearBasis.Func[0](x) * LinearBasis.Func[0](x) * LinearBasis.Func[0](x)),
            _integrator.Integrate1D(mesh,
                x => LinearBasis.Func[0](x) * LinearBasis.Func[0](x) * LinearBasis.Func[1](x)),
            _integrator.Integrate1D(mesh,
                x => LinearBasis.Func[0](x) * LinearBasis.Func[1](x) * LinearBasis.Func[1](x)),
            _integrator.Integrate1D(mesh,
                x => LinearBasis.Func[1](x) * LinearBasis.Func[1](x) * LinearBasis.Func[1](x)),

            _integrator.Integrate1D(mesh, x => LinearBasis.Func[0](x) * LinearBasis.Func[0](x)),
            _integrator.Integrate1D(mesh, x => LinearBasis.Func[0](x) * LinearBasis.Func[1](x)),
            _integrator.Integrate1D(mesh, x => LinearBasis.Func[1](x) * LinearBasis.Func[1](x))
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