using CoreMcp.Protocol;

namespace CoreMcp.Server.Handlers;

public interface IMcpMethodHandler<TRequest, TResult>
    where TRequest : IMcpRequest
    where TResult : IMcpResult
{
    string Method { get; }

    Task<TResult> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken);
}