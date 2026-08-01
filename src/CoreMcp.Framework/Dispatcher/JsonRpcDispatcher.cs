using CoreMcp.Framework.Dispatcher;
using CoreMcp.Framework.Handlers;
using CoreMcp.Protocol.Messages;

namespace CoreMcp.Framework.Dispatcher;

public sealed class JsonRpcDispatcher : IJsonRpcDispatcher
{
    private readonly Dictionary<string, IHandlerAdapter> _adapters;

    public JsonRpcDispatcher(
        IEnumerable<IHandlerAdapter> adapters)
    {
        _adapters =
            adapters.ToDictionary(
                x => x.Method,
                StringComparer.Ordinal);
    }

    public async Task<JsonRpcResponse?> DispatchAsync(
    JsonRpcRequest request,
    CancellationToken cancellationToken)
    {
        if (!_adapters.TryGetValue(request.Method, out var adapter))
        {
            return JsonRpcResponse.Failure(
                request.Id,
                JsonRpcErrors.MethodNotFound(request.Method));
        }

        var result = await adapter.HandleAsync(
            request,
            cancellationToken);

        if (request.IsNotification)
        {
            return null;
        }

        return JsonRpcResponse.Success(
            request.Id,
            result);
    }
}