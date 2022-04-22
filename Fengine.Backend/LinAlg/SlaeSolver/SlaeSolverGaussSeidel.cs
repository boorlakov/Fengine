using Fengine.Backend.DataModels;
using Fengine.Backend.Fem.Slae;
using Fengine.Backend.LinAlg.Matrix;

namespace Fengine.Backend.LinAlg.SlaeSolver;

/// <summary>
///     Class for holding methods of solving systems of linear equations
/// </summary>
public class SlaeSolverGaussSeidel : ISlaeSolver
{
    /// <summary>
    /// Gauss-Seidel solve method 
    /// </summary>
    /// <param name="slae"></param>
    /// <param name="accuracy"></param>
    /// <returns></returns>
    public double[] Solve(ISlae slae, Accuracy accuracy)
    {
        slae.ResVec = Iterate(slae.ResVec, slae.Matrix, 1.7, slae.RhsVec);
        var residual = Utils.RelResidual(slae.Matrix, slae.ResVec, slae.RhsVec);
        var iter = 1;
        var prevResVec = new double[slae.ResVec.Length];

        while (iter <= accuracy.MaxIter && accuracy.Eps < residual &&
               !Utils.CheckIsStagnate(prevResVec, slae.ResVec, accuracy.Delta))
        {
            slae.ResVec.AsSpan().CopyTo(prevResVec);
            slae.ResVec = Iterate(slae.ResVec, slae.Matrix, 1.0, slae.RhsVec);
            residual = Utils.RelResidual(slae.Matrix, slae.ResVec, slae.RhsVec);
            iter++;
        }

        return slae.ResVec;
    }

    /// <summary>
    ///     1 iteration of Gauss-Seidel iteration method of solving Ax = f
    /// </summary>
    /// <param name="x">Given approximation. x part in slae</param>
    /// <param name="m">Given weights. A part in slae</param>
    /// <param name="w">Relaxation parameter</param>
    /// <param name="f">Right part (f) of the slae</param>
    /// <returns>New approximation x</returns>
    private static double[] Iterate(double[] x, IMatrix m, double w, double[] f)
    {
        for (var i = 0; i < x.Length; i++)
        {
            var sum = GeneralOperations.Dot(i, m, x);
            x[i] += w * (f[i] - sum) / m.Data["center"][i];
        }

        return x;
    }
}