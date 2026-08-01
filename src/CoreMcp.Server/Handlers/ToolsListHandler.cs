using CoreMcp.Framework.Handlers;
using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Tools;

namespace CoreMcp.Server.Handlers;

public sealed class ToolsListHandler
    : IMcpMethodHandler<ToolsListRequest, ToolsListResponse>
{
    private readonly ToolRegistry _toolRegistry;

    public ToolsListHandler(
        ToolRegistry toolRegistry)
    {
        _toolRegistry = toolRegistry;
    }

    public string Method => "tools/list";

    public Task<ToolsListResponse> HandleAsync(
        ToolsListRequest request,
        CancellationToken cancellationToken)
    {
        var response = new ToolsListResponse(
            _toolRegistry.Definitions.ToList());

        return Task.FromResult(response);
    }
}