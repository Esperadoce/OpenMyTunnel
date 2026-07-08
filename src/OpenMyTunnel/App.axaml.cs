using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using OpenMyTunnel.Tray;
using OpenMyTunnel.ViewModels;
using OpenMyTunnel.Views;

namespace OpenMyTunnel;

public partial class App : Application
{
    private TrayManager? _tray;
    private MainWindow? _window;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Keep the app alive when the window is closed (tray-only mode).
            desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnExplicitShutdown;

            var vm = new MainViewModel();
            _window = new MainWindow { DataContext = vm };

            _tray = new TrayManager(vm, _window);

            if (vm.StartMinimised)
            {
                // Do not assign MainWindow so the window stays hidden on start.
                desktop.MainWindow = null;
                TrayNotification.ShowTrayStartup(onClicked: () =>
                {
                    _window.Show();
                    _window.Activate();
                });
            }
            else
            {
                desktop.MainWindow = _window;
                _window.Show();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    // Called from the background show-window listener thread in Program.cs.
    internal void ShowMainWindow()
    {
        _window?.Show();
        _window?.Activate();
    }
}
