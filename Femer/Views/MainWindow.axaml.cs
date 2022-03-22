using System;
using System.Threading.Tasks;
using Avalonia;
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
                Console.WriteLine(exception.Message);
            }
        }
        private async void TextBox_OnCopyingToClipboard(object? sender, RoutedEventArgs e)
        {
            CopyTextBlock.IsVisible = true;
            await Task.Delay(3000);
            CopyTextBlock.IsVisible = false;
        }
        private async void Button_CopyToClipboard_OnClick(object? sender, RoutedEventArgs e)
        {
            await Application.Current.Clipboard.SetTextAsync(ResultBox.Text);
            TextBox_OnCopyingToClipboard(sender, e);
        }
    }
}