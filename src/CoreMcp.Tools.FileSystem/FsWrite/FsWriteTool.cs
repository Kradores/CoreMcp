using System.Text.Json;
using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Tools;
using CoreMcp.Tools.FileSystem.Services;

namespace CoreMcp.Tools.FileSystem.FsWrite;

public sealed class FsWriteTool : IMcpTool<FsWriteArguments>
{
    private readonly FileWriteService _service;

    public FsWriteTool(FileWriteService service)
    {
        _service = service;
    }

    public ToolDescriptor Definition => new(
        Name: "fs_write",
        Description:
        """
        Creates a UTF-8 text file or replaces the contents of an existing text file.
        Existing files are protected by default. Set overwrite to true only when replacing the current content is intended.
        Parent directories must already exist; use fs_mkdir before writing to a new directory.
        Returns the normalized file path, UTF-8 byte count, and whether the file was created or overwritten.
        """);

    public async Task<CallToolResponse> ExecuteAsync(
        FsWriteArguments arguments,
        CancellationToken cancellationToken)
    {
        var result = await _service.WriteAsync(
            arguments.Path,
            arguments.Content,
            arguments.Overwrite,
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
