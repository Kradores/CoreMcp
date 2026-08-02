using CoreMcp.Framework.Dispatcher;
using CoreMcp.Protocol.Messages;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Transport;
using CoreMcp.Server.ErrorHandling;
using System.Text;

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

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        Console.Error.WriteLine("Server started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            Console.Error.WriteLine("[SERVER] Waiting for message...");

            var message = await _transport.ReadMessageAsync(cancellationToken);

            Console.Error.WriteLine(
                $"[SERVER] Message received: {message?.Length ?? -1} bytes");

            if (message is null)
                break;

            Console.Error.WriteLine($"Received {message.Value.Length} bytes");
            Console.Error.WriteLine(Encoding.UTF8.GetString(message.Value.Span));

            JsonRpcRequest? request = null;

            try
            {
                request = JsonRpcSerializer
                    .Deserialize<JsonRpcRequest>(message.Value);

                Console.Error.WriteLine($"Received '{request.Method}'");

                var response = await _dispatcher.DispatchAsync(request, cancellationToken);

                if (response is null)
                    continue;

                var bytes = JsonRpcSerializer.Serialize(response);

                Console.Error.WriteLine(Encoding.UTF8.GetString(bytes.Span));

                await _transport.WriteMessageAsync(bytes);

                Console.Error.WriteLine("[SERVER] Response sent.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);

                if (request is { IsNotification: false })
                {
                    var response =
                        JsonRpcExceptionMapper.Map(
                            request,
                            ex);

                    await _transport.WriteMessageAsync(
                        JsonRpcSerializer.Serialize(response));
                }
            }
        }
    }
}