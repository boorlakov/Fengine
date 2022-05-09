namespace Fengine.Backend.DataModels.Areas;

public class TwoDim : Area
{
    public double LeftBorder { get; init; }

    public double RightBorder { get; init; }

    public double UpperBorder { get; init; }

    public double LowerBorder { get; init; }

    public int AmountPointsR { get; init; }

    public int AmountPointsZ { get; init; }
}