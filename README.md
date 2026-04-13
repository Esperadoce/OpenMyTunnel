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
- Config persisted locally - no secrets ever written to disk
- Start minimised to tray option
- Cross-platform: Windows, Linux, macOS
- Single self-contained native binary - no .NET runtime required (AOT compiled)

## Download

Pre-built binaries are attached to each [GitHub Release](https://github.com/Esperadoce/OpenMyTunnel/releases).

| Platform | Archive |
|---|---|
| Windows x64 | `OpenMyTunnel-vX.X.X-win-x64.zip` |
| Linux x64 | `OpenMyTunnel-vX.X.X-linux-x64.tar.gz` |
| macOS Apple Silicon | `OpenMyTunnel-vX.X.X-osx-arm64.tar.gz` |

Intel Mac users: build from source (see below).

## Build from source

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Native C toolchain:
  - Windows: Visual Studio Build Tools (MSVC)
  - Linux: `clang` and `zlib1g-dev`
  - macOS: Xcode Command Line Tools (`xcode-select --install`)

### Clone

```bash
git clone https://github.com/Esperadoce/OpenMyTunnel.git
cd OpenMyTunnel
```

### Run in development

```bash
dotnet run --project src/OpenMyTunnel/OpenMyTunnel.csproj
```

### Publish a native AOT binary

**Windows (x64)**

```powershell
dotnet publish src/OpenMyTunnel/OpenMyTunnel.csproj -c Release -r win-x64
```

Output: `src/OpenMyTunnel/bin/Release/net10.0/win-x64/publish/OpenMyTunnel.exe`

**Linux (x64)**

```bash
sudo apt-get install -y clang zlib1g-dev
dotnet publish src/OpenMyTunnel/OpenMyTunnel.csproj -c Release -r linux-x64
```

Output: `src/OpenMyTunnel/bin/Release/net10.0/linux-x64/publish/OpenMyTunnel`

**macOS Apple Silicon (arm64)**

```bash
dotnet publish src/OpenMyTunnel/OpenMyTunnel.csproj -c Release -r osx-arm64
```

Output: `src/OpenMyTunnel/bin/Release/net10.0/osx-arm64/publish/OpenMyTunnel`

**macOS Intel (x64)**

```bash
dotnet publish src/OpenMyTunnel/OpenMyTunnel.csproj -c Release -r osx-x64
```

Output: `src/OpenMyTunnel/bin/Release/net10.0/osx-x64/publish/OpenMyTunnel`

## Usage

1. Enter the server **Host** (IP or hostname) and **SSH Port** (default 22)
2. Enter your **Username**
3. Choose **Password** or **SSH Key** authentication and fill in the credentials
4. Set the **Local SOCKS Port** (default 1080)
5. Click **Connect**
6. Configure your browser or system proxy to use `127.0.0.1:1080` (SOCKS5)
7. Click **Minimize to Tray** to keep the tunnel running in the background
8. Right-click the tray icon to connect, disconnect, or exit

## Configuration

Non-secret settings are saved automatically to:

| Platform | Path |
|---|---|
| Windows | `%LOCALAPPDATA%\OpenMyTunnel\config.json` |
| Linux | `~/.config/OpenMyTunnel/config.json` |
| macOS | `~/Library/Application Support/OpenMyTunnel/config.json` |

Passwords and passphrases are never written to disk.

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
