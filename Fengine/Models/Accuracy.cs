namespace Fengine.Models;

/// <summary>
///     Accuracy model for given essential info about solving slae & other iteration processes
/// </summary>
public class Accuracy
{
    /// <summary>
    ///     Maximum iterations that can be done by iteration process
    /// </summary>
    public int MaxIter { get; init; }

    /// <summary>
    ///     Parameter for checking relative residual status
    /// </summary>
    public double Eps { get; init; }

    /// <summary>
    ///     Parameter for checking stagnation status
    /// </summary>
    public double Delta { get; init; }

    public bool AutoRelax { get; init; }

    public double RelaxRatio { get; init; }
}