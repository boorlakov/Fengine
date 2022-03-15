using System.Text.Json;

namespace FiniteDifferenceMethod;

public static class Program
{
    public static void Main()
    {
        var area = JsonSerializer.Deserialize<AreaModel>(File.ReadAllText("input_data/area.json"));
        var grid = new Grid(area!);

        var boundaryConditions =
            JsonSerializer.Deserialize<Dictionary<string, string>>(
                File.ReadAllText("input_data/boundary_conditions.json"));
        var accuracy = JsonSerializer.Deserialize<SlaeAccuracyModel>(File.ReadAllText("input_data/accuracy.json"));

        var slae = new Slae(area!, grid, boundaryConditions!);
        slae.Solve(accuracy!);

        var resVec = Utils.ExcludeFictiveFrom(slae.ResVec, grid);
    }
}