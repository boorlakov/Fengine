namespace FiniteElementsMethod;

public static class Utils
{
    public static double EvalFunc(string inputFuncString, double arg)
    {
        var calc = new Sprache.Calc.XtensibleCalculator();
        var toEval = calc.ParseFunction(inputFuncString).Compile();
        return toEval(MakeDict1D(arg));
    }

    public static Dictionary<string, double> MakeDict1D(double x) => new() {{"x", x}};
}