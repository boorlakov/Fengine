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
    ///     Multiplication m by vector
    /// </summary>
    /// <param name="m">Given matrix in 3-diagonal format</param>
    /// <param name="vec">Vector, represented by double values</param>
    /// <returns>Multiplication result m by vector</returns>
    public static double[] MatMul(Matrix3Diag m, double[] vec)
    {
        var res = new double[vec.Length];

        for (var i = 0; i < vec.Length; i++)
        {
            res[i] = Dot(i, m, vec);
        }

        return res;
    }

    /// <summary>
    ///     Dot product of a specified row in m by vector
    /// </summary>
    /// <param name="i">Row, along which the product is produced</param>
    /// <param name="m">3-diagonal matrix</param>
    /// <param name="vec">Vector, represented by double values</param>
    /// <returns>Dot product of a specified row i in m by vector</returns>
    public static double Dot(int i, Matrix3Diag m, double[] vec)
    {
        var res = m.Data["center"][i] * vec[i];

        if (i >= 0 && i < m.Size - 1)
        {
            res += m.Data["upper"][i] * vec[i + 1];
        }

        if (i > 0 && i <= m.Size - 1)
        {
            res += m.Data["lower"][i - 1] * vec[i - 1];
        }

        return res;
    }
}