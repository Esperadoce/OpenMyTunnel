namespace OpenMyTunnel.Models;

public enum AuthMode
{
    Password,
    PrivateKey
}

public sealed class TunnelConfig
{
    public string  Host            { get; set; } = string.Empty;
    public int     SshPort         { get; set; } = 22;
    public string  Username        { get; set; } = string.Empty;
    public int     LocalSocksPort  { get; set; } = 1080;
    public AuthMode AuthMode       { get; set; } = AuthMode.Password;
    public string  KeyFilePath     { get; set; } = string.Empty;
    public bool    StartMinimised  { get; set; } = false;
    // Passwords and passphrases are never persisted.
}
