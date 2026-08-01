using System.Text.Json;

namespace CoreMcp.Protocol.Tools;

public sealed record ToolDefinition(
    string Name,
    string Description,
    JsonElement InputSchema);
