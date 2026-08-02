using CoreMcp.Framework.Handlers;
using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Tools;

namespace CoreMcp.Server.Handlers;

public sealed class ToolsListHandler
    : IMcpMethodHandler<ToolsListRequest, ToolsListResponse>
{
    private readonly IToolRegistry _toolRegistry;

    public ToolsListHandler(
        IToolRegistry toolRegistry)
    {
        _toolRegistry = toolRegistry;
    }

    public string Method => "tools/list";

    public Task<ToolsListResponse> HandleAsync(
        ToolsListRequest request,
        CancellationToken cancellationToken)
    {
        var response = new ToolsListResponse(
            _toolRegistry.GetAll()
                .Select(tool =>
                    new ToolDefinition(
                        tool.Definition.Name,
                        tool.Definition.Description,
                        tool.InputSchema))
                .ToArray());

        return Task.FromResult(response);
    }
}