namespace umf_1;

public static class BoundaryFunc
{
    public static readonly Func<double, double, double>[] Left =
    {
        (x, y) => x,
    };

    public static readonly Func<double, double, double>[] Right =
    {
        (x, y) => x,
    };

    public static readonly Func<double, double, double>[] Upper =
    {
        (x, y) => x,
    };

    public static readonly Func<double, double, double>[] Lower =
    {
        (x, y) => x,
    };
}