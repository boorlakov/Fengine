namespace Fengine.Backend.LinAlg.Matrix;

public interface IMatrix
{
    Dictionary<string, double[]> Data { get; set; }

    /// <summary>
    ///     Size of matrix3Diag
    /// </summary>
    double Size { get; }
}