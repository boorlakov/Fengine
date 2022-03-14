using System.Text.Json;

namespace umf_2;

public static class Program
{
    public static void Main()
    {
        var calc = new Sprache.Calc.XtensibleCalculator();

        var area = JsonSerializer.Deserialize<JsonModels.AreaModel>(File.ReadAllText("input/area.json"))!;
        var inputFuncs =
            JsonSerializer.Deserialize<JsonModels.InputFuncsModel>(File.ReadAllText("input/inputFuncs.json"))!;
        var boundaryConditions =
            JsonSerializer.Deserialize<JsonModels.BoundaryConditionsModel>(
                File.ReadAllText("input/boundaryConditions.json"))!;
        var accuracy = JsonSerializer.Deserialize<JsonModels.AccuracyModel>(File.ReadAllText("input/accuracy.json"))!;

        var grid = new Grid(area);
        var result = Fem.SolveWithSimpleIteration(grid, inputFuncs, area, boundaryConditions, accuracy);
    }
}

public static class LinearBasis
{
    public static Func<double, double>[] Func =
    {
        x => x,
        x => 1.0 - x
    };
}