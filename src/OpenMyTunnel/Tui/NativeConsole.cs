using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace OpenMyTunnel.Tui;

internal static partial class NativeConsole
{
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [SupportedOSPlatform("windows")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AllocConsole();

    // Opens a dedicated console window for TUI mode (WinExe has no console by default).
    [SupportedOSPlatform("windows")]
    public static void Alloc() => AllocConsole();
}
