namespace Fengine.Backend.LinearAlgebra.Matrix;

/// <summary>
///     Matrix class represented in 3-diagonal format
/// </summary>
public class ThreeDiagonal : IMatrix
{
    // Matrix example
    // +---+
    // |cu |
    // |lcu|
    // | lc|
    // +---+        
    //
    // u - upper
    // c - center
    // l - lower

    public ThreeDiagonal(double[] upper, double[] center, double[] lower)
    {
        Data.Add("upper", upper);
        Data.Add("center", center);
        Data.Add("lower", lower);
        Size = Data["center"].Length;
    }

    public ThreeDiagonal()
    {
        Size = -1;
    }

    public double[] Multiply(double[] v)
    {
        double Dot(int i, double[] vec)
        {
            var res = Data["center"][i] * vec[i];

            if (i >= 0 && i < Size - 1)
            {
                res += Data["upper"][i] * vec[i + 1];
            }

            if (i > 0 && i <= Size - 1)
            {
                res += Data["lower"][i - 1] * vec[i - 1];
            }

            return res;
        }

        var res = new double[v.Length];

        for (var i = 0; i < v.Length; i++)
        {
            res[i] = Dot(i, v);
        }

        return res;
    }
    public Dictionary<string, double[]> Data { get; set; } = new();

    /// <summary>
    ///     Size of matrix3Diag
    /// </summary>
    public int Size { get; }
}