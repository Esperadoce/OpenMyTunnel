using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using OpenMyTunnel.ViewModels;

namespace OpenMyTunnel.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Hide to tray instead of closing the application.
        Closing += (_, e) =>
        {
            e.Cancel = true;
            Hide();
        };
    }

    private void OnMinimizeToTray(object? sender, RoutedEventArgs e) => Hide();

    private async void OnBrowseKeyFile(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title         = "Select SSH Private Key",
            AllowMultiple = false
        });

        if (files.Count > 0 && DataContext is MainViewModel vm)
            vm.KeyFilePath = files[0].Path.LocalPath;
    }
}
