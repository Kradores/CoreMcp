using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Tools;
using CoreMcp.Tools.FileSystem.Services;
using System.Text.Json;

namespace CoreMcp.Tools.FileSystem.FsList;

public sealed class FsListTool
    : IMcpTool<FsListArguments>
{
    private readonly FileListService _service;

    public FsListTool(FileListService service)
    {
        _service = service;
    }

    public ToolDescriptor Definition => new(
        Name: "fs_list",
        Description:
            """
            Lists the immediate contents of a directory without recursion.
            Returns the names of files and subdirectories contained directly within the specified folder.
            Use this tool when you already know the directory and want to inspect its contents without traversing the entire project.
            Prefer this tool over fs_tree when only a single directory needs to be examined.
            Hidden, ignored and non-relevant directories such as build artifacts and dependency folders may be omitted automatically.
            """);

    public async Task<CallToolResponse> ExecuteAsync(
        FsListArguments arguments,
        CancellationToken cancellationToken)
    {
        var result = await _service.ListAsync(
            arguments.Path,
            cancellationToken);

        return new CallToolResponse(
        [
            new TextContent(
                JsonSerializer.Serialize(
                    result,
                    JsonRpcSerializer.Options))
        ]);
    }
}
