# OpenMyTunnel

A lightweight SSH SOCKS5 tunnel manager. Configure your server once, click Connect, and the tunnel runs quietly in the system tray.

Now includes a **Terminal UI (TUI)** for headless environments and keyboard-driven workflows.

Equivalent to:
```bash
ssh -D 1080 -N user@host -p 22
```

Point your browser or system proxy at `127.0.0.1:1080` (SOCKS5) and you're done.

---

## Features

- **Dual Interface**: Modern Avalonia desktop UI with system tray support, or a full-featured Terminal UI (TUI).
- **One-Click Tunnel**: Quickly establish SOCKS5 dynamic port forwarding via SSH.
- **Flexible Auth**: Supports passwords, private keys (with passphrases), and multi-factor (key + password).
- **Visual Status**: Colour-coded indicators (Grey/Amber/Green/Red) in both Tray and TUI.
- **Stealth Mode**: Start minimised to the system tray for a clean workspace.
- **Privacy Centric**: All settings are stored locally. Passwords and passphrases are never persisted to disk.
- **Native & Portable**: AOT compiled for instant startup and zero dependencies. No .NET runtime required.
- **Cross-platform**: Windows (x64), Linux (x64), and macOS (Apple Silicon & Intel).

## Usage

### Graphical Mode (Default)
Simply run `OpenMyTunnel`. It opens the configuration window and minimizes to the tray when closed.

### Terminal Mode
Run with the `--tui` or `-t` flag:
```bash
./OpenMyTunnel --tui
```
This mode is perfect for remote servers or terminal-only setups.

## Download

Pre-built binaries are available on the [GitHub Releases](https://github.com/Esperadoce/OpenMyTunnel/releases) page.

| Platform | File |
|---|---|
| Windows x64 | `OpenMyTunnel-vX.X.X-win-x64.zip` |
| Linux x64 | `OpenMyTunnel-vX.X.X-linux-x64.tar.gz` |
| macOS Apple Silicon | `OpenMyTunnel-vX.X.X-osx-arm64.tar.gz` |
| macOS Intel | `OpenMyTunnel-vX.X.X-osx-x64.tar.gz` |

> **Windows note:** The zip archive includes `OpenMyTunnel.exe` and necessary native libraries (like `libSkiaSharp.dll`). Ensure all files remain in the same directory.

## Build from Source

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) and a native C toolchain (MSVC on Windows, `clang` + `zlib1g-dev` on Linux, Xcode CLI tools on macOS).

```bash
git clone https://github.com/Esperadoce/OpenMyTunnel.git
cd OpenMyTunnel

# Run in Development (GUI)
dotnet run --project src/OpenMyTunnel/OpenMyTunnel.csproj

# Run in Development (TUI)
dotnet run --project src/OpenMyTunnel/OpenMyTunnel.csproj -- --tui

# Publish AOT - Windows
dotnet publish src/OpenMyTunnel/OpenMyTunnel.csproj -c Release -r win-x64

# Publish AOT - Linux
sudo apt-get install -y clang zlib1g-dev
dotnet publish src/OpenMyTunnel/OpenMyTunnel.csproj -c Release -r linux-x64

# Publish AOT - macOS Apple Silicon
dotnet publish src/OpenMyTunnel/OpenMyTunnel.csproj -c Release -r osx-arm64

# Publish AOT - macOS Intel
dotnet publish src/OpenMyTunnel/OpenMyTunnel.csproj -c Release -r osx-x64
```

## Configuration

Configuration is stored in the following platform-specific locations:

| Platform | Path |
|---|---|
| Windows | `%LOCALAPPDATA%\OpenMyTunnel\config.json` |
| Linux | `~/.local/share/OpenMyTunnel/config.json` |
| macOS | `~/Library/Application Support/OpenMyTunnel/config.json` |

## Technology Stack

- **Framework**: .NET 10 (Native AOT)
- **Desktop UI**: Avalonia UI 12
- **Terminal UI**: Terminal.Gui
- **SSH Protocol**: SSH.NET
- **Serialisation**: System.Text.Json (Source Generated)

## About

Developed by **Esperadoce** (Hicham Bouchikhi) - hicham@bouchikhi.net

## License

[MIT](LICENSE)
