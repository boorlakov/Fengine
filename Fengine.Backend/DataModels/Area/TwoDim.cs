namespace Fengine.Backend.DataModels.Area;

public class TwoDim : Area
{
    public double LeftBorder { get; init; }

    public double RightBorder { get; init; }

    public double UpperBorder { get; init; }

    public double LowerBorder { get; init; }

    public int AmountPointsR { get; init; }

    public int AmountPointsZ { get; init; }

    /// <summary>
    ///     Ratio for constructing non-uniform grid.
    ///     If you want uniform grid set value to 1.0
    /// </summary>
    public double DischargeRatioR { get; init; } = 1.0;

    /// <summary>
    ///     Ratio for constructing non-uniform grid.
    ///     If you want uniform grid set value to 1.0
    /// </summary>
    public double DischargeRatioZ { get; init; } = 1.0;
}