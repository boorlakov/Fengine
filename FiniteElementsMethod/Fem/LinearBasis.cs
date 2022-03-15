namespace FiniteElementsMethod.Fem;

public static class LinearBasis
{
    public static readonly Func<double, double>[] Func =
    {
        x => x,
        x => 1.0 - x
    };
}