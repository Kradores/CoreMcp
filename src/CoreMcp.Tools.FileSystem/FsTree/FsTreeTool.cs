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
        Recursively explores the directory structure of a project.

        Use this tool ONLY when you need an overview of an unfamiliar project.

        Do NOT use this tool to locate a known file or inspect a single directory.

        Instead:
        - use fs_search to find files by name or wildcard,
        - use fs_list to inspect one directory,
        - use fs_read to read file contents.
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
