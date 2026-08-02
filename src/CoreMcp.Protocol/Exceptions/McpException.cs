using CoreMcp.Protocol.Messages;

namespace CoreMcp.Protocol.Exceptions;

public abstract class McpException : Exception
{
    protected McpException(JsonRpcError error)
        : base(error.Message)
    {
        Error = error;
    }

    public JsonRpcError Error { get; }
}
