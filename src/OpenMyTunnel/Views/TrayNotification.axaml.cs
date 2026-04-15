using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace OpenMyTunnel.Views;

public partial class TrayNotification : Window
{
    public TrayNotification()
    {
        InitializeComponent();
    }

    // ------------------------------------------------------------------ public API

    /// <summary>
    /// Shows an appropriate startup notification on every platform.
    /// onClicked is invoked when the user clicks the toast (Windows only).
    /// </summary>
    public static void ShowTrayStartup(Action? onClicked = null)
    {
        if (OperatingSystem.IsLinux())
        {
            // Try the standard freedesktop notification daemon first.
            // Falls back to the Avalonia toast if notify-send is not installed
            // (works on X11; on Wayland the compositor decides placement).
            if (!TryProcess("notify-send", "-a", "OpenMyTunnel", "-t", "4000",
                            "OpenMyTunnel", "Running in the system tray"))
            {
                ShowWindowsToast(TimeSpan.FromSeconds(4), onClicked);
            }
        }
        else if (OperatingSystem.IsMacOS())
        {
            if (!TryProcess("osascript", "-e",
                            "display notification \"Running in the system tray\" with title \"OpenMyTunnel\""))
            {
                ShowWindowsToast(TimeSpan.FromSeconds(4), onClicked);
            }
        }
        else
        {
            ShowWindowsToast(TimeSpan.FromSeconds(4), onClicked);
        }
    }

    // ------------------------------------------------------------------ Windows toast

    private static void ShowWindowsToast(TimeSpan displayFor, Action? onClicked)
    {
        var toast = new TrayNotification();

        toast.PointerPressed += (_, _) =>
        {
            toast.Close();
            onClicked?.Invoke();
        };

        toast.Opened += (_, _) => PositionBottomRight(toast);
        toast.Show();

        var timer = new DispatcherTimer { Interval = displayFor };
        timer.Tick += (_, _) => { timer.Stop(); toast.Close(); };
        timer.Start();
    }

    private static void PositionBottomRight(Window w)
    {
        var screen = w.Screens.Primary;
        if (screen is null) return;

        var wa = screen.WorkingArea;
        w.Position = new PixelPoint(
            wa.Right - (int)(280 * screen.Scaling) - 16,
            wa.Bottom - (int)(68 * screen.Scaling) - 16);
    }

    // ------------------------------------------------------------------ helpers

    // Returns true if the process launched successfully, false if the tool is not available.
    private static bool TryProcess(string exe, params string[] args)
    {
        try
        {
            var psi = new ProcessStartInfo(exe)
            {
                UseShellExecute = false,
                CreateNoWindow = true
            };
            foreach (var a in args) psi.ArgumentList.Add(a);
            Process.Start(psi);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
