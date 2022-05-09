namespace Fengine.Backend.LinearAlgebra.Matrix;

public interface IMatrix
{
    double[] Multiply(double[] v);

    Dictionary<string, double[]> Data { get; }

    Dictionary<string, int[]> IndexData => throw new NotSupportedException();

    /// <summary>
    ///     Size of matrix3Diag
    /// </summary>
    int Size { get; }
}