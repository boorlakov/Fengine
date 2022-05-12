namespace Fengine.Backend.DataModels.Conditions.Boundary;

public class TwoDim : BoundaryConditions
{
    /// <summary>
    ///     Boundary condition type on left border. Valid values is: "First", "Second", "Third"
    /// </summary>
    public string LeftType { get; init; } = string.Empty;

    /// <summary>
    ///     Boundary function on left border. Represented by string
    /// </summary>
    public string LeftFunc { get; init; } = "0.0";

    /// <summary>
    ///     Boundary condition type on right border. Valid values is: "First", "Second", "Third"
    /// </summary>
    public string RightType { get; init; } = string.Empty;

    /// <summary>
    ///     Boundary function on right border. Represented by string
    /// </summary>
    public string RightFunc { get; init; } = "0.0";

    /// <summary>
    ///     Boundary condition type on upper border. Valid values is: "First", "Second", "Third"
    /// </summary>
    public string UpperType { get; init; } = string.Empty;

    /// <summary>
    ///     Boundary function on upper border. Represented by string
    /// </summary>
    public string UpperFunc { get; init; } = "0.0";

    /// <summary>
    ///     Boundary condition type on lower border. Valid values is: "First", "Second", "Third"
    /// </summary>
    public string LowerType { get; init; } = string.Empty;

    /// <summary>
    ///     Boundary function on lower border. Represented by string
    /// </summary>
    public string LowerFunc { get; init; } = "0.0";
}