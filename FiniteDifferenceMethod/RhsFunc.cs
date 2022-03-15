namespace FiniteDifferenceMethod;

/// <summary>
/// Right side of differential equation, named f
/// -div(\lambda * grad(u)) + \gamma * u = f
/// </summary>
public static class RhsFunc
{
    public static readonly Func<double, double, double> Eval = (x, y) => x;
}