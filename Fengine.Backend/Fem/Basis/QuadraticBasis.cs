namespace Fengine.Backend.Fem.Basis;

public class QuadraticBasis : IBasis
{
    public static readonly Func<double, double>[] Func =
    {
        x => 2.0 * (x - 0.5) * (x - 1.0),
        x => -4.0 * x * (x - 1.0),
        x => -2.0 * x * (x - 0.5)
    };

    public static readonly Func<double, double>[] FirstDerivative =
    {
        x => 4 * x - 3.0,
        x => -8.0 * x + 4.0,
        x => -4.0 * x + 1.0
    };
}