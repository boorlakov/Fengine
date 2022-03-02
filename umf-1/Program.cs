using System.Text.Json;

namespace umf_1;

public static class Program
{
    public static void Main()
    {
        var area = JsonSerializer.Deserialize<AreaModel>(File.ReadAllText("area.json"));
        var grid = new Grid(area!);
        var boundaryConditions = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("BoundaryConditions.json"));
        var accuracy = JsonSerializer.Deserialize<SlaeAccuracyModel>(File.ReadAllText("accuracy.json"));
        var slae = new Slae(area!, grid, boundaryConditions!);
        slae.Solve(accuracy!);
        var resVec = Utils.ExcludeFictiveFrom(slae.ResVec, grid);
        // For breakpoint
        var bom = 0;
    }
}