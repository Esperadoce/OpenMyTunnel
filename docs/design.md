# OpenMyTunnel - Design Document

## 1. Overview

**OpenMyTunnel** is a lightweight cross-platform desktop utility that lets a user establish a
persistent SSH dynamic-port-forwarding tunnel (SOCKS5 proxy) with a single click.

Typical use-case:

```
ssh -D 1080 -N esperadoce@172.160.244.150
```

The user fills in Host, SSH Port, Username, and local SOCKS port once. The application handles
the rest, lives in the system tray, and lets the user toggle the tunnel on/off at any time.

---

## 2. Technology Stack

| Concern | Choice | Rationale |
|---|---|---|
| Runtime | **.NET 10** | Current LTS release |
| UI Framework | **Avalonia UI 11** | Cross-platform, modern XAML UI, explicit AOT support, fully custom styling |
| SSH Library | **SSH.NET 2024.x** | Pure .NET SOCKS5 dynamic forwarding via `ForwardedPortDynamic` |
| Serialisation | **System.Text.Json** with source generators | AOT-safe, zero reflection at runtime |
| AOT | `<PublishAot>true</PublishAot>` | Single self-contained native binary per platform |
| Targets | `win-x64`, `linux-x64`, `osx-x64` | Windows, Linux, macOS |

### Why Avalonia and not MAUI?

MAUI on Windows delegates rendering to WinUI 3 / Windows App SDK. AOT support there is still
experimental and requires redistributing the Windows App SDK runtime separately. Avalonia renders
via its own Skia-based pipeline, produces a single self-contained binary with no external runtime
dependencies, and has explicit `PublishAot=true` support starting with .NET 8.

---

## 3. Architecture

```
TrayApplicationContext (or platform tray helper)
  |
  +-- owns --> MainWindow          (Avalonia Window)
  |              |
  |              +-- binds to --> MainViewModel    (MVVM)
  |                                  |
  |                                  +-- calls --> SshTunnelService
  |
  +-- owns --> SshTunnelService    (pure service, no UI)
                  SshClient + ForwardedPortDynamic
                  Status: Disconnected / Connecting / Connected / Error
```

The application follows a lightweight MVVM pattern using Avalonia's built-in binding system.
No third-party MVVM framework is required for a project of this scope.

---

## 4. Component Details

### 4.1 Tray Integration

- **Windows**: `NotifyIcon` via the `Avalonia.Controls.ApplicationLifetime` + the
  `Avalonia.Tray` community package (or a slim P/Invoke wrapper).
- **Linux**: `AppIndicator` or `XEmbedTrayIcon` via the same Avalonia tray package.
- **macOS**: `NSStatusItem` via Avalonia tray support.

Tray icon states:

| State | Icon colour |
|---|---|
| Disconnected | Grey |
| Connecting | Yellow |
| Connected | Green |
| Error | Red |

Context menu items:

- Open
- Connect (disabled when connected)
- Disconnect (disabled when disconnected)
- [separator]
- Exit

### 4.2 MainWindow

The window is fixed-size (non-resizable), borderless-style with a custom title bar to match a
modern compact utility look.

| Control | Purpose |
|---|---|
| TextBox Host | IP or hostname of the SSH server |
| NumericUpDown SSH Port | Remote SSH port (default 22) |
| TextBox Username | SSH username |
| RadioButton Password / SSH Key | Authentication mode selector |
| TextBox Password | Masked input, password mode only |
| ToggleButton Show/Hide | Reveals password in clear text |
| TextBox Key File | Path to private key file, key mode only |
| Button Browse | Opens file picker for key file |
| TextBox Passphrase | Optional passphrase for key file |
| NumericUpDown SOCKS Port | Local SOCKS5 listening port (default 1080) |
| Label Command preview | Read-only, shows equivalent `ssh -D ...` command |
| Status indicator | Coloured dot + text label for current state |
| Button Connect / Disconnect | Toggles the tunnel |
| Button Minimize to Tray | Hides the window |
| CheckBox Start minimised | On next launch, skip straight to tray |

### 4.3 MainViewModel

Properties (all `INotifyPropertyChanged`):

```
Host            : string
SshPort         : int       = 22
Username        : string
AuthMode        : AuthMode  = Password
Password        : string    (not persisted)
KeyFilePath     : string
Passphrase      : string    (not persisted)
LocalSocksPort  : int       = 1080
CommandPreview  : string    (computed, read-only)
TunnelStatus    : TunnelStatus
IsConnected     : bool      (computed)
IsConnecting    : bool      (computed)
```

Commands:

```
ConnectCommand
DisconnectCommand
BrowseKeyFileCommand
MinimizeToTrayCommand
```

### 4.4 SshTunnelService

```
ConnectAsync(config, credential)
  -> builds AuthenticationMethod (password or private key)
  -> SshClient.Connect()
  -> ForwardedPortDynamic("127.0.0.1", socksPort).Start()
  -> fires StatusChanged(Connected)

Disconnect()
  -> port.Stop()
  -> client.Disconnect()
  -> fires StatusChanged(Disconnected)
```

`ForwardedPortDynamic` is the SSH.NET equivalent of `ssh -D <port> -N`.

### 4.5 TunnelConfig (persisted model)

```csharp
public sealed class TunnelConfig
{
    public string   Host           { get; set; } = "";
    public int      SshPort        { get; set; } = 22;
    public string   Username       { get; set; } = "";
    public int      LocalSocksPort { get; set; } = 1080;
    public AuthMode AuthMode       { get; set; } = AuthMode.Password;
    public string   KeyFilePath    { get; set; } = "";
    public bool     StartMinimised { get; set; } = false;
}

public enum AuthMode { Password, PrivateKey }
```

