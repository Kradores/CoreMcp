namespace CoreMcp.Protocol.Exceptions;

public sealed class InvalidParamsException : McpException
{
    public InvalidParamsException(string message)
        : base(JsonRpcErrors.InvalidParams(message))
    {
    }
}
