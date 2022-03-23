namespace FiniteElementsMethod;

/// <summary>
/// Class with useful utilities
/// </summary>
public static class Utils
{
    /// <summary>
    /// Evaluates function (given in string form)
    /// </summary>
    /// <param name="inputFuncString">F: R -> R</param>
    /// <param name="arg">Values of passing parameter</param>
    /// <returns>Function value at point arg</returns>
    public static double EvalFunc(string inputFuncString, double arg)
    {
        var calc = new Sprache.Calc.XtensibleCalculator();
        var toEval = calc.ParseFunction(inputFuncString).Compile();
        return toEval(MakeDict1D(arg));
    }

    /// <summary>
    /// Makes specific dictionary for passing into function in string form 
    /// </summary>
    /// <param name="x">Value of passing argument to function</param>
    /// <returns>Dictionary like {{"x", x}}</returns>
    public static Dictionary<string, double> MakeDict1D(double x) => new() {{"x", x}};

    /// <summary>
    /// Makes specific dictionary for passing into function in string form 
    /// </summary>
    /// <param name="x">Value of passing argument x to function</param>
    /// <param name="u">Value of passing argument u to function</param>
    /// <returns>Dictionary like {{"x", x}, {"u": u}}</returns>
    public static Dictionary<string, double> MakeDict2D(double x, double u) => new() {{"x", x}, {"u", u}};
}