Passwords and passphrases are never written to disk.

Config is saved to:

- Windows: `%LOCALAPPDATA%\OpenMyTunnel\config.json`
- Linux: `~/.config/OpenMyTunnel/config.json`
- macOS: `~/Library/Application Support/OpenMyTunnel/config.json`

Serialisation uses `System.Text.Json` with a source-generated `JsonSerializerContext` for
full AOT compatibility.

---

## 5. UI Design

### 5.1 Visual Style

- Background: dark (`#1E1E2E`) with a subtle card surface (`#2A2A3E`) for each section.
- Accent colour: `#7C6AF7` (soft indigo/violet).
- Typography: system default sans-serif, 13 px body, 11 px labels.
- Corner radius: 6 px on inputs, 8 px on section cards.
- The window has no OS chrome; a custom title bar row contains the app name and the three window
  control buttons (minimise, close).

### 5.2 Wireframe

```
+------------------------------------------------+
| OpenMyTunnel                            _ [X]  |
+------------------------------------------------+
|  SERVER                                        |
|  +------------------------------------------+ |
|  | Host  [172.160.244.150     ]  Port [ 22 ] | |
|  | User  [esperadoce          ]              | |
|  +------------------------------------------+ |
|                                                |
|  AUTHENTICATION                                |
|  +------------------------------------------+ |
|  | (o) Password   ( ) SSH Key               | |
|  | Password  [**************] [show]         | |
|  +------------------------------------------+ |
|                                                |
|  TUNNEL                                        |
|  +------------------------------------------+ |
|  | SOCKS Port  [1080]                        | |
|  | > ssh -D 1080 -N esperadoce@172.160.244.1 | |
|  +------------------------------------------+ |
|                                                |
|  [o] Disconnected                              |
|                                                |
|  [    Connect    ]       [Minimize to Tray]   |
+------------------------------------------------+
```

SSH Key mode (auth section swaps):

```
|  | ( ) Password   (o) SSH Key               | |
|  | Key File  [/home/user/.ssh/id_rsa] [...]  | |
|  | Passphrase[            ] (optional)       | |
```

### 5.3 Status Indicator States

| State | Dot colour | Label text |
|---|---|---|
| Disconnected | `#6B7280` grey | Disconnected |
| Connecting | `#F59E0B` amber | Connecting... |
| Connected | `#10B981` green | Connected |
| Error | `#EF4444` red | Error: {message} |

---

## 6. Data Flow - Connect

```
User clicks [Connect]
  |
  +- ViewModel reads all fields
  +- SaveConfig() writes non-secret fields to config.json
  |
  v
SshTunnelService.ConnectAsync(config, credential)
  |
  +- Status -> Connecting  (UI: button disabled, dot amber)
  |
  +- Task.Run { SshClient.Connect() }   <- thread-pool, non-blocking UI
  |
  +- ForwardedPortDynamic.Start()
  |
  +- Status -> Connected
  |     UI: button label -> "Disconnect", dot green
  |     Tray icon -> green
  |
  +- On any exception -> Status -> Error
        UI: red dot, error message shown
        auto-transition to Disconnected after 3 s
```

---

## 7. AOT Considerations

| Area | Approach |
|---|---|
| JSON serialisation | `JsonSerializerContext` source generator - zero reflection |
| SSH.NET | Library ships trim-safe annotations; `<SuppressTrimAnalysisWarnings>true` guards residual warnings |
| Avalonia | AOT-compatible since 11.1; XAML compiled to C# via `XamlX` at build time |
| MVVM bindings | Source-generated via `[ObservableProperty]` (CommunityToolkit.Mvvm) or manual INPC |
| Publish command | `dotnet publish -c Release -r win-x64` (repeat per target RID) |

Expected output sizes:

| Platform | Binary size |
|---|---|
| win-x64 | ~20-25 MB |
| linux-x64 | ~22-27 MB |
| osx-x64 | ~22-27 MB |

---

## 8. Security Considerations

- Passwords and passphrases live only in memory as `string`. `SecureString` is deprecated in
  .NET 5+ and offers negligible benefit on modern runtimes.
- The config file contains no secrets.
- The SOCKS proxy binds to `127.0.0.1` only and is not reachable from the network.
- SSH host key verification is delegated to SSH.NET. On first connection to an unknown host the
  app prompts the user to confirm the fingerprint (trust-on-first-use, same as OpenSSH).

---

## 9. Project File Structure

```
OpenMyTunnel/
+-- OpenMyTunnel.csproj
+-- Program.cs
+-- App.axaml
+-- App.axaml.cs
+-- Assets/
|   +-- icon_grey.ico
|   +-- icon_green.ico
|   +-- icon_amber.ico
|   +-- icon_red.ico
+-- Models/
|   +-- TunnelConfig.cs
|   +-- AppJsonContext.cs
+-- Services/
|   +-- SshTunnelService.cs
|   +-- ConfigService.cs
+-- ViewModels/
|   +-- MainViewModel.cs
+-- Views/
|   +-- MainWindow.axaml
|   +-- MainWindow.axaml.cs
+-- Tray/
|   +-- TrayManager.cs
+-- docs/
    +-- design.md
```

---

## 10. Future Enhancements

- Windows Credential Manager integration for passwordless auto-connect.
- Multiple tunnel profiles with a list/tab view.
- Run-at-startup option via platform-specific mechanisms.
- Auto-reconnect on network change detection.
- System proxy auto-configuration on connect/disconnect.
