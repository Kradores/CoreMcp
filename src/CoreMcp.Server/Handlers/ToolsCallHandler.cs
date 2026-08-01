using CoreMcp.Framework.Handlers;
using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Tools;

namespace CoreMcp.Server.Handlers;

public sealed class ToolsCallHandler
    : IMcpMethodHandler<CallToolRequest, CallToolResponse>
{
    private readonly ToolRegistry _toolRegistry;

    public ToolsCallHandler(
        ToolRegistry toolRegistry)
    {
        _toolRegistry = toolRegistry;
    }

    public string Method => "tools/call";

    public async Task<CallToolResponse> HandleAsync(
        CallToolRequest request,
        CancellationToken cancellationToken)
    {
        var tool = _toolRegistry.GetRequiredTool(request.Name);

        return await tool.ExecuteAsync(
            request.Arguments,
            cancellationToken);
    }
}
