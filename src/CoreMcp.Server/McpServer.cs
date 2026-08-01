using CoreMcp.Framework.Dispatcher;
using CoreMcp.Protocol.Messages;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Transport;

namespace CoreMcp.Server;

public sealed class McpServer
{
    private readonly McpTransport _transport;
    private readonly JsonRpcDispatcher _dispatcher;

    public McpServer(McpTransport transport, JsonRpcDispatcher dispatcher)
    {
        _transport = transport;
        _dispatcher = dispatcher;
    }

    public async Task RunAsync(
        CancellationToken cancellationToken = default)
    {
        Console.Error.WriteLine("Server started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            var bytes = await _transport.ReadMessageAsync();

            if (bytes is null)
            {
                Console.Error.WriteLine("Client disconnected.");
                return;
            }

            var request = JsonRpcSerializer.Deserialize<JsonRpcRequest>(bytes.Value);

            Console.Error.WriteLine(
                $"Received '{request.Method}' " +
                $"({(request.IsNotification ? "notification" : "request")})");

            var response = await _dispatcher.DispatchAsync(request, cancellationToken);

            if (response is not null)
            {
                await _transport.WriteMessageAsync(
                    JsonRpcSerializer.Serialize(response),
                    cancellationToken);
            }
        }
    }
}