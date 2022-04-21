namespace Fengine.Backend.LinAlg.Matrix;

/// <summary>
///     Matrix class represented in 3-diagonal format
/// </summary>
public class Matrix3Diagonal : IMatrix
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

    public Matrix3Diagonal(double[] upper, double[] center, double[] lower)
    {
        Data.Add("upper", upper);
        Data.Add("center", center);
        Data.Add("lower", lower);
        Size = Data["center"].Length;
    }

    public Matrix3Diagonal()
    {
        Size = -1;
    }

    public Dictionary<string, double[]> Data { get; set; } = new();

    /// <summary>
    ///     Size of matrix3Diag
    /// </summary>
    public double Size { get; }
}