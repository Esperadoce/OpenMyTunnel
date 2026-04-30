using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace OpenMyTunnel.Tui;

internal static partial class NativeConsole
{
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [SupportedOSPlatform("windows")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AllocConsole();

    // Opens a dedicated console window for TUI mode (WinExe has no console by default)
    // and re-opens the standard streams so Console.In/Out/Error route to the new window.
    [SupportedOSPlatform("windows")]
    public static void Alloc()
    {
        AllocConsole();
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        Console.SetIn(new StreamReader(Console.OpenStandardInput()));
        Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
    }
}
