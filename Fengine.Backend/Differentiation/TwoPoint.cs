using Sprache.Calc;

namespace Fengine.Backend.Differentiation;

public class TwoPoint : IDerivative
{
    public double FindFirst1D(Func<double, double> func, double point, double step)
    {
        return (func(point + step) - func(point - step)) / (2 * step);
    }

    public double FindFirst1D(string func, double point, double step)
    {
        var calc = new XtensibleCalculator();
        var funcToDerive = calc.ParseFunction(func).Compile();

        return (funcToDerive(Utils.MakeDict1D(point + step)) - funcToDerive(Utils.MakeDict1D(point - step))) /
               (2 * step);
    }

    public double FindFirst1D(Func<Dictionary<string, double>, double> func, double point, double step)
    {
        return (func(Utils.MakeDict1D(point + step)) - func(Utils.MakeDict1D(point - step))) /
               (2 * step);
    }

    public double FindFirst2DAt1Point(Func<Dictionary<string, double>, double> func, double x1, double x2, double step)
    {
        throw new NotImplementedException();
    }

    public double FindFirst2DAt2Point(Func<Dictionary<string, double>, double> func, double x1, double x2, double step)
    {
        return (func(Utils.MakeDict2D(x1, x2 + step)) - func(Utils.MakeDict2D(x1, x2 - step))) /
               (2 * step);
    }
}