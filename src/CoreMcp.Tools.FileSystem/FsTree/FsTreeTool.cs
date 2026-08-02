using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Tools;
using CoreMcp.Tools.FileSystem.Services;
using System.Text.Json;

namespace CoreMcp.Tools.FileSystem.FsTree;

public sealed class FsTreeTool
    : IMcpTool<FsTreeArguments>
{
    private readonly FileTreeService _treeService;

    public FsTreeTool(
        FileTreeService treeService)
    {
        _treeService = treeService;
    }

    public ToolDescriptor Definition => new(
        Name: "fs_tree",
        Description: """
        Builds a recursive directory tree for a local folder or project.

        Use this tool FIRST when the user asks questions about:
        - a software project
        - source code
        - repository structure
        - folders or files
        - "read this project"
        - "analyze this repository"
        - "understand this codebase"

        The returned tree helps determine which files should be read next.
        """);

    public async Task<CallToolResponse> ExecuteAsync(
        FsTreeArguments arguments,
        CancellationToken cancellationToken)
    {
        var tree = await _treeService.BuildTreeAsync(
            arguments.Path,
            arguments.MaxDepth,
            cancellationToken);

        var json = JsonSerializer.Serialize(
            tree,
            JsonRpcSerializer.Options);

        return new CallToolResponse(
        [
            new TextContent(json)
        ]);
    }
}
