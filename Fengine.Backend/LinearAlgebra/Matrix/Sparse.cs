namespace Fengine.Backend.LinearAlgebra.Matrix;

public class Sparse : IMatrix
{
    public Sparse()
    {
        Size = -1;
    }

    public Sparse
    (
        double[] ggl,
        double[] ggu,
        double[] di,
        int[] ig,
        int[] jg,
        bool decomposed = false
    )
    {
        Data.Add("ggl", ggl);
        Data.Add("ggu", ggu);
        Data.Add("di", di);

        IndexData.Add("ig", ig);
        IndexData.Add("jg", jg);

        Size = Data["di"].Length;
        Decomposed = decomposed;
    }

    public bool Decomposed { get; private set; }

    public Dictionary<string, double[]> Data { get; } = new();
    public Dictionary<string, int[]> IndexData { get; } = new();
    public double Size { get; }
}