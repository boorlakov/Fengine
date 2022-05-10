using Fengine.Backend.LinearAlgebra.Matrix.Exception;

namespace Fengine.Backend.LinearAlgebra.Matrix;

public interface IMatrix
{
    double[] Multiply(double[] v);

    Dictionary<string, double[]> Data { get; }

    Dictionary<string, int[]> Profile => throw new NotSupportedException();

    void Factorize() => throw new NotSupportedException();

    void CopyTo(IMatrix matrix) => throw new NotDecomposedException();

    bool Decomposed => throw new NotSupportedException();

    /// <summary>
    ///     Size of matrix3Diag
    /// </summary>
    int Size { get; }
}