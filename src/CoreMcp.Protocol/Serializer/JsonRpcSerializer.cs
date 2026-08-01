using System.Text.Json;

namespace CoreMcp.Protocol.Serializer;

public static class JsonRpcSerializer
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public static T Deserialize<T>(ReadOnlyMemory<byte> payload)
    {
        return JsonSerializer.Deserialize<T>(
            payload.Span,
            Options)!;
    }

    public static T Deserialize<T>(JsonElement element)
    {
        return element.Deserialize<T>(Options)!;
    }

    public static ReadOnlyMemory<byte> Serialize<T>(T value)
    {
        return JsonSerializer.SerializeToUtf8Bytes(
            value,
            Options);
    }
}