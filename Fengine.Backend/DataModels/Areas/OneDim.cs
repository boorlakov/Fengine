namespace Fengine.Backend.DataModels.Areas;

/// <summary>
///     1D area description, that essential for constructing grid in solving process.
///     For example: [0.0, 1.0]. LeftBorder stands for 0.0, RightBorder for 1.0
/// </summary>
public class OneDim : Area
{
    /// <summary>
    ///     Starting point of area
    /// </summary>
    public double LeftBorder { get; init; }

    /// <summary>
    ///     Ending point of area
    /// </summary>
    public double RightBorder { get; init; }

    /// <summary>
    ///     Amount of points in segment. Including starting point
    /// </summary>
    public int AmountPoints { get; init; }
}