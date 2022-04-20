namespace Fengine.Fem;

/// <summary>
///     IBasis class that is essential for decomposition in basis func
/// </summary>
public interface IBasis
{
    public static readonly Func<double[], double>[] Func;
}

public class LinearBasis : IBasis
{
    public static readonly Func<double, double>[] Func =
    {
        x => 1.0 - x,
        x => x
    };
}

public class BilinearBasis : IBasis
{
    public static readonly Func<double, double, double>[] Func =
    {
        (x, y) => x * y,
        (x, y) => (1.0 - x) * y,
        (x, y) => (1.0 - y) * x,
        (x, y) => (1.0 - x) * (1.0 - y)
    };
}