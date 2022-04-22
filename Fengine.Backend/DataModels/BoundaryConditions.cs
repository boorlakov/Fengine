namespace Fengine.Backend.DataModels;

/// <summary>
///     Boundary condition setting class. Support all types of conditions
/// </summary>
public class BoundaryConditions
{
    /// <summary>
    ///     Boundary condition type on left border. Valid values is: "First", "Second", "Third"
    /// </summary>
    public string Left { get; init; } = string.Empty;

    /// <summary>
    ///     Boundary function on left border. Represented by string
    /// </summary>
    public string LeftFunc { get; init; } = string.Empty;

    /// <summary>
    ///     Boundary condition type on right border. Valid values is: "First", "Second", "Third"
    /// </summary>
    public string Right { get; init; } = string.Empty;

    /// <summary>
    ///     Boundary function on right border. Represented by string
    /// </summary>
    public string RightFunc { get; init; } = string.Empty;

    /// <summary>
    ///     Parameter for third boundary condition
    /// </summary>
    public double Beta { get; init; }
}