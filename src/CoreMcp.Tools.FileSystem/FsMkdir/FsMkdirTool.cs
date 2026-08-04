using System.Text.Json;
using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Tools;
using CoreMcp.Tools.FileSystem.Services;

namespace CoreMcp.Tools.FileSystem.FsMkdir;

public sealed class FsMkdirTool : IMcpTool<FsMkdirArguments>
{
    private readonly DirectoryCreateService _service;

    public FsMkdirTool(DirectoryCreateService service)
    {
        _service = service;
    }

    public ToolDescriptor Definition => new(
        Name: "fs_mkdir",
        Description:
        """
        Creates a directory and any missing parent directories.
        This operation is idempotent: if the directory already exists, it succeeds and reports created as false.
        Use fs_write after this tool when creating a file in a new directory.
        """);

    public async Task<CallToolResponse> ExecuteAsync(
        FsMkdirArguments arguments,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(
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
