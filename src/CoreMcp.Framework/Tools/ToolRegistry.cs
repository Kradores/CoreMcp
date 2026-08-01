using CoreMcp.Protocol.Tools;

namespace CoreMcp.Framework.Tools;

public sealed class ToolRegistry
{
    private readonly Dictionary<string, IToolAdapter> _tools;
    private readonly IReadOnlyList<ToolDefinition> _definitions;

    public ToolRegistry(IEnumerable<IToolAdapter> tools)
    {
        _tools = tools.ToDictionary(
            t => t.Definition.Name,
            StringComparer.OrdinalIgnoreCase);

        _definitions = _tools.Values
            .Select(t => t.Definition)
            .ToArray();
    }

    public IReadOnlyList<ToolDefinition> Definitions => _definitions;

    public IToolAdapter GetRequiredTool(string name)
    {
        if (!_tools.TryGetValue(name, out var tool))
        {
            throw new InvalidOperationException(
                $"Tool '{name}' was not found.");
        }

        return tool;
    }
}
