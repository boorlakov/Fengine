namespace Fengine.Backend.LinearAlgebra.Matrix;

public interface IMatrix
{
    Dictionary<string, double[]> Data { get; }

    Dictionary<string, int[]> IndexData => throw new NotSupportedException();

    /// <summary>
    ///     Size of matrix3Diag
    /// </summary>
    double Size { get; }
}