using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Media;
using OpenMyTunnel.Models;
using OpenMyTunnel.Services;

namespace OpenMyTunnel.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly SshTunnelService _tunnel;

    // ------------------------------------------------------------------ config fields

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CommandPreview))]
    private string _host = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CommandPreview))]
    private int _sshPort = 22;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CommandPreview))]
    private string _username = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPasswordMode))]
    [NotifyPropertyChangedFor(nameof(IsKeyMode))]
    private AuthMode _authMode = AuthMode.Password;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(KeyFileName))]
    [NotifyPropertyChangedFor(nameof(HasKeyFile))]
    private string _keyFilePath = string.Empty;

    public string KeyFileName => Path.GetFileName(KeyFilePath);
    public bool   HasKeyFile  => !string.IsNullOrEmpty(KeyFilePath);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CommandPreview))]
    private int _localSocksPort = 1080;

    [ObservableProperty]
    private bool _startMinimised;

    // Credentials - held in memory only, never persisted.
    public string Password   { get; set; } = string.Empty;
    public string Passphrase { get; set; } = string.Empty;

    // ------------------------------------------------------------------ status

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(StatusBrush))]
    [NotifyPropertyChangedFor(nameof(ConnectButtonText))]
    [NotifyPropertyChangedFor(nameof(IsConnecting))]
    [NotifyPropertyChangedFor(nameof(IsConnected))]
    [NotifyCanExecuteChangedFor(nameof(DisconnectCommand))]
    private TunnelStatus _tunnelStatus = TunnelStatus.Disconnected;

    public string StatusText => TunnelStatus switch
    {
        TunnelStatus.Disconnected => "Disconnected",
        TunnelStatus.Connecting   => "Connecting...",
        TunnelStatus.Connected    => "Connected",
        TunnelStatus.Error        => "Error",
        _                         => "Unknown"
    };

    public SolidColorBrush StatusBrush => TunnelStatus switch
    {
        TunnelStatus.Disconnected => new SolidColorBrush(Color.Parse("#6B7280")),
        TunnelStatus.Connecting   => new SolidColorBrush(Color.Parse("#F59E0B")),
        TunnelStatus.Connected    => new SolidColorBrush(Color.Parse("#10B981")),
        TunnelStatus.Error        => new SolidColorBrush(Color.Parse("#EF4444")),
        _                         => new SolidColorBrush(Colors.Gray)
    };

    public string ConnectButtonText =>
        TunnelStatus == TunnelStatus.Connected ? "Disconnect" : "Connect";

    public bool IsConnecting => TunnelStatus == TunnelStatus.Connecting;
    public bool IsConnected  => TunnelStatus == TunnelStatus.Connected;

    // ------------------------------------------------------------------ derived

    public bool IsPasswordMode
    {
        get => AuthMode == AuthMode.Password;
        set { if (value) AuthMode = AuthMode.Password; }
    }

    public bool IsKeyMode
    {
        get => AuthMode == AuthMode.PrivateKey;
        set { if (value) AuthMode = AuthMode.PrivateKey; }
    }

    public string CommandPreview =>
        $"ssh -D {LocalSocksPort} -N {Username}@{Host} -p {SshPort}";

    // ------------------------------------------------------------------ constructor

    public MainViewModel()
    {
        _tunnel = new SshTunnelService();
        _tunnel.StatusChanged += (_, s) => Avalonia.Threading.Dispatcher.UIThread.Post(() => TunnelStatus = s);
        _tunnel.ErrorOccurred += (_, msg) => Avalonia.Threading.Dispatcher.UIThread.Post(() => OnError(msg));

        var config = ConfigService.Load();
        ApplyConfig(config);
    }

    // ------------------------------------------------------------------ commands

    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (TunnelStatus == TunnelStatus.Connected)
        {
            _tunnel.Disconnect();
            return;
        }

        var config = BuildConfig();
        ConfigService.Save(config);
        await _tunnel.ConnectAsync(config, Password, Passphrase);
    }

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    private void Disconnect() => _tunnel.Disconnect();
    private bool CanDisconnect() => TunnelStatus == TunnelStatus.Connected;

    [RelayCommand]
    private void ClearKeyFile() => KeyFilePath = string.Empty;

    [RelayCommand]
    private void MinimizeToTray() { /* raised to the view via event */ }

    // ------------------------------------------------------------------ helpers

    private void ApplyConfig(TunnelConfig c)
    {
        Host           = c.Host;
        SshPort        = c.SshPort;
        Username       = c.Username;
        LocalSocksPort = c.LocalSocksPort;
        AuthMode       = c.AuthMode;
        KeyFilePath    = c.KeyFilePath;
        StartMinimised = c.StartMinimised;
    }

    private TunnelConfig BuildConfig() => new()
    {
        Host           = Host,
        SshPort        = SshPort,
        Username       = Username,
        LocalSocksPort = LocalSocksPort,
        AuthMode       = AuthMode,
        KeyFilePath    = KeyFilePath,
        StartMinimised = StartMinimised
    };

    private void OnError(string message)
    {
        // Error already reflected in TunnelStatus; message surfaced via ErrorMessage binding.
        ErrorMessage = message;
    }

    [ObservableProperty]
    private string _errorMessage = string.Empty;
}
