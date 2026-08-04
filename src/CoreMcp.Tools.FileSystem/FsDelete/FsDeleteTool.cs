using System.Text.Json;
using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Tools;
using CoreMcp.Tools.FileSystem.Services;

namespace CoreMcp.Tools.FileSystem.FsDelete;

public sealed class FsDeleteTool : IMcpTool<FsDeleteArguments>
{
    private readonly FileDeleteService _service;

    public FsDeleteTool(FileDeleteService service)
    {
        _service = service;
    }

    public ToolDescriptor Definition => new(
        Name: "fs_delete",
        Description:
        """
        Permanently deletes one file or directory from the local file system.
        Files and empty directories can be deleted with the default options.
        To delete a non-empty directory, set recursive to true. This permanently removes all files and subdirectories beneath it.
        Use fs_metadata or fs_list to verify the target before deleting it.
        """);

    public async Task<CallToolResponse> ExecuteAsync(
        FsDeleteArguments arguments,
        CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(
            arguments.Path,
            arguments.Recursive,
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
