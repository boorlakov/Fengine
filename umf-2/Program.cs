using System.Text.Json;

namespace umf_2;

public static class Program
{
    public static void Main()
    {
        var calc = new Sprache.Calc.XtensibleCalculator();

        var inputFuncs = JsonSerializer.Deserialize<InputFuncsModel>(File.ReadAllText("funcs/inputFuncs.json"))!;

        var lambdaFunc = calc.ParseFunction(inputFuncs.Lambda).Compile();
        var gammaFunc = calc.ParseFunction(inputFuncs.Gamma).Compile();
        var rhsFunc = calc.ParseFunction(inputFuncs.RhsFunc).Compile();
        var uStarFunc = calc.ParseFunction(inputFuncs.UStar).Compile();

        var x = 3.14;
        var y = 6.28;

        Console.WriteLine($"Lambda({x}, {y}) = {lambdaFunc(MakeDict(x, y))}");
        Console.WriteLine($"Gamma({x}, {y}) = {gammaFunc(MakeDict(x, y))}");
        Console.WriteLine($"rhsFunc({x}, {y}) = {rhsFunc(MakeDict(x, y))}");
        Console.WriteLine($"uStar({x}, {y}) = {uStarFunc(MakeDict(x, y))}");
    }

    public static Dictionary<string, double> MakeDict(double x, double y) => new() {{"x", x}, {"y", y}};
}

public class InputFuncsModel
{
    public string? Lambda { get; init; }
    public string? Gamma { get; init; }
    public string? RhsFunc { get; init; }
    public string? UStar { get; init; }
}