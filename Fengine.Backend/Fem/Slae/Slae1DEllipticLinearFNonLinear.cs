using Fengine.Backend.Fem.Basis;
using Fengine.Backend.Fem.Mesh;
using Fengine.Backend.Integration;
using Fengine.Backend.LinAlg.Matrix;
using Fengine.Backend.LinAlg.SlaeSolver;
using Fengine.Backend.Models;
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
        var rhsFunc = funcCalc.ParseFunction(inputFuncs.RhsFunc).Compile();
        var lambda = funcCalc.ParseFunction(inputFuncs.Lambda).Compile();
        var gamma = funcCalc.ParseFunction(inputFuncs.Gamma).Compile();

        for (var i = 0; i < mesh.Nodes.Length - 1; i++)
        {
            var step = mesh.Nodes[i + 1].Coordinates[IMesh.Axis.X] - mesh.Nodes[i].Coordinates[IMesh.Axis.X];

            BuildMatrix(i, step, mesh, localStiffness, localMass, lambda, gamma, upper, center, lower);

            BuildRhs(i, step, mesh, rhsFunc, localMass);
        }

        Matrix = new Matrix3Diagonal(upper, center, lower);
    }

    private static void BuildMatrix(int i,
        double step,
        IMesh cartesian1DMesh,
        double[][][] localStiffness,
        double[][][] localMass,
        Func<Dictionary<string, double>, double> lambda,
        Func<Dictionary<string, double>, double> gamma,
        double[] upper,
        double[] center,
        double[] lower
    )
    {
        center[i] +=
            (lambda(Utils.MakeDict1D(cartesian1DMesh.Nodes[i].Coordinates[IMesh.Axis.X])) * localStiffness[0][0][0] +
             lambda(Utils.MakeDict1D(cartesian1DMesh.Nodes[i + 1].Coordinates[IMesh.Axis.X])) *
             localStiffness[1][0][0]) /
            step +
            (gamma(Utils.MakeDict1D(cartesian1DMesh.Nodes[i].Coordinates[IMesh.Axis.X])) * localMass[0][0][0] +
             gamma(Utils.MakeDict1D(cartesian1DMesh.Nodes[i + 1].Coordinates[IMesh.Axis.X])) * localMass[1][0][0]) *
            step;

        center[i + 1] += (lambda(Utils.MakeDict1D(cartesian1DMesh.Nodes[i].Coordinates[IMesh.Axis.X])) *
                          localStiffness[0][1][1] +
                          lambda(Utils.MakeDict1D(cartesian1DMesh.Nodes[i + 1].Coordinates[IMesh.Axis.X])) *
                          localStiffness[1][1][1]) / step +
                         (gamma(Utils.MakeDict1D(cartesian1DMesh.Nodes[i].Coordinates[IMesh.Axis.X])) *
                          localMass[0][1][1] +
                          gamma(Utils.MakeDict1D(cartesian1DMesh.Nodes[i + 1].Coordinates[IMesh.Axis.X])) *
                          localMass[1][1][1]) * step;

        upper[i] += (lambda(Utils.MakeDict1D(cartesian1DMesh.Nodes[i].Coordinates[IMesh.Axis.X])) *
                     localStiffness[0][0][1] +
                     lambda(Utils.MakeDict1D(cartesian1DMesh.Nodes[i + 1].Coordinates[IMesh.Axis.X])) *
                     localStiffness[1][0][1]) / step +
                    (gamma(Utils.MakeDict1D(cartesian1DMesh.Nodes[i].Coordinates[IMesh.Axis.X])) * localMass[0][0][1] +
                     gamma(Utils.MakeDict1D(cartesian1DMesh.Nodes[i + 1].Coordinates[IMesh.Axis.X])) *
                     localMass[1][0][1]) *
                    step;

        lower[i] += (lambda(Utils.MakeDict1D(cartesian1DMesh.Nodes[i].Coordinates[IMesh.Axis.X])) *
                     localStiffness[0][1][0] +
                     gamma(Utils.MakeDict1D(cartesian1DMesh.Nodes[i + 1].Coordinates[IMesh.Axis.X])) *
                     localStiffness[1][1][0]) / step +
                    (gamma(Utils.MakeDict1D(cartesian1DMesh.Nodes[i].Coordinates[IMesh.Axis.X])) * localMass[0][1][0] +
                     gamma(Utils.MakeDict1D(cartesian1DMesh.Nodes[i + 1].Coordinates[IMesh.Axis.X])) *
                     localMass[1][1][0]) *
                    step;
    }

    private void BuildRhs(int i,
        double step,
        IMesh cartesian1DMesh,
        Func<Dictionary<string, double>, double> rhsFunc,
        double[][][] localMass
    )
    {
        RhsVec[i] += step * (rhsFunc(Utils.MakeDict2D(cartesian1DMesh.Nodes[i].Coordinates[IMesh.Axis.X], ResVec[i])) *
                             localMass[2][0][0] +
                             rhsFunc(Utils.MakeDict2D(cartesian1DMesh.Nodes[i + 1].Coordinates[IMesh.Axis.X],
                                 ResVec[i + 1])) * localMass[2][0][1]);
        RhsVec[i + 1] += step *
                         (rhsFunc(Utils.MakeDict2D(cartesian1DMesh.Nodes[i].Coordinates[IMesh.Axis.X], ResVec[i])) *
                          localMass[2][1][0] +
                          rhsFunc(Utils.MakeDict2D(cartesian1DMesh.Nodes[i + 1].Coordinates[IMesh.Axis.X],
                              ResVec[i + 1])) * localMass[2][1][1]);
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