using CoreMcp.Framework.Reflection;
using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Tools;
using System.Text.Json;

namespace CoreMcp.Framework.Tools;

public sealed class ToolAdapter<TArguments>
    : IToolAdapter
    where TArguments : class
{
    private readonly IMcpTool<TArguments> _tool;

    public ToolAdapter(IMcpTool<TArguments> tool)
    {
        _tool = tool;
    }

    public ToolDescriptor Definition => _tool.Definition;

    public JsonElement InputSchema => JsonSchemaGenerator.Generate(typeof(TArguments));

    public async Task<CallToolResponse> ExecuteAsync(
        JsonElement arguments,
        CancellationToken cancellationToken)
    {
        var typed = JsonRpcSerializer.Deserialize<TArguments>(arguments);

        return await _tool.ExecuteAsync(
            typed,
            cancellationToken);
    }
}
