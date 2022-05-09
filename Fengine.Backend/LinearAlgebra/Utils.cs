namespace Fengine.Backend.LinearAlgebra;

public static class Utils
{
    public static Matrix.IMatrix Copying(Matrix.IMatrix m)
    {
        var ggl = new double[m.Data["ggl"].Length];
        m.Data["ggl"].AsSpan().CopyTo(ggl);

        var ggu = new double[m.Data["ggu"].Length];
        m.Data["ggu"].AsSpan().CopyTo(ggu);

        var di = new double[m.Data["di"].Length];
        m.Data["di"].AsSpan().CopyTo(di);

        var ig = new int[m.Profile["ig"].Length];
        m.Profile["ig"].AsSpan().CopyTo(ig);

        var jg = new int[m.Profile["jg"].Length];
        m.Profile["jg"].AsSpan().CopyTo(jg);

        var decomposed = m.Decomposed;

        return new Matrix.Sparse(ggl, ggu, di, ig, jg, decomposed);
    }

    /// <summary>
    ///     Checks if iteration method is stagnating or not
    /// </summary>
    /// <param name="prevVec">Previous value of a vector</param>
    /// <param name="vec">Current value of a vector</param>
    /// <param name="delta">Tolerance parameter</param>
    /// <returns>True, if method is stagnating. Otherwise, false</returns>
    public static bool CheckIsStagnate(double[] prevVec, double[] vec, double delta)
    {
        var difVec = new double[vec.Length];

        for (var i = 0; i < vec.Length; i++)
        {
            difVec[i] = prevVec[i] - vec[i];
        }

        return Math.Abs(GeneralOperations.Norm(difVec)) < delta;
    }

    /// <summary>
    ///     Relative residual (||f - Ax|| / ||f||) of slae Ax = f
    /// </summary>
    /// <param name="m">Given weights. A part in slae</param>
    /// <param name="x">Given approximation. x part in slae</param>
    /// <param name="f">RightType part (f) of the slae</param>
    /// <returns>Relative residual value</returns>
    public static double RelativeResidual(Matrix.IMatrix m, double[] x, double[] f)
    {
        var diff = new double[f.Length];

        var innerProd = GeneralOperations.MatrixMultiply(m, x);

        for (var i = 0; i < f.Length; i++)
        {
            diff[i] = f[i] - innerProd[i];
        }

        return GeneralOperations.Norm(diff) / GeneralOperations.Norm(f);
    }

    /// <summary>
    ///     Relative residual (||f - Ax|| / ||f||) of slae Ax = f
    /// </summary>
    /// <param name="slae">Given slae</param>
    /// <returns>Relative residual value</returns>
    public static double RelativeResidual(Fem.Slae.NonlinearTask.Elliptic.OneDim.Linear slae)
    {
        var diff = new double[slae.RhsVec.Length];

        var innerProd =
            GeneralOperations.MatrixMultiply(slae.Matrix, slae.ResVec);

        for (var i = 0; i < slae.RhsVec.Length; i++)
        {
            diff[i] = slae.RhsVec[i] - innerProd[i];
        }

        return GeneralOperations.Norm(diff) / GeneralOperations.Norm(slae.RhsVec);
    }
}