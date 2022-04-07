namespace FiniteElementsMethod.LinAlg;

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
    ///     Multiplication matrix by vector
    /// </summary>
    /// <param name="matrix">Given matrix in 3-diagonal format</param>
    /// <param name="vec">Vector, represented by double values</param>
    /// <returns>Multiplication result matrix by vector</returns>
    public static double[] MatMul(Matrix matrix, double[] vec)
    {
        var res = new double[vec.Length];

        for (var i = 0; i < vec.Length; i++)
        {
            res[i] = Dot(i, matrix, vec);
        }

        return res;
    }

    /// <summary>
    ///     Dot product of a specified row in matrix by vector
    /// </summary>
    /// <param name="i">Row, along which the product is produced</param>
    /// <param name="matrix">3-diagonal matrix</param>
    /// <param name="vec">Vector, represented by double values</param>
    /// <returns>Dot product of a specified row i in matrix by vector</returns>
    public static double Dot(int i, Matrix matrix, double[] vec)
    {
        var res = matrix.Center[i] * vec[i];

        if (i >= 0 && i < matrix.Size - 1)
        {
            res += matrix.Upper[i] * vec[i + 1];
        }

        if (i > 0 && i <= matrix.Size - 1)
        {
            res += matrix.Lower[i - 1] * vec[i - 1];
        }

        return res;
    }
}