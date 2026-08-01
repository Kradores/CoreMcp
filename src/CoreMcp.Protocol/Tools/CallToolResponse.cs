namespace CoreMcp.Protocol.Tools;

public sealed record CallToolResponse(
    IReadOnlyList<ToolContent> Content,
    bool IsError = false) : IMcpResult;
