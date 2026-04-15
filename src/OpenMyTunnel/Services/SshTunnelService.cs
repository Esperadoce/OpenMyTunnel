using Renci.SshNet;
using OpenMyTunnel.Models;

namespace OpenMyTunnel.Services;

public enum TunnelStatus
{
    Disconnected,
    Connecting,
    Connected,
    Error
}

public sealed class SshTunnelService : IDisposable
{
    private SshClient?            _client;
    private ForwardedPortDynamic? _port;
    private readonly Lock         _gate = new();

    public TunnelStatus Status { get; private set; } = TunnelStatus.Disconnected;

    public event EventHandler<TunnelStatus>? StatusChanged;
    public event EventHandler<string>?       ErrorOccurred;

    // Connects and starts the SOCKS5 dynamic port forward.
    // Equivalent to: ssh -D localSocksPort -N username@host -p sshPort
    public async Task ConnectAsync(
        TunnelConfig config,
        string       password,
        string       passphrase,
        CancellationToken ct = default)
    {
        lock (_gate)
        {
            if (Status is TunnelStatus.Connected or TunnelStatus.Connecting)
                return;
            SetStatus(TunnelStatus.Connecting);
        }

        try
        {
            await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                var authMethods = config.AuthMode == AuthMode.PrivateKey
                    ? BuildKeyAuthMethods(config, passphrase, password)
                    : new AuthenticationMethod[] { BuildPasswordAuth(config, password) };

                var connInfo = new ConnectionInfo(
                    config.Host,
                    config.SshPort,
                    config.Username,
                    authMethods)
                {
                    Timeout = TimeSpan.FromSeconds(20)
                };

                lock (_gate)
                {
                    _client = new SshClient(connInfo);
                }

                _client.Connect();
                ct.ThrowIfCancellationRequested();

                var port = new ForwardedPortDynamic("127.0.0.1", (uint)config.LocalSocksPort);
                _client.AddForwardedPort(port);
                port.Start();

                lock (_gate) { _port = port; }

            }, ct);

            SetStatus(TunnelStatus.Connected);
        }
        catch (OperationCanceledException)
        {
            Cleanup();
            SetStatus(TunnelStatus.Disconnected);
        }
        catch (Exception ex)
        {
            Cleanup();
            ErrorOccurred?.Invoke(this, ex.Message);
            SetStatus(TunnelStatus.Error);
        }
    }

    public void Disconnect()
    {
        Cleanup();
        SetStatus(TunnelStatus.Disconnected);
    }

    private static AuthenticationMethod BuildPasswordAuth(TunnelConfig config, string password)
        => new PasswordAuthenticationMethod(config.Username, password);

    private static AuthenticationMethod[] BuildKeyAuthMethods(
        TunnelConfig config, string passphrase, string password)
    {
        var key = string.IsNullOrEmpty(passphrase)
            ? new PrivateKeyFile(config.KeyFilePath)
            : new PrivateKeyFile(config.KeyFilePath, passphrase);

        var keyMethod = new PrivateKeyAuthenticationMethod(config.Username, key);

        if (string.IsNullOrEmpty(password))
            return [keyMethod];

        // Some servers require key + password (multi-factor). SSH.NET tries methods in order.
        return [keyMethod, new PasswordAuthenticationMethod(config.Username, password)];
    }

    private void Cleanup()
    {
        lock (_gate)
        {
            try { _port?.Stop();         } catch { /* best-effort */ }
            try { _port?.Dispose();      } catch { }
            try { _client?.Disconnect(); } catch { }
            try { _client?.Dispose();    } catch { }
            _port   = null;
            _client = null;
        }
    }

    private void SetStatus(TunnelStatus status)
    {
        Status = status;
        StatusChanged?.Invoke(this, status);
    }

    public void Dispose() => Cleanup();
}
