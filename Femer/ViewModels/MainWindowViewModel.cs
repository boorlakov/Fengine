using System;
using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Threading;
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

            var sb = new StringBuilder();

            for (var i = 0; i < res.Length - 1; i++)
            {
                sb.Append($"{res[i]}\n");
            }

            sb.Append($"{res[^1]}");

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