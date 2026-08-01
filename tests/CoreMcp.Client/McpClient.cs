using CoreMcp.Protocol.Messages;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Transport;
using System;
using System.Collections.Generic;
using System.Text;

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

        var response =
            await _transport.ReadMessageAsync(cancellationToken);

        return JsonRpcSerializer.Deserialize<JsonRpcResponse>(
            response!.Value);
    }
}
