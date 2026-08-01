using CoreMcp.Protocol;
using CoreMcp.Protocol.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreMcp.Server.Handlers;

public interface IHandlerAdapter
{
    string Method { get; }

    Task<IMcpResult> HandleAsync(
        JsonRpcRequest request,
        CancellationToken cancellationToken);
}
