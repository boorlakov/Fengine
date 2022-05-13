namespace Fengine.Backend.Fem.Basis;

public class BilinearBasis : IBasis
{
    public static readonly Func<double, double, double>[] Func =
    {
        (x, y) => (1.0 - x) * (1.0 - y),
        (x, y) => x * (1.0 - y),
        (x, y) => (1.0 - x) * y,
        (x, y) => x * y
    };
}