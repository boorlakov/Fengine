using System.Data;

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

    /// <summary>
    /// LU(sq)-decomposition with value=1 in diagonal elements of U matrix.
    /// Corrupts base object. To access data as one matrix you need to build it from L and U.
    /// </summary>
    /// <exception cref="DivideByZeroException"> If diagonal element is zero </exception>
    public void Factorize()
    {
        for (var i = 0; i < Size; i++)
        {
            var sumDi = 0.0;

            var i0 = IndexData["ig"][i];
            var i1 = IndexData["ig"][i + 1];

            for (var k = i0; k < i1; k++)
            {
                var j = IndexData["jg"][k];
                var j0 = IndexData["ig"][j];
                var j1 = IndexData["ig"][j + 1];

                var iK = i0;
                var kJ = j0;

                var sumL = 0.0;
                var sumU = 0.0;

                while (iK < k && kJ < j1)
                {
                    if (IndexData["jg"][iK] == IndexData["jg"][kJ])
                    {
                        sumL += Data["ggl"][iK] * Data["Ggu"][kJ];
                        sumU += Data["ggu"][iK] * Data["ggl"][kJ];
                        iK++;
                        kJ++;
                    }
                    else
                    {
                        if (Data["jg"][iK] > Data["jg"][kJ])
                        {
                            kJ++;
                        }
                        else
                        {
                            iK++;
                        }
                    }
                }

                if (Data["di"][j] == 0.0)
                {
                    throw new DivideByZeroException($"Di[{j}] has thrown at pos {i} {j}");
                }

                Data["ggl"][k] = (Data["ggl"][k] - sumL) / Data["di"][j];
                Data["ggu"][k] = (Data["ggu"][k] - sumU) / Data["di"][j];

                sumDi += Data["ggl"][k] * Data["ggu"][k];
            }

            Data["di"][i] = Math.Sqrt(Data["di"][i] - sumDi);
        }

        Decomposed = true;
    }

    public double[] Multiply(double[] v)
    {
        if (Size != v.Length)
        {
            throw new EvaluateException($"[ERR] Different sizes. Matrix size = {Size}, vector size = {v.Length}");
        }

        var res = new double[v.Length];

        for (var i = 0; i < v.Length; i++)
        {
            res[i] = Data["di"][i] * v[i];

            for (var j = IndexData["ig"][i]; j < IndexData["ig"][i + 1]; j++)
            {
                res[i] += Data["ggl"][j] * v[IndexData["jg"][j]];
                res[IndexData["jg"][j]] += Data["ggu"][j] * v[i];
            }
        }

        return res;
    }
    public Dictionary<string, double[]> Data { get; } = new();
    public Dictionary<string, int[]> IndexData { get; } = new();
    public int Size { get; }
}