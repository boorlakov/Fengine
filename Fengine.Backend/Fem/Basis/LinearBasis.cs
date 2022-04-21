namespace Fengine.Backend.Fem.Basis;

public class LinearBasis : IBasis
{
    public static readonly Func<double, double>[] Func =
    {
        x => 1.0 - x,
        x => x
    };
}