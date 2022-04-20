﻿using System;
using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Threading;
using Fengine;
using Fengine.Fem;
using Fengine.Fem.Mesh;
using Fengine.Models;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Sprache.Calc;

namespace Femer.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly Solver _solver;

    private string _result = string.Empty;

    private readonly string _statusLabelContent = string.Empty;

    public MainWindowViewModel()
    {
        var serviceProvider = DependencyInjectionModule
            .ConfigureServices()
            .BuildServiceProvider();

        _solver = serviceProvider.GetService<Solver>() ?? throw new InvalidOperationException();
    }

    public Area Area { get; } = new();

    public InputFuncs InputFuncs { get; } = new();

    public ObservableCollection<string> BoundaryConditionItems { get; } = new()
    {
        "First",
        "Second",
        "Third"
    };

    public ObservableCollection<Tuple<double, double>> ResultTable { get; } = new()
    {
        new Tuple<double, double>(0.0, 0.0)
    };

    public BoundaryConditions BoundaryConditions { get; } = new();

    public Accuracy Accuracy { get; } = new();

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
        var mesh = new Cartesian1DMesh(Area);

        var res = _solver.SolveWithSimpleIteration(
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
            sb.Append($"\t{uStar(Utils.MakeDict1D(mesh.Nodes[i].Coordinates["x"]))}\n");
        }

        sb.Append("\n|u - u*|:\n");

        for (var i = 0; i < res.Values.Length; i++)
        {
            sb.Append($"\t{Math.Abs(res.Values[i] - uStar(Utils.MakeDict1D(mesh.Nodes[i].Coordinates["x"])))}\n");
        }

        sb.Append($"\nIterations: {res.Iterations}\n");
        sb.Append($"Residual: {res.Residual}\n");
        sb.Append($"Error: {res.Error}\n");

        sb.Append($"Auto Relax: {Accuracy.AutoRelax}\n");
        sb.Append($"Relax Ratio: {res.RelaxRatio}");

        dispatcher.InvokeAsync(() => Result = sb.ToString());
    }
}