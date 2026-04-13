using System.Text.Json.Serialization;
using OpenMyTunnel.Models;

namespace OpenMyTunnel.Models;

/// <summary>
/// Source-generated JSON context for AOT-safe serialisation.
/// Avoids all reflection at runtime.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(TunnelConfig))]
internal partial class AppJsonContext : JsonSerializerContext { }
