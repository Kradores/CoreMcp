using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreMcp.Protocol.Messages;

public sealed class JsonRpcResponse : JsonRpcMessage
{
    public JsonElement? Id { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Result { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonRpcError? Error { get; init; }

    public static JsonRpcResponse Success(
        JsonElement? id,
        object result)
    {
        return new JsonRpcResponse
        {
            JsonRpc = McpProtocol.JsonRpcVersion,
            Id = id,
            Result = result
        };
    }

    public static JsonRpcResponse Failure(
    JsonElement? id,
    JsonRpcError error)
    {
        return new JsonRpcResponse
        {
            JsonRpc = McpProtocol.JsonRpcVersion,
            Id = id,
            Error = error
        };
    }
}