using System.Text.Json.Serialization;

namespace CoreMcp.Protocol.Initialize;

public sealed record ToolCapabilities(
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    bool? ListChanged = null);
