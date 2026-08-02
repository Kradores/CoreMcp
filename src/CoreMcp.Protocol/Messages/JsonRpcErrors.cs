using CoreMcp.Protocol.Messages;

public static class JsonRpcErrors
{
    public const int ParseErrorCode = -32700;
    public const int InvalidRequestCode = -32600;
    public const int MethodNotFoundCode = -32601;
    public const int InvalidParamsCode = -32602;
    public const int InternalErrorCode = -32603;

    public static JsonRpcError ParseError(string message) =>
        new(ParseErrorCode, message);

    public static JsonRpcError InvalidRequest(string message) =>
        new(InvalidRequestCode, message);

    public static JsonRpcError MethodNotFound(string method) =>
        new(MethodNotFoundCode, $"Method '{method}' was not found.");

    public static JsonRpcError InvalidParams(string message) =>
        new(InvalidParamsCode, message);

    public static JsonRpcError Internal(string message) =>
        new(InternalErrorCode, message);

    public static JsonRpcError Internal(Exception exception) =>
        new(InternalErrorCode, exception.Message);
}