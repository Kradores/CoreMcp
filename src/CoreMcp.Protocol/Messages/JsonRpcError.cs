namespace CoreMcp.Protocol.Messages;

public sealed record JsonRpcError(
    int Code,
    string Message,
    object? Data = null);