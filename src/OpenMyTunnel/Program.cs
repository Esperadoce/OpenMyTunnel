using System.Threading;
using Avalonia;
using OpenMyTunnel.Tui;

namespace OpenMyTunnel;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        bool tuiMode = Array.Exists(args, a => a is "--tui" or "-t");

        using var mutex = new Mutex(initiallyOwned: true, "OpenMyTunnel_SingleInstance", out bool isNewInstance);
        if (!isNewInstance)
            return;

        if (tuiMode)
        {
            if (OperatingSystem.IsWindows())
                NativeConsole.Alloc();

            TuiApp.Run();
            return;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
                  .UsePlatformDetect()
                  .LogToTrace();
}
