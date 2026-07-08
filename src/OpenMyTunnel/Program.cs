using System.Runtime.Versioning;
using System.Threading;
using Avalonia;
using Avalonia.Logging;
using OpenMyTunnel.Tui;

namespace OpenMyTunnel;

internal static class Program
{
    private const string MutexName = "OpenMyTunnel_SingleInstance";
    private const string ShowEventName = "OpenMyTunnel_ShowWindow";

    [STAThread]
    public static int Main(string[] args)
    {
        bool tuiMode = Array.Exists(args, a => a is "--tui" or "-t");

        using var mutex = new Mutex(initiallyOwned: true, MutexName, out bool isNewInstance);
        if (!isNewInstance)
        {
            // The app is already running (likely hidden in the tray).
            // Signal it to bring its window to the front, then exit cleanly.
            if (OperatingSystem.IsWindows())
                SignalExistingInstance();
            return 0;
        }

        if (tuiMode)
        {
            if (OperatingSystem.IsWindows())
                NativeConsole.Alloc();

            TuiApp.Run();
            return 0;
        }

        if (OperatingSystem.IsWindows())
            StartShowWindowListener();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        return 0;
    }

    [SupportedOSPlatform("windows")]
    private static void SignalExistingInstance()
    {
        try
        {
            using var ev = EventWaitHandle.OpenExisting(ShowEventName);
            ev.Set();
        }
        catch (WaitHandleCannotBeOpenedException) { /* First instance already exited. */ }
    }

    [SupportedOSPlatform("windows")]
    private static void StartShowWindowListener()
    {
        var showEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ShowEventName);

        // Background thread: waits for a second-instance signal and brings the window forward.
        new Thread(() =>
        {
            try
            {
                while (showEvent.WaitOne())
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        if (Application.Current is App app)
                            app.ShowMainWindow();
                    });
                }
            }
            catch (ObjectDisposedException) { /* Normal on shutdown. */ }
        })
        { IsBackground = true, Name = "ShowWindowListener" }.Start();
    }

    private static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
                  .UsePlatformDetect()
                  .LogToTrace();
}
