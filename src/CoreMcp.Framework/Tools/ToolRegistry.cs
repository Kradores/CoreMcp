using CoreMcp.Framework.Tools;

public sealed class ToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, IToolAdapter> _tools;

    public ToolRegistry(IEnumerable<IToolAdapter> tools)
    {
        _tools = tools.ToDictionary(
            t => t.Definition.Name,
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<IToolAdapter> GetAll()
        => _tools.Values;

    public bool TryGetTool(
        string name,
        out IToolAdapter adapter)
        => _tools.TryGetValue(name, out adapter!);

    public IToolAdapter GetRequiredTool(string name)
    {
        if (TryGetTool(name, out var tool))
        {
            return tool;
        }

        throw new InvalidOperationException(
            $"Tool '{name}' was not found.");
    }
}