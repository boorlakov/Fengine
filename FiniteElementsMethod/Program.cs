using System.Text.Json;
using FiniteElementsMethod.Fem;

namespace FiniteElementsMethod;

public static class Program
{
    public static void Main()
    {
        var area = JsonSerializer.Deserialize<Models.Area>(File.ReadAllText("input/area.json"))!;
        var inputFuncs =
            JsonSerializer.Deserialize<Models.InputFuncs>(File.ReadAllText("input/inputFuncs.json"))!;
        var boundaryConditions =
            JsonSerializer.Deserialize<Models.BoundaryConditions>(
                File.ReadAllText("input/boundaryConditions.json"))!;
        var accuracy = JsonSerializer.Deserialize<Models.Accuracy>(File.ReadAllText("input/accuracy.json"))!;

        var grid = new Grid(area);
        var result = Solver.SolveWithSimpleIteration(grid, inputFuncs, area, boundaryConditions, accuracy);
    }
}