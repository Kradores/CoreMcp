using System.Text.Json;
using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Tools;
using CoreMcp.Tools.FileSystem.Services;

namespace CoreMcp.Tools.FileSystem.FsCopy;

public sealed class FsCopyTool : IMcpTool<FsCopyArguments>
{
    private readonly FileCopyService _service;

    public FsCopyTool(FileCopyService service)
    {
        _service = service;
    }

    public ToolDescriptor Definition => new(
        Name: "fs_copy",
        Description:
        """
        Copies one file or directory, including all contents of a directory.
        The destination path is the exact copy path, not a containing directory.
        Parent directories for the destination must already exist; use fs_mkdir first when needed.
        Existing destinations are protected by default. Set overwrite to true only when replacing them is intended.
        """);

    public async Task<CallToolResponse> ExecuteAsync(
        FsCopyArguments arguments,
        CancellationToken cancellationToken)
    {
        var result = await _service.CopyAsync(
            arguments.SourcePath,
            arguments.DestinationPath,
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
