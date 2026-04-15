using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace OpenMyTunnel.Views;

public partial class TrayNotification : Window
{
    private TrayNotification()
    {
        InitializeComponent();
    }

    // Shows the toast in the bottom-right corner and auto-closes after displayFor.
    public static void Show(TimeSpan displayFor)
    {
        var toast = new TrayNotification();

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
            wa.Right  - (int)(280 * screen.Scaling) - 16,
            wa.Bottom - (int)( 68 * screen.Scaling) - 16);
    }
}
