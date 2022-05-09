namespace Fengine.Backend.DataModels.Areas;

public abstract class Area
{
    /// <summary>
    ///     Ratio for constructing non-uniform grid.
    ///     If you want uniform grid set value to 1.0
    /// </summary>
    public double DischargeRatio { get; init; }
}