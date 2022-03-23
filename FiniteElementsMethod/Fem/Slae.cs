using FiniteElementsMethod.Integration;
using FiniteElementsMethod.LinAlg;
using FiniteElementsMethod.Models;

namespace FiniteElementsMethod.Fem;

public class Slae
{
    public void Solve(Accuracy accuracy)
    {
        ResVec = SlaeSolver.Iterate(ResVec, Matrix, 1.7, RhsVec);
        var residual = SlaeSolver.RelResidual(Matrix, ResVec, RhsVec);
        var iter = 1;
        var prevResVec = new double[ResVec.Length];

        while (iter <= accuracy.MaxIter && accuracy.Eps < residual &&
               !SlaeSolver.CheckIsStagnate(prevResVec, ResVec, accuracy.Delta))
        {
            ResVec.AsSpan().CopyTo(prevResVec);
            ResVec = SlaeSolver.Iterate(ResVec, Matrix, 1.0, RhsVec);
            residual = SlaeSolver.RelResidual(Matrix, ResVec, RhsVec);
            iter++;
        }
    }

    public Slae(Grid grid, InputFuncs inputFuncs, double[] initApprox)
    {
        ResVec = new double[grid.X.Length];
        initApprox.AsSpan().CopyTo(ResVec);
        RhsVec = new double[grid.X.Length];

        var localStiffness = BuildLocalStiffness();
        var localMass = BuildLocalMass();

        var upper = new double[grid.X.Length - 1];
        var center = new double[grid.X.Length];
        var lower = new double[grid.X.Length - 1];
        var rhsCalc = new Sprache.Calc.XtensibleCalculator();
        var rhsFunc = rhsCalc.ParseFunction(inputFuncs.RhsFunc).Compile();

        for (var i = 0; i < grid.X.Length - 1; i++)
        {
            var step = grid.X[i + 1] - grid.X[i];

            #region matrixBuild

            #region center

            center[i] += (Utils.EvalFunc(inputFuncs.Lambda, grid.X[i]) * localStiffness[0][0][0] +
                          Utils.EvalFunc(inputFuncs.Lambda, grid.X[i + 1]) * localStiffness[1][0][0]) / step +
                         (Utils.EvalFunc(inputFuncs.Gamma, grid.X[i]) * localMass[0][0][0] +
                          Utils.EvalFunc(inputFuncs.Gamma, grid.X[i + 1]) * localMass[1][0][0]) * step;

            center[i + 1] += (Utils.EvalFunc(inputFuncs.Lambda, grid.X[i]) * localStiffness[0][1][1] +
                              Utils.EvalFunc(inputFuncs.Lambda, grid.X[i + 1]) * localStiffness[1][1][1]) / step +
                             (Utils.EvalFunc(inputFuncs.Gamma, grid.X[i]) * localMass[0][1][1] +
                              Utils.EvalFunc(inputFuncs.Gamma, grid.X[i + 1]) * localMass[1][1][1]) * step;

            #endregion

            #region upper

            upper[i] += (Utils.EvalFunc(inputFuncs.Lambda, grid.X[i]) * localStiffness[0][0][1] +
                         Utils.EvalFunc(inputFuncs.Lambda, grid.X[i + 1]) * localStiffness[1][0][1]) / step +
                        (Utils.EvalFunc(inputFuncs.Gamma, grid.X[i]) * localMass[0][0][1] +
                         Utils.EvalFunc(inputFuncs.Gamma, grid.X[i + 1]) * localMass[1][0][1]) * step;

            #endregion

            #region lower

            lower[i] += (Utils.EvalFunc(inputFuncs.Lambda, grid.X[i]) * localStiffness[0][1][0] +
                         Utils.EvalFunc(inputFuncs.Lambda, grid.X[i + 1]) * localStiffness[1][1][0]) / step +
                        (Utils.EvalFunc(inputFuncs.Gamma, grid.X[i]) * localMass[0][1][0] +
                         Utils.EvalFunc(inputFuncs.Gamma, grid.X[i + 1]) * localMass[1][1][0]) * step;

            #endregion

            #endregion

            #region buildRhs

            RhsVec[i] += step * (rhsFunc(Utils.MakeDict2D(grid.X[i], ResVec[i])) * localMass[2][0][0] +
                                 rhsFunc(Utils.MakeDict2D(grid.X[i + 1], ResVec[i + 1])) * localMass[2][0][1]);
            RhsVec[i + 1] += step * (rhsFunc(Utils.MakeDict2D(grid.X[i], ResVec[i])) * localMass[2][1][0] +
                                     rhsFunc(Utils.MakeDict2D(grid.X[i + 1], ResVec[i + 1])) * localMass[2][1][1]);

            #endregion
        }

        Matrix = new Matrix(upper, center, lower);
    }

    public Slae(Matrix matrix, double[] rhsVec)
    {
        Matrix = matrix;
        ResVec = new double[rhsVec.Length];
        RhsVec = rhsVec;
    }

    private static double[][][] BuildLocalStiffness()
    {
        var grid = Integrator.MakeGrid(0.0, 1.0);

        var localStiffness = new double[2][][];
        localStiffness[0] = new double[2][];
        localStiffness[1] = new double[2][];

        var integralValues = new[]
        {
            Integrator.Integrate1D(grid, LinearBasis.Func[0]),
            Integrator.Integrate1D(grid, LinearBasis.Func[1])
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

    private static double[][][] BuildLocalMass()
    {
        var grid = Integrator.MakeGrid(0.0, 1.0);

        var localMass = new double[3][][];
        localMass[0] = new double[2][];
        localMass[1] = new double[2][];
        localMass[2] = new double[2][];

        var integralValues = new[]
        {
            Integrator.Integrate1D(grid, x => LinearBasis.Func[0](x) * LinearBasis.Func[0](x) * LinearBasis.Func[0](x)),
            Integrator.Integrate1D(grid, x => LinearBasis.Func[0](x) * LinearBasis.Func[0](x) * LinearBasis.Func[1](x)),
            Integrator.Integrate1D(grid, x => LinearBasis.Func[0](x) * LinearBasis.Func[1](x) * LinearBasis.Func[1](x)),
            Integrator.Integrate1D(grid, x => LinearBasis.Func[1](x) * LinearBasis.Func[1](x) * LinearBasis.Func[1](x)),
            Integrator.Integrate1D(grid, x => LinearBasis.Func[0](x) * LinearBasis.Func[0](x)),
            Integrator.Integrate1D(grid, x => LinearBasis.Func[0](x) * LinearBasis.Func[1](x)),
            Integrator.Integrate1D(grid, x => LinearBasis.Func[1](x) * LinearBasis.Func[1](x)),
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

    public readonly double[] RhsVec;
    public readonly Matrix Matrix;
    public double[] ResVec;
}