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

    public Dictionary<string, double[]> Data { get; set; } = new();

    /// <summary>
    ///     Size of matrix3Diag
    /// </summary>
    public double Size { get; }
}