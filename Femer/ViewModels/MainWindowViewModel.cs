using System;
using System.Collections.ObjectModel;
using System.Text;
using FiniteElementsMethod.Fem;
using FiniteElementsMethod.Models;
using ReactiveUI;

namespace Femer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public void Solve()
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
                sb.Append($"{res[i]}, ");
            }

            sb.Append($"{res[^1]}");

            Result = sb.ToString();
        }

        public Area Area { get; } = new();

        public InputFuncs InputFuncs { get; } = new();

        public ObservableCollection<string> BoundaryConditionItems { get; } = new()
        {
            "First",
            "Second",
            "Third"
        };

        public ObservableCollection<Tuple<double, double>> ResultTable { get; } = new();

        public BoundaryConditions BoundaryConditions { get; } = new();

        public Accuracy Accuracy { get; } = new();

        private string _result;

        public string Result
        {
            get => _result;
            set => this.RaiseAndSetIfChanged(ref _result, value);
        }
    }
}