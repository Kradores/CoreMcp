using CoreMcp.Protocol.Tools;
using System.Text.Json;

namespace CoreMcp.Framework.Tools;

public interface IToolAdapter
{
    ToolDefinition Definition { get; }

    Task<CallToolResponse> ExecuteAsync(
        JsonElement arguments,
        CancellationToken cancellationToken);
}
