using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Femer.ViewModels;

namespace Femer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Solve_OnClick(object? sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;

        try
        {
            SolveButton.IsEnabled = false;
            SolveButton.Content = "In progress";

            StatusLabel.Content = "✨ Doing magic...";
            await Task.Run(() => viewModel!.Solve(Dispatcher.UIThread));
            StatusLabel.Content = "🤗 Solved!";
        }
        catch (Exception exception)
        {
            StatusLabel.Content = $"⛔️ Error while solving occured: {exception.Message}";
            Console.WriteLine(exception.Message);
        }
        finally
        {
            SolveButton.Content = "Solve";
            SolveButton.IsEnabled = true;
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
        try
        {
            CopyButton.IsEnabled = false;

            // Checking for null
            if (Application.Current is {Clipboard: { }})
            {
                await Application.Current.Clipboard.SetTextAsync(ResultBox.Text);
            }
            else
            {
                StatusLabel.Content = "⛔️ Error while copy occured: Nothing to copy. Values is null";
            }

            TextBox_OnCopyingToClipboard(sender, e);
        }
        catch (Exception exception)
        {
            StatusLabel.Content = $"⛔️ Error while copy occured: {exception.Message}";
            Console.WriteLine(exception.Message);
        }
        finally
        {
            CopyButton.IsEnabled = true;
        }
    }
}