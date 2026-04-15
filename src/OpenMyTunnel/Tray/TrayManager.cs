using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using OpenMyTunnel.Services;
using OpenMyTunnel.ViewModels;
using OpenMyTunnel.Views;

namespace OpenMyTunnel.Tray;

public sealed class TrayManager : IDisposable
{
    private readonly TrayIcon _icon;
    private readonly MainViewModel _vm;
    private readonly MainWindow _window;

    public TrayManager(MainViewModel vm, MainWindow window)
    {
        _vm = vm;
        _window = window;

        _icon = new TrayIcon
        {
            ToolTipText = "OpenMyTunnel - Disconnected",
            Icon = BuildIcon(Color.Parse("#6B7280")),
            Menu = BuildMenu()
        };

        _icon.Clicked += (_, _) => ShowWindow();

        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.TunnelStatus))
                UpdateIcon();
        };

        // Register with Avalonia so the icon appears in the tray.
        TrayIcon.SetIcons(Application.Current!, [_icon]);
    }

    // ------------------------------------------------------------------ private

    private NativeMenu BuildMenu()
    {
        var menu = new NativeMenu();

        menu.Items.Add(new NativeMenuItem
        {
            Header = "Open",
            Command = new RelayCommand(ShowWindow)
        });

        menu.Items.Add(new NativeMenuItemSeparator());

        menu.Items.Add(new NativeMenuItem
        {
            Header = "Connect",
            Command = _vm.ConnectCommand
        });

        menu.Items.Add(new NativeMenuItem
        {
            Header = "Disconnect",
            Command = _vm.DisconnectCommand
        });

        menu.Items.Add(new NativeMenuItemSeparator());

        menu.Items.Add(new NativeMenuItem
        {
            Header = "Exit",
            Command = new RelayCommand(Exit)
        });

        return menu;
    }

    private void ShowWindow()
    {
        _window.Show();
        _window.Activate();
        _window.BringIntoView();
    }

    private static void Exit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime app)
            app.Shutdown();
    }

    private void UpdateIcon()
    {
        var color = _vm.TunnelStatus switch
        {
            TunnelStatus.Disconnected => Color.Parse("#6B7280"),
            TunnelStatus.Connecting => Color.Parse("#F59E0B"),
            TunnelStatus.Connected => Color.Parse("#10B981"),
            TunnelStatus.Error => Color.Parse("#EF4444"),
            _ => Color.Parse("#6B7280")
        };

        _icon.Icon = BuildIcon(color);
        _icon.ToolTipText = $"OpenMyTunnel - {_vm.StatusText}";
    }

    // Draws a filled circle on a 32x32 WriteableBitmap.
    private static WindowIcon BuildIcon(Color fill)
    {
        const int size = 32;
        var bmp = new WriteableBitmap(
            new PixelSize(size, size),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);

        using var locked = bmp.Lock();

        unsafe
        {
            var ptr = (byte*)locked.Address;
            float cx = size * 0.5f, cy = size * 0.5f, r = size * 0.42f;

            // Premultiply the alpha channel.
            byte a = fill.A;
            byte pr = (byte)(fill.R * a / 255);
            byte pg = (byte)(fill.G * a / 255);
            byte pb = (byte)(fill.B * a / 255);

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                    int i = y * locked.RowBytes + x * 4;

                    if (dx * dx + dy * dy <= r * r)
                    {
                        ptr[i] = pb; // B
                        ptr[i + 1] = pg; // G
                        ptr[i + 2] = pr; // R
                        ptr[i + 3] = a;  // A
                    }
                    else
                    {
                        ptr[i] = ptr[i + 1] = ptr[i + 2] = ptr[i + 3] = 0;
                    }
                }
        }

        return new WindowIcon(bmp);
    }

    public void Dispose() => _icon.Dispose();
}
