using CoreMcp.Protocol.Messages;

namespace CoreMcp.Protocol.Exceptions;

public sealed class MethodNotFoundException
    : McpException
{
    public MethodNotFoundException(string method)
        : base(JsonRpcErrors.MethodNotFound(method))
    {
    }
}
