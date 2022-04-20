using Fengine.LinAlg.Matrix;

namespace Fengine.LinAlg;

/// <summary>
///     Class, holding general operations in linear algebra
/// </summary>
public static class GeneralOperations
{
    /// <summary>
    ///     Euclidean norm of a vector
    /// </summary>
    /// <param name="x">Vector, represented by double values</param>
    /// <returns>Euclidean norm of a given x</returns>
    public static double Norm(IEnumerable<double> x)
    {
        return Math.Sqrt(x.Sum(t => t * t));
    }

    /// <summary>
    ///     Multiplication matrix3Diag by vector
    /// </summary>
    /// <param name="matrix3Diag">Given matrix3Diag in 3-diagonal format</param>
    /// <param name="vec">Vector, represented by double values</param>
    /// <returns>Multiplication result matrix3Diag by vector</returns>
    public static double[] MatMul(Matrix3Diag matrix3Diag, double[] vec)
    {
        var res = new double[vec.Length];

        for (var i = 0; i < vec.Length; i++)
        {
            res[i] = Dot(i, matrix3Diag, vec);
        }

        return res;
    }

    /// <summary>
    ///     Dot product of a specified row in matrix3Diag by vector
    /// </summary>
    /// <param name="i">Row, along which the product is produced</param>
    /// <param name="matrix3Diag">3-diagonal matrix3Diag</param>
    /// <param name="vec">Vector, represented by double values</param>
    /// <returns>Dot product of a specified row i in matrix3Diag by vector</returns>
    public static double Dot(int i, Matrix3Diag matrix3Diag, double[] vec)
    {
        var res = matrix3Diag.Data["center"][i] * vec[i];

        if (i >= 0 && i < matrix3Diag.Size - 1)
        {
            res += matrix3Diag.Data["upper"][i] * vec[i + 1];
        }

        if (i > 0 && i <= matrix3Diag.Size - 1)
        {
            res += matrix3Diag.Data["lower"][i - 1] * vec[i - 1];
        }

        return res;
    }
}