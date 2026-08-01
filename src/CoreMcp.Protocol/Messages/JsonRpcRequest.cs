using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreMcp.Protocol.Messages;

public sealed class JsonRpcRequest : JsonRpcMessage
{
    public JsonElement? Id { get; init; }
    public required string Method { get; init; }

    [JsonPropertyName("params")]
    public JsonElement Parameters { get; init; }
    public bool IsNotification => Id is null;
}