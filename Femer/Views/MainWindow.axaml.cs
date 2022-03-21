using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Femer.ViewModels;

namespace Femer.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Solve_OnClick(object? sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainWindowViewModel;

            try
            {
                viewModel.Solve();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}