namespace FiniteElementsMethod.LinAlg;

/// <summary>
/// Matrix class represented in 3-diagonal format
/// </summary>
public class Matrix
{
    // Matrix example
    // +- -+
    // |cu |
    // |lcu|
    // | lc|
    // +- -+        
    //
    // u - upper
    // c - center
    // l - lower

    /// <summary>
    /// Upper part of the matrix
    /// </summary>
    public double[] Upper { get; init; }

    /// <summary>
    /// Central part of the matrix
    /// </summary>
    public double[] Center { get; init; }

    /// <summary>
    /// Lower part of the matrix
    /// </summary>
    public double[] Lower { get; init; }

    /// <summary>
    /// Size of matrix
    /// </summary>
    public double Size { get; }

    public Matrix(double[] upper, double[] center, double[] lower)
    {
        Upper = upper;
        Center = center;
        Lower = lower;

        Size = center.Length;
    }
}