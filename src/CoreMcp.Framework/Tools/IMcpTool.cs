using CoreMcp.Protocol.Tools;

namespace CoreMcp.Framework.Tools;

public interface IMcpTool<TArguments>
    where TArguments : class
{
    ToolDefinition Definition { get; }

    Task<CallToolResponse> ExecuteAsync(
        TArguments arguments,
        CancellationToken cancellationToken);
}