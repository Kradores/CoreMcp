using CoreMcp.Protocol.Messages;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Transport;

namespace CoreMcp.Client;

public sealed class McpClient
{
    private readonly McpTransport _transport;

    public McpClient(McpTransport transport)
    {
        _transport = transport;
    }

    public async Task<JsonRpcResponse> SendAsync(
        JsonRpcRequest request,
        CancellationToken cancellationToken = default)
    {
        await _transport.WriteMessageAsync(
            JsonRpcSerializer.Serialize(request),
            cancellationToken);

        var payload = await _transport.ReadMessageAsync(cancellationToken);

        if (payload is null)
        {
            throw new IOException(
                "The MCP server closed the connection unexpectedly.");
        }

        return JsonRpcSerializer.Deserialize<JsonRpcResponse>(
            payload.Value);
    }
}
