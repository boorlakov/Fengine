using Fengine.Backend.Fem.Slae;
using Fengine.Backend.LinAlg.Matrix;

namespace Fengine.Backend.LinAlg;

public static class Utils
{
    /// <summary>
    ///     Checks if iteration method is stagnating or not
    /// </summary>
    /// <param name="prevVec">Previous value of a vector</param>
    /// <param name="vec">Current value of a vector</param>
    /// <param name="delta">Tolerance parameter</param>
    /// <returns>True, if method is stagnating. Otherwise, false</returns>
    public static bool CheckIsStagnate(double[] prevVec, double[] vec, double delta)
    {
        var difVec = new double[vec.Length];

        for (var i = 0; i < vec.Length; i++)
        {
            difVec[i] = prevVec[i] - vec[i];
        }

        return Math.Abs(GeneralOperations.Norm(difVec)) < delta;
    }

    /// <summary>
    ///     Relative residual (||f - Ax|| / ||f||) of slae Ax = f
    /// </summary>
    /// <param name="m">Given weights. A part in slae</param>
    /// <param name="x">Given approximation. x part in slae</param>
    /// <param name="f">Right part (f) of the slae</param>
    /// <returns>Relative residual value</returns>
    public static double RelResidual(IMatrix m, double[] x, double[] f)
    {
        var diff = new double[f.Length];

        var innerProd = GeneralOperations.MatMul(m, x);

        for (var i = 0; i < f.Length; i++)
        {
            diff[i] = f[i] - innerProd[i];
        }

        return GeneralOperations.Norm(diff) / GeneralOperations.Norm(f);
    }

    /// <summary>
    ///     Relative residual (||f - Ax|| / ||f||) of slae Ax = f
    /// </summary>
    /// <param name="slae1DEllipticLinearFNonLinear">Given slae</param>
    /// <returns>Relative residual value</returns>
    public static double RelResidual(Slae1DEllipticLinearFNonLinear slae1DEllipticLinearFNonLinear)
    {
        var diff = new double[slae1DEllipticLinearFNonLinear.RhsVec.Length];

        var innerProd =
            GeneralOperations.MatMul(slae1DEllipticLinearFNonLinear.Matrix, slae1DEllipticLinearFNonLinear.ResVec);

        for (var i = 0; i < slae1DEllipticLinearFNonLinear.RhsVec.Length; i++)
        {
            diff[i] = slae1DEllipticLinearFNonLinear.RhsVec[i] - innerProd[i];
        }

        return GeneralOperations.Norm(diff) / GeneralOperations.Norm(slae1DEllipticLinearFNonLinear.RhsVec);
    }
}