namespace FiniteElementsMethod.LinAlg;

public class Matrix
{
    public double[] Upper { get; init; }
    public double[] Center { get; init; }
    public double[] Lower { get; init; }

    public double Size { get; }

    public Matrix(double[] upper, double[] center, double[] lower)
    {
        Upper = upper;
        Center = center;
        Lower = lower;

        Size = center.Length;
    }
}