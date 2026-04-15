# OpenMyTunnel

A lightweight SSH SOCKS5 tunnel manager. Configure your server once, click Connect, and the tunnel runs quietly in the system tray.

Equivalent to:
```bash
ssh -D 1080 -N user@host -p 22
```

Point your browser or system proxy at `127.0.0.1:1080` (SOCKS5) and you're done.

---

![Screenshot](docs/screenshot.png)

---

## Features

- One-click SOCKS5 tunnel via SSH dynamic port forwarding
- Password and SSH key authentication (supports key + passphrase + server password)
- System tray icon with colour-coded status (grey / amber / green / red)
- Settings saved locally. Passwords are never written to disk.
- Start minimised to tray
- Cross-platform: Windows, Linux, macOS
- AOT compiled. No .NET runtime required.

## Download

Pre-built binaries are attached to each [GitHub Release](https://github.com/Esperadoce/OpenMyTunnel/releases).

| Platform | File |
|---|---|
| Windows x64 | `OpenMyTunnel-vX.X.X-win-x64.zip` |
| Linux x64 | `OpenMyTunnel-vX.X.X-linux-x64.tar.gz` |
| macOS Apple Silicon | `OpenMyTunnel-vX.X.X-osx-arm64.tar.gz` |

Intel Mac users: build from source (see below).

> **Windows note:** the zip contains `OpenMyTunnel.exe` and 3 native DLLs required by Avalonia's Skia renderer (`libSkiaSharp.dll`, `libHarfBuzzSharp.dll`, `av_libglesv2.dll`). Keep all files in the same folder.

## Build from source

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) plus a native C toolchain (MSVC on Windows, `clang` + `zlib1g-dev` on Linux, Xcode CLI tools on macOS).

```bash
git clone https://github.com/Esperadoce/OpenMyTunnel.git
cd OpenMyTunnel

# Development run
dotnet run --project src/OpenMyTunnel/OpenMyTunnel.csproj

# AOT publish - Windows
dotnet publish src/OpenMyTunnel/OpenMyTunnel.csproj -c Release -r win-x64

# AOT publish - Linux
sudo apt-get install -y clang zlib1g-dev
dotnet publish src/OpenMyTunnel/OpenMyTunnel.csproj -c Release -r linux-x64

# AOT publish - macOS Apple Silicon
dotnet publish src/OpenMyTunnel/OpenMyTunnel.csproj -c Release -r osx-arm64

# AOT publish - macOS Intel
dotnet publish src/OpenMyTunnel/OpenMyTunnel.csproj -c Release -r osx-x64
```

## Configuration

Non-secret settings are saved automatically:

| Platform | Path |
|---|---|
| Windows | `%LOCALAPPDATA%\OpenMyTunnel\config.json` |
| Linux | `~/.config/OpenMyTunnel/config.json` |
| macOS | `~/Library/Application Support/OpenMyTunnel/config.json` |

## Technology

Avalonia UI 11, SSH.NET, CommunityToolkit.Mvvm, .NET 10 AOT.

## About

Made by **Esperadoce** (Hicham Bouchikhi) - hicham@bouchikhi.net

## License

[MIT](LICENSE)
