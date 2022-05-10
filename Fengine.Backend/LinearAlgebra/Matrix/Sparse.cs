using System.Data;

namespace Fengine.Backend.LinearAlgebra.Matrix;

public class Sparse : IMatrix
{
    public void CopyTo(Sparse m)
    {
        m.Data["ggl"] = new double[Data["ggl"].Length];
        Data["ggl"].AsSpan().CopyTo(m.Data["ggl"]);

        m.Data["ggu"] = new double[Data["ggu"].Length];
        Data["ggu"].AsSpan().CopyTo(m.Data["ggu"]);

        m.Data["di"] = new double[Data["di"].Length];
        Data["di"].AsSpan().CopyTo(m.Data["di"]);

        m.Profile["ig"] = new int[Profile["ig"].Length];
        Profile["ig"].AsSpan().CopyTo(m.Profile["ig"]);

        m.Profile["jg"] = new int[Profile["jg"].Length];
        Profile["jg"].AsSpan().CopyTo(m.Profile["jg"]);

        m.Decomposed = Decomposed;
        m.Size = Size;
    }

    public Sparse()
    {
        Size = -1;
    }

    public Sparse(IMatrix m)
    {
        Data["ggl"] = new double[m.Data["ggl"].Length];
        m.Data["ggl"].AsSpan().CopyTo(Data["ggl"]);

        Data["ggu"] = new double[m.Data["ggu"].Length];
        m.Data["ggu"].AsSpan().CopyTo(Data["ggu"]);

        Data["di"] = new double[m.Data["di"].Length];
        m.Data["di"].AsSpan().CopyTo(Data["di"]);

        Profile["ig"] = new int[m.Profile["ig"].Length];
        m.Profile["ig"].AsSpan().CopyTo(Profile["ig"]);

        Profile["jg"] = new int[m.Profile["jg"].Length];
        m.Profile["jg"].AsSpan().CopyTo(Profile["jg"]);

        Decomposed = m.Decomposed;
        Size = m.Size;
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

        Profile.Add("ig", ig);
        Profile.Add("jg", jg);

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

            var i0 = Profile["ig"][i];
            var i1 = Profile["ig"][i + 1];

            for (var k = i0; k < i1; k++)
            {
                var j = Profile["jg"][k];
                var j0 = Profile["ig"][j];
                var j1 = Profile["ig"][j + 1];

                var iK = i0;
                var kJ = j0;

                var sumL = 0.0;
                var sumU = 0.0;

                while (iK < k && kJ < j1)
                {
                    if (Profile["jg"][iK] == Profile["jg"][kJ])
                    {
                        sumL += Data["ggl"][iK] * Data["ggu"][kJ];
                        sumU += Data["ggu"][iK] * Data["ggl"][kJ];
                        iK++;
                        kJ++;
                    }
                    else
                    {
                        if (Profile["jg"][iK] > Profile["jg"][kJ])
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

            for (var j = Profile["ig"][i]; j < Profile["ig"][i + 1]; j++)
            {
                res[i] += Data["ggl"][j] * v[Profile["jg"][j]];
                res[Profile["jg"][j]] += Data["ggu"][j] * v[i];
            }
        }

        return res;
    }
    public Dictionary<string, double[]> Data { get; } = new();
    public Dictionary<string, int[]> Profile { get; } = new();
    public int Size { get; private set; }
}