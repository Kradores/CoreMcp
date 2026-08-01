using CoreMcp.Protocol;
using CoreMcp.Protocol.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CoreMcp.Server.Handlers;

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
        var typedRequest =
            JsonSerializer.Deserialize<TRequest>(
                request.Parameters)!;

        return await _handler.HandleAsync(
            typedRequest,
            cancellationToken);
    }
}
