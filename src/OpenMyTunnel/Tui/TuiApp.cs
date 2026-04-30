using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using OpenMyTunnel.Models;
using OpenMyTunnel.Services;

namespace OpenMyTunnel.Tui;

internal static class TuiApp
{
    public static void Run()
    {
        var app = Application.Create();
        app.Init();
        RunApp(app);
    }

    private static void RunApp(IApplication app)
    {
        var config = ConfigService.Load();
        using var tunnel = new SshTunnelService();
        using var cts = new CancellationTokenSource();

        // ── Server ──────────────────────────────────────────────────────────
        var hostField = new TextField { Text = config.Host,               X = 14, Y = 0, Width = Dim.Fill() - 2 };
        var portField = new TextField { Text = config.SshPort.ToString(), X = 14, Y = 1, Width = 8 };
        var userField = new TextField { Text = config.Username,           X = 14, Y = 2, Width = Dim.Fill() - 2 };

        var serverFrame = new FrameView { Title = "Server", X = 1, Y = 0, Width = Dim.Fill() - 2, Height = 5 };
        serverFrame.Add(
            new Label { Text = "Host:",     X = 1, Y = 0 }, hostField,
            new Label { Text = "SSH Port:", X = 1, Y = 1 }, portField,
            new Label { Text = "Username:", X = 1, Y = 2 }, userField);

        // ── Authentication ───────────────────────────────────────────────────
        var authSelector = new OptionSelector<AuthMode>
        {
            X = 1, Y = 0,
            Value = config.AuthMode
        };

        bool initPwd = config.AuthMode == AuthMode.Password;
        var passLabel   = new Label     { Text = "Password:",   X = 1, Y = 3, Visible = initPwd };
        var passField   = new TextField { X = 14, Y = 3, Width = Dim.Fill() - 2, Secret = true, Visible = initPwd };
        var keyLabel    = new Label     { Text = "Key File:",   X = 1, Y = 3, Visible = !initPwd };
        var keyField    = new TextField { Text = config.KeyFilePath, X = 14, Y = 3, Width = Dim.Fill() - 2, Visible = !initPwd };
        var phraseLabel = new Label     { Text = "Passphrase:", X = 1, Y = 4, Visible = !initPwd };
        var phraseField = new TextField { X = 14, Y = 4, Width = Dim.Fill() - 2, Secret = true, Visible = !initPwd };

        authSelector.ValueChanged += (_, _) =>
        {
            bool isPwd = authSelector.Value == AuthMode.Password;
            passLabel.Visible  = isPwd;
            passField.Visible  = isPwd;
            keyLabel.Visible   = !isPwd;
            keyField.Visible   = !isPwd;
            phraseLabel.Visible = !isPwd;
            phraseField.Visible = !isPwd;
        };

        var authFrame = new FrameView { Title = "Authentication", X = 1, Y = 6, Width = Dim.Fill() - 2, Height = 8 };
        authFrame.Add(authSelector, passLabel, passField, keyLabel, keyField, phraseLabel, phraseField);

        // ── Tunnel ───────────────────────────────────────────────────────────
        var socksField  = new TextField { Text = config.LocalSocksPort.ToString(), X = 14, Y = 0, Width = 8 };
        var tunnelFrame = new FrameView { Title = "Tunnel", X = 1, Y = 15, Width = Dim.Fill() - 2, Height = 3 };
        tunnelFrame.Add(new Label { Text = "SOCKS Port:", X = 1, Y = 0 }, socksField);

        // ── Status & error ───────────────────────────────────────────────────
        var statusLabel = new Label { Text = "○  Disconnected", X = 2, Y = 19 };
        var errorLabel  = new Label { Text = "",                X = 2, Y = 20 };

        // ── Buttons ──────────────────────────────────────────────────────────
        var connectBtn = new Button { Text = "Connect", X = Pos.Center() - 9, Y = 22, IsDefault = true };
        var quitBtn    = new Button { Text = "Quit",    X = Pos.Center() + 3, Y = 22 };

        // ── Tunnel events ─────────────────────────────────────────────────────
        tunnel.StatusChanged += (_, status) =>
        {
            app.Invoke(() =>
            {
                statusLabel.Text   = status switch
                {
                    TunnelStatus.Connected  => "●  Connected",
                    TunnelStatus.Connecting => "◐  Connecting...",
                    TunnelStatus.Error      => "✗  Error",
                    _                       => "○  Disconnected",
                };
                connectBtn.Enabled = status is not TunnelStatus.Connecting;
                connectBtn.Text    = status == TunnelStatus.Connected ? "Disconnect" : "Connect";
            });
        };

        tunnel.ErrorOccurred += (_, msg) =>
            app.Invoke(() => errorLabel.Text = msg);

        // ── Button actions ────────────────────────────────────────────────────
        connectBtn.Accepting += (_, _) =>
        {
            if (tunnel.Status == TunnelStatus.Connected)
            {
                tunnel.Disconnect();
                return;
            }

            errorLabel.Text       = "";
            config.Host           = hostField.Text;
            config.SshPort        = int.TryParse(portField.Text, out var p)  ? p  : 22;
            config.Username       = userField.Text;
            config.AuthMode       = authSelector.Value ?? AuthMode.Password;
            config.LocalSocksPort = int.TryParse(socksField.Text, out var sp) ? sp : 1080;
            config.KeyFilePath    = keyField.Text;
            ConfigService.Save(config);

            _ = tunnel.ConnectAsync(config, passField.Text, phraseField.Text, cts.Token);
        };

        quitBtn.Accepting += (_, _) =>
        {
            cts.Cancel();
            tunnel.Disconnect();
            app.RequestStop();
        };

        // ── Window ────────────────────────────────────────────────────────────
        var win = new Window
        {
            Title  = "OpenMyTunnel  –  SSH SOCKS5 Tunnel",
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        win.Add(serverFrame, authFrame, tunnelFrame, statusLabel, errorLabel, connectBtn, quitBtn);

        app.Run(win);
    }
}
