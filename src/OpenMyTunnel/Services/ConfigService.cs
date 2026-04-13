using System.Text.Json;
using OpenMyTunnel.Models;

namespace OpenMyTunnel.Services;

public static class ConfigService
{
    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "OpenMyTunnel",
        "config.json");

    public static TunnelConfig Load()
    {
        try
        {
            if (!File.Exists(ConfigPath))
                return new TunnelConfig();

            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize(json, AppJsonContext.Default.TunnelConfig)
                   ?? new TunnelConfig();
        }
        catch
        {
            return new TunnelConfig();
        }
    }

    public static void Save(TunnelConfig config)
    {
        try
        {
            var dir = Path.GetDirectoryName(ConfigPath)!;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(config, AppJsonContext.Default.TunnelConfig);
            File.WriteAllText(ConfigPath, json);
        }
        catch
        {
            // Non-critical; swallow silently.
        }
    }
}
