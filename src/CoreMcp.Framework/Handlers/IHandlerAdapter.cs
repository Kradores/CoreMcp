using CoreMcp.Protocol;
using CoreMcp.Protocol.Messages;

namespace CoreMcp.Framework.Handlers;

public interface IHandlerAdapter
{
    string Method { get; }

    Task<IMcpResult> HandleAsync(
        JsonRpcRequest request,
        CancellationToken cancellationToken);
}
