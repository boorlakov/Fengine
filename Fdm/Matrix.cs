namespace Fdm;

public class Matrix
{
    public readonly double[] Diag;

    public readonly double[][] UpperPart = new double[2][];
    public readonly double[][] LowPart = new double[2][];

    public readonly int Shift;
    public readonly int Size;

    public Matrix(double[] diag, double[] l0, double[] l1, double[] u0, double[] u1, int shift)
    {
        Diag = diag;
        LowPart[1] = l1;
        LowPart[0] = l0;
        UpperPart[1] = u1;
        UpperPart[0] = u0;
        Shift = shift;
        Size = diag.Length;
    }
}