using CoreMcp.Protocol.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreMcp.Framework.Dispatcher;

public interface IJsonRpcDispatcher
{
    Task<JsonRpcResponse?> DispatchAsync(
        JsonRpcRequest request,
        CancellationToken cancellationToken);
}
