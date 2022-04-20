namespace Fengine.Fem.Basis;

/// <summary>
///     IBasis class that is essential for decomposition in basis func
/// </summary>
public interface IBasis
{
    public static readonly Func<double[], double>[] Func;
}