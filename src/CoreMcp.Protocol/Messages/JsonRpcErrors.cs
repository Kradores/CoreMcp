namespace CoreMcp.Protocol.Messages;

public static class JsonRpcErrors
{
    public static JsonRpcError ParseError(
        object? data = null) =>
        new(-32700, "Parse error", data);

    public static JsonRpcError InvalidRequest(
        object? data = null) =>
        new(-32600, "Invalid Request", data);

    public static JsonRpcError MethodNotFound(
        string method) =>
        new(-32601, $"Method '{method}' not found");

    public static JsonRpcError InvalidParams(
        object? data = null) =>
        new(-32602, "Invalid params", data);

    public static JsonRpcError InternalError(
        object? data = null) =>
        new(-32603, "Internal error", data);
}