using umf_2.Integration;

namespace umf_2;

public class Slae
{
    public void Solve(JsonModels.AccuracyModel accuracy)
    {
        ResVec = LinAlg.SlaeSolver.Iterate(ResVec, Matrix, 1.0, RhsVec);
        var residual = LinAlg.SlaeSolver.RelResidual(Matrix, ResVec, RhsVec);
        var iter = 1;
        var prevResVec = new double[ResVec.Length];

        while (iter <= accuracy.MaxIter && accuracy.Eps < residual &&
               !LinAlg.SlaeSolver.CheckIsStagnate(prevResVec, ResVec, accuracy.Delta))
        {
            ResVec.AsSpan().CopyTo(prevResVec);
            ResVec = LinAlg.SlaeSolver.Iterate(ResVec, Matrix, 1.0, RhsVec);
            residual = LinAlg.SlaeSolver.RelResidual(Matrix, ResVec, RhsVec);
            iter++;
        }
    }

    public Slae(Grid grid, JsonModels.InputFuncsModel inputFuncs, double[] initApprox)
    {
        ResVec = initApprox;

        var localStiffness = BuildLocalStiffness();
        var localMass = BuildLocalMass();

        var upper = new double[grid.X.Length - 1];
        var center = new double[grid.X.Length];
        var lower = new double[grid.X.Length - 1];

        var uK = BuildUk(grid);

        for (var i = 0; i < grid.X.Length - 1; i++)
        {
            var step = grid.X[i + 1] - grid.X[i];

            #region matrixBuild

            #region center

            center[i] += (Utils.EvalFunc(inputFuncs.Lambda!, grid.X[i]) * localStiffness[0][0][0] +
                          Utils.EvalFunc(inputFuncs.Lambda!, grid.X[i + 1]) * localStiffness[1][0][0]) / step +
                         (Utils.EvalFunc(inputFuncs.Gamma!, grid.X[i]) * localMass[0][0][0] +
                          Utils.EvalFunc(inputFuncs.Gamma!, grid.X[i + 1]) * localMass[1][0][0]) * step;

            center[i + 1] += (Utils.EvalFunc(inputFuncs.Lambda!, grid.X[i]) * localStiffness[0][1][1] +
                              Utils.EvalFunc(inputFuncs.Lambda!, grid.X[i + 1]) * localStiffness[1][1][1]) / step +
                             (Utils.EvalFunc(inputFuncs.Gamma!, grid.X[i]) * localMass[0][1][1] +
                              Utils.EvalFunc(inputFuncs.Gamma!, grid.X[i + 1]) * localMass[1][1][1]) * step;

            #endregion

            #region upper

            upper[i] += (Utils.EvalFunc(inputFuncs.Lambda!, grid.X[i]) * localStiffness[0][0][1] +
                         Utils.EvalFunc(inputFuncs.Lambda!, grid.X[i + 1]) * localStiffness[1][0][1]) / step +
                        (Utils.EvalFunc(inputFuncs.Gamma!, grid.X[i]) * localMass[0][0][1] +
                         Utils.EvalFunc(inputFuncs.Gamma!, grid.X[i + 1]) * localMass[1][0][1]) * step;

            #endregion

            #region lower

            lower[i] += (Utils.EvalFunc(inputFuncs.Lambda!, grid.X[i]) * localStiffness[0][1][0] +
                         Utils.EvalFunc(inputFuncs.Lambda!, grid.X[i + 1]) * localStiffness[1][1][0]) / step +
                        (Utils.EvalFunc(inputFuncs.Gamma!, grid.X[i]) * localMass[0][1][0] +
                         Utils.EvalFunc(inputFuncs.Gamma!, grid.X[i + 1]) * localMass[1][1][0]) * step;

            #endregion

            #endregion
        }
    }

    private Func<double, double> BuildUk(Grid grid)
    {
        var uK = (double x) =>
            ResVec[0] * (x - grid.X[0]) / (grid.X[1] - grid.X[0]) +
            ResVec[1] * (grid.X[1] - x) / (grid.X[1] - grid.X[0]);

        for (var i = 1; i < grid.X.Length - 1; i++)
        {
            var toSumFunc = (double x) => uK(x) +
                                          ResVec[i] * (x - grid.X[i]) / (grid.X[i + 1] - grid.X[i]) +
                                          ResVec[i + 1] * (grid.X[i + 1] - x) / (grid.X[i + 1] - grid.X[i]);
            uK = toSumFunc;
        }

        return uK;
    }

    private static double[][][] BuildLocalStiffness()
    {
        var grid = Integrator.Make0To1Grid();

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
        var grid = Integrator.Make0To1Grid();

        var localStiffness = new double[2][][];
        localStiffness[0] = new double[2][];
        localStiffness[1] = new double[2][];

        var integralValues = new[]
        {
            Integrator.Integrate1D(grid, x => LinearBasis.Func[0](x) * LinearBasis.Func[0](x) * LinearBasis.Func[0](x)),
            Integrator.Integrate1D(grid, x => LinearBasis.Func[0](x) * LinearBasis.Func[0](x) * LinearBasis.Func[1](x)),
            Integrator.Integrate1D(grid, x => LinearBasis.Func[0](x) * LinearBasis.Func[1](x) * LinearBasis.Func[1](x)),
            Integrator.Integrate1D(grid, x => LinearBasis.Func[1](x) * LinearBasis.Func[1](x) * LinearBasis.Func[1](x))
        };

        for (var i = 0; i < 2; i++)
        {
            localStiffness[0][i] = new double[2];
            localStiffness[1][i] = new double[2];

            for (var j = 0; j <= i; j++)
            {
                if (i == j)
                {
                    localStiffness[0][i][j] = integralValues[2 * i];
                    localStiffness[1][i][j] = integralValues[2 * i + 1];
                }
                else
                {
                    localStiffness[0][i][j] = integralValues[i];
                    localStiffness[0][j][i] = localStiffness[0][i][j];

                    localStiffness[1][i][j] = integralValues[2 * i];
                    localStiffness[1][j][i] = localStiffness[1][i][j];
                }
            }
        }

        return localStiffness;
    }

    public readonly double[] RhsVec;
    public readonly Matrix Matrix;
    public double[] ResVec;
}