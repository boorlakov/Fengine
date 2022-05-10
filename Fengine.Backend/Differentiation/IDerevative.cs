namespace Fengine.Backend.Differentiation;

public interface IDerivative
{
    public double FindFirst1D(Func<double, double> func, double point, double step);
    public double FindFirst1D(string func, double point, double step);
    public double FindFirst1D(Func<Dictionary<string, double>, double> func, double point, double step);

    public double FindFirst2DAt1Point(Func<Dictionary<string, double>, double> func, double x1, double x2, double step);

    public double FindFirst2DAt2Point(Func<Dictionary<string, double>, double> func, double x1, double x2, double step);
}