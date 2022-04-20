namespace Fengine.Models;

/// <summary>
///     Input functions of solving equation. -div(\lambda(x) * grad(u)) + \gamma(x) * u = f(u)
/// </summary>
public class InputFuncs
{
    /// <summary>
    ///     Lambda part of differential equation
    ///     -div(\lambda * grad(u)) + \gamma * u = f
    ///     Also called as a diffusion coefficient. Represented by string
    /// </summary>
    public string Lambda { get; init; } = string.Empty;

    /// <summary>
    ///     gamma part of differential equation
    ///     -div(\lambda * grad(u)) + \gamma * u = f. Represented by string
    /// </summary>
    public string Gamma { get; init; } = string.Empty;

    /// <summary>
    ///     Right side of differential equation, named f
    ///     -div(\lambda * grad(u)) + \gamma * u = f. Represented by string
    /// </summary>
    public string RhsFunc { get; init; } = string.Empty;

    /// <summary>
    ///     Target function, that solver tries to approximate, solving
    ///     differential equation -div(\lambda * grad(u)) + \gamma * u = f. Represented by string
    /// </summary>
    public string UStar { get; init; } = string.Empty;
}