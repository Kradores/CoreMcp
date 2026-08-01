namespace CoreMcp.Protocol.Tools;

public sealed record ToolsListResponse(
    IReadOnlyList<ToolDefinition> Tools) : IMcpResult;