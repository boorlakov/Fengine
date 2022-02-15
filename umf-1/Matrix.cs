namespace umf_1;

public class Matrix
{
    public readonly double[] Diag;

    public readonly double[][] LowPart = new double[3][];

    public readonly int M;
    public readonly int N;

    public readonly double[][] UpperPart = new double[3][];

    public Matrix()
    {
        M = 0;
        N = 0;
    }

    public Matrix(int n, int m, double[] diag, double[][] upperPart, double[][] lowPart)
    {
        N = n;
        M = m;
        Diag = diag;
        UpperPart = upperPart;
        LowPart = lowPart;
    }
}