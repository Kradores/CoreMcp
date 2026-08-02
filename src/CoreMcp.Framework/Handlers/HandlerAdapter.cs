using CoreMcp.Protocol;
using CoreMcp.Protocol.Messages;
using CoreMcp.Protocol.Serializer;
using System.Text.Json;

namespace CoreMcp.Framework.Handlers;

public sealed class HandlerAdapter<TRequest, TResult>
    : IHandlerAdapter
    where TRequest : IMcpRequest
    where TResult : IMcpResult
{
    private readonly IMcpMethodHandler<TRequest, TResult> _handler;

    public HandlerAdapter(
        IMcpMethodHandler<TRequest, TResult> handler)
    {
        _handler = handler;
    }

    public string Method => _handler.Method;

    public async Task<IMcpResult> HandleAsync(
        JsonRpcRequest request,
        CancellationToken cancellationToken)
    {
        Console.Error.WriteLine(request.ParametersOrEmpty.GetRawText());

        var typedRequest =
            JsonRpcSerializer.Deserialize<TRequest>(request.ParametersOrEmpty);

        Console.Error.WriteLine(
            JsonSerializer.Serialize(
                typedRequest,
                JsonRpcSerializer.Options));

        return await _handler.HandleAsync(
            typedRequest,
            cancellationToken);
    }
}
