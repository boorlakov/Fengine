namespace FiniteElementsMethod.Fem;

/// <summary>
/// Basis class that is essential for decomposition in basis func
/// </summary>
public static class Basis
{
    public static class Linear
    {
        public static readonly Func<double, double>[] Func =
        {
            x => x,
            x => 1.0 - x
        };
    }
}