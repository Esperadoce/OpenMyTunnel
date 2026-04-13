# OpenMyTunnel

A lightweight, cross-platform SSH tunnel manager that creates a SOCKS5 proxy via SSH dynamic
port forwarding. Configure once, connect with one click, control from the system tray.

Equivalent to running:

```bash
ssh -D 1080 -N user@host -p 22
```

## Features

- One-click SSH SOCKS5 tunnel (dynamic port forwarding)
- System tray icon with colour-coded status (grey / amber / green / red)
- Password and SSH private key authentication
- Config persisted locally (no secrets ever written to disk)
- Start minimised to tray option
- Cross-platform: Windows, Linux, macOS
- Single self-contained native binary (AOT compiled, no .NET runtime required)

## Screenshots

> Coming soon.

## Getting Started

### Prerequisites

- .NET 10 SDK (for building from source)
- An SSH server you can reach

### Build from source

```bash
git clone https://github.com/Esperadoce/OpenMyTunnel.git
cd OpenMyTunnel
dotnet build
dotnet run
```

### Publish a native AOT binary

```bash
# Windows
dotnet publish -c Release -r win-x64

# Linux
dotnet publish -c Release -r linux-x64

# macOS (Intel)
dotnet publish -c Release -r osx-x64

# macOS (Apple Silicon)
dotnet publish -c Release -r osx-arm64
```

Output is in `bin/Release/net10.0/<rid>/publish/`.

## Usage

1. Enter the server **Host** (IP or hostname) and **SSH Port** (default 22).
2. Enter your **Username**.
3. Choose **Password** or **SSH Key** authentication and fill in the credentials.
4. Set the **Local SOCKS Port** (default 1080).
5. Click **Connect**.
6. Configure your browser or system proxy to use `127.0.0.1:1080` (SOCKS5).
7. Click **Minimize to Tray** to keep the tunnel running in the background.
8. Right-click the tray icon to connect, disconnect, or exit.

## Configuration

Non-secret settings are saved to:

| Platform | Path |
|---|---|
| Windows | `%LOCALAPPDATA%\OpenMyTunnel\config.json` |
| Linux | `~/.config/OpenMyTunnel/config.json` |
| macOS | `~/Library/Application Support/OpenMyTunnel/config.json` |

Passwords and passphrases are **never** written to disk.

## Technology

| Component | Library |
|---|---|
| UI | Avalonia UI 11 |
| SSH | SSH.NET |
| MVVM | CommunityToolkit.Mvvm |
| Serialisation | System.Text.Json (source generated) |
| Runtime | .NET 10 (AOT) |

## Contributing

Pull requests are welcome. Please open an issue first for significant changes.

## License

[MIT](LICENSE)
