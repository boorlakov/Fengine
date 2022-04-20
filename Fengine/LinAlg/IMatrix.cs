namespace Fengine.LinAlg;

public interface IMatrix
{
    Dictionary<string, double[]> Data { get; set; }

    /// <summary>
    ///     Size of matrix3Diag
    /// </summary>
    double Size { get; }
}