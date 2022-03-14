namespace umf_2.LinAlg;

public static class GeneralOperations
{
    public static double Norm(IEnumerable<double> x) => Math.Sqrt(x.Sum(t => t * t));

    public static double[] MatMul(Matrix matrix, double[] vec)
    {
        var res = new double[vec.Length];

        for (var i = 0; i < vec.Length; i++)
        {
            res[i] = Dot(i, matrix, vec);
        }

        return res;
    }

    public static double Dot(int i, Matrix matrix, double[] vec)
    {
        var res = 0.0;
        res += matrix.Center[i] * vec[i];

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