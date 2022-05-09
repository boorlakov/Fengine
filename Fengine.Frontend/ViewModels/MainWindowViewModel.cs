using System;
using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Threading;
using Fengine.Backend.Fem.Mesh;
using Fengine.Backend.LinearAlgebra.Matrix;
using Fengine.Backend.LinearAlgebra.SlaeSolver;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Sprache.Calc;

namespace Fengine.Frontend.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly Backend.Fem.Solver.SimpleIteration _femSolver;

    private string _result = string.Empty;

    private readonly ServiceProvider _serviceProvider;

    private readonly string _statusLabelContent = string.Empty;

    public MainWindowViewModel()
    {
        _serviceProvider = DependencyInjectionModule
            .ConfigureServices()
            .BuildServiceProvider();

        var slaeSolver = new GaussSeidel();
        var integrator = new Backend.Integration.GaussFourPoints();
        var matrixType = new ThreeDiagonal();
        var slaeType = new Backend.Fem.Slae.NonlinearTask.Elliptic.OneDim.Linear();
        var differentiatorType = new Backend.Differentiation.TwoPoints();

        _femSolver =
            new Backend.Fem.Solver.SimpleIteration(slaeSolver, integrator, matrixType, slaeType, differentiatorType);
    }

    public Backend.DataModels.Area.OneDim Area { get; } = new();

    public Backend.DataModels.InputFuncs InputFuncs { get; } = new();

    public ObservableCollection<string> BoundaryConditionItems { get; } = new()
    {
        "First",
        "Second",
        "Third"
    };

    public Backend.DataModels.Conditions.Boundary.OneDim BoundaryConditions { get; } = new();

    public Backend.DataModels.Accuracy Accuracy { get; } = new();

    public string Result
    {
        get => _result;
        set => this.RaiseAndSetIfChanged(ref _result, value);
    }

    public string StatusLabelContent
    {
        get => _statusLabelContent;
        set => this.RaiseAndSetIfChanged(ref _result, value);
    }

    public void Solve(Dispatcher dispatcher)
    {
        var mesh = new Backend.Fem.Mesh.Cartesian.OneDim(Area);

        var res = _femSolver.Solve(
            mesh,
            InputFuncs,
            Area,
            BoundaryConditions,
            Accuracy
        );
        var calc = new XtensibleCalculator();
        var uStar = calc.ParseFunction(InputFuncs.UStar).Compile();

        var sb = new StringBuilder("u:\n");

        foreach (var val in res.Values)
        {
            sb.Append($"\t{val}\n");
        }

        sb.Append("\nu*:\n");

        for (var i = 0; i < res.Values.Length; i++)
        {
            sb.Append($"\t{uStar(Backend.Utils.MakeDict1D(mesh.Nodes[i].Coordinates[Axis.X]))}\n");
        }

        sb.Append("\n|u - u*|:\n");

        for (var i = 0; i < res.Values.Length; i++)
        {
            sb.Append(
                $"\t{Math.Abs(res.Values[i] - uStar(Backend.Utils.MakeDict1D(mesh.Nodes[i].Coordinates[Axis.X])))}\n");
        }

        sb.Append($"\nIterations: {res.Iterations}\n");
        sb.Append($"Residual: {res.Residual}\n");
        sb.Append($"Error: {res.Error}\n");

        sb.Append($"Auto Relax: {Accuracy.AutoRelax}\n");
        sb.Append($"Relax Ratio: {res.RelaxRatio}");

        dispatcher.InvokeAsync(() => Result = sb.ToString());
    }
}