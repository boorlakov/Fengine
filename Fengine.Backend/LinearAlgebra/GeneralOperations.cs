using Fengine.Backend.LinearAlgebra.Matrix;

namespace Fengine.Backend.LinearAlgebra;

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
    /// Matrix multiplication by vector
    /// </summary>
    /// <param name="m"> Matrix, stored in 2-dim double array</param>
    /// <param name="b"> Vector, that multiplies matrix </param>
    /// <returns> Resulting vector of multiplication </returns>
    public static double[] MatrixMultiply(double[,] m, double[] b)
    {
        var columns = m.GetLength(0);
        var rows = m.GetLength(1);

        var res = new double[rows];

        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < columns; j++)
            {
                res[i] += m[i, j] * b[j];
            }
        }

        return res;
    }

    public static double[] MatrixMultiply(IMatrix m, double[] b)
    {
        return m.Multiply(b);
    }


    /// <summary>
    ///     Dot product of a specified row in m by vector
    /// </summary>
    /// <param name="i">Row, along which the product is produced</param>
    /// <param name="m">3-diagonal matrix</param>
    /// <param name="vec">Vector, represented by double values</param>
    /// <returns>Dot product of a specified row i in m by vector</returns>
    public static double Dot(int i, IMatrix m, double[] vec)
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