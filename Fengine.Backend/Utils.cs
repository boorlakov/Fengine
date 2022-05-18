using Sprache.Calc;

namespace Fengine.Backend;

/// <summary>
///     Class with useful utilities
/// </summary>
public static class Utils
{
    /// <summary>
    ///     Evaluates function (given in string form)
    /// </summary>
    /// <param name="inputFuncString">F: R -> R</param>
    /// <param name="arg">Values of passing parameter</param>
    /// <returns>Function value at point arg</returns>
    public static double EvalFunc(string inputFuncString, double arg)
    {
        var calc = new XtensibleCalculator();
        var toEval = calc.ParseFunction(inputFuncString).Compile();
        return toEval(MakeDict1D(arg));
    }

    /// <summary>
    ///     Makes specific dictionary for passing into function in string form
    /// </summary>
    /// <param name="x">Value of passing argument to function</param>
    /// <returns>Dictionary like {{"x", x}}</returns>
    public static Dictionary<string, double> MakeDict1D(double x)
    {
        return new Dictionary<string, double> {{"x", x}};
    }

    /// <summary>
    ///     Makes specific dictionary for passing into function in string form
    /// </summary>
    /// <param name="x">Value of passing argument x to function</param>
    /// <param name="u">Value of passing argument u to function</param>
    /// <returns>Dictionary like {{"x", x}, {"u": u}}</returns>
    public static Dictionary<string, double> MakeDict2DCartesian(double x, double u)
    {
        return new Dictionary<string, double> {{"x", x}, {"u", u}};
    }

    /// <summary>
    ///     Makes specific dictionary for passing into function in string form
    /// </summary>
    /// <param name="r">Value of passing argument r to function</param>
    /// <param name="z">Value of passing argument z to function</param>
    /// <returns>Dictionary like {{"x", x}, {"u": u}}</returns>
    public static Dictionary<string, double> MakeDict2DCylindrical(double r, double z)
    {
        return new Dictionary<string, double> {{"r", r}, {"z", z}};
    }

    /// <summary>
    ///     Makes specific dictionary for passing into function in string form
    /// </summary>
    /// <param name="r">Value of passing argument r to function</param>
    /// <param name="z">Value of passing argument z to function</param>
    /// <param name="t">Value of passing argument t to function</param>
    /// <returns>Dictionary like {{"x", x}, {"u": u}}</returns>
    public static Dictionary<string, double> MakeDict2DCylindricalTime(double r, double z, double t)
    {
        return new Dictionary<string, double> {{"r", r}, {"z", z}, {"t", t}};
    }

    /// <summary>
    ///     Makes 1D grid with 11 points in it
    /// </summary>
    /// <param name="leftBorder">LeftType border point</param>
    /// <param name="rightBorder">RightType border point</param>
    /// <returns>integrationGrid from leftBorder to rightBorder with 11 points in it</returns>
    public static double[] Create1DIntegrationMesh(double leftBorder, double rightBorder)
    {
        var integrationGrid = new double[11];
        var step = (rightBorder - leftBorder) / 10.0;

        for (var i = 0; i < integrationGrid.Length; i++)
        {
            integrationGrid[i] = leftBorder + i * step;
        }

        return integrationGrid;
    }
}