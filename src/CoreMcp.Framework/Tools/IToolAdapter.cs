using CoreMcp.Protocol.Tools;
using System.Text.Json;

namespace CoreMcp.Framework.Tools;

public interface IToolAdapter
{
    ToolDescriptor Definition { get; }
    JsonElement InputSchema { get; }

    Task<CallToolResponse> ExecuteAsync(
        JsonElement arguments,
        CancellationToken cancellationToken);
}
