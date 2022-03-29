using System;
using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Threading;
using FiniteElementsMethod;
using FiniteElementsMethod.Fem;
using FiniteElementsMethod.Models;
using ReactiveUI;

namespace Femer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public void Solve(Dispatcher dispatcher)
        {
            var grid = new Grid(Area);

            var res = Solver.SolveWithSimpleIteration(
                grid,
                InputFuncs,
                Area,
                BoundaryConditions,
                Accuracy
            );
            var calc = new Sprache.Calc.XtensibleCalculator();
            var uStar = calc.ParseFunction(InputFuncs.UStar).Compile();

            var sb = new StringBuilder("u:\n");

            foreach (var val in res.Values)
            {
                sb.Append($"\t{val}\n");
            }

            sb.Append("\nu*:\n");

            for (var i = 0; i < res.Values.Length; i++)
            {
                sb.Append($"\t{uStar(Utils.MakeDict1D(grid.X[i]))}\n");
            }

            sb.Append("\n|u - u*|:\n");

            for (var i = 0; i < res.Values.Length; i++)
            {
                sb.Append($"\t{Math.Abs(res.Values[i] - uStar(Utils.MakeDict1D(grid.X[i])))}\n");
            }

            sb.Append($"\nIterations: {res.Iterations}\n");
            sb.Append($"Residual: {res.Residual}\n");
            sb.Append($"Error: {res.Error}\n");

            dispatcher.InvokeAsync(() => Result = sb.ToString());
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

        private string _result = string.Empty;

        public string Result
        {
            get => _result;
            set => this.RaiseAndSetIfChanged(ref _result, value);
        }

        private string _statusLabelContent = string.Empty;

        public string StatusLabelContent
        {
            get => _statusLabelContent;
            set => this.RaiseAndSetIfChanged(ref _result, value);
        }
    }
}