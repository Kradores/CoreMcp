using System.Text.Json;
using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Tools;
using CoreMcp.Tools.FileSystem.Services;

namespace CoreMcp.Tools.FileSystem.FsMove;

public sealed class FsMoveTool : IMcpTool<FsMoveArguments>
{
    private readonly FileMoveService _service;

    public FsMoveTool(FileMoveService service)
    {
        _service = service;
    }

    public ToolDescriptor Definition => new(
        Name: "fs_move",
        Description:
        """
        Moves or renames one file or directory.
        The destination path is the exact new path, not a containing directory.
        Parent directories for the destination must already exist; use fs_mkdir first when needed.
        Existing destinations are protected by default. Set overwrite to true only when replacing them is intended.
        """);

    public async Task<CallToolResponse> ExecuteAsync(
        FsMoveArguments arguments,
        CancellationToken cancellationToken)
    {
        var result = await _service.MoveAsync(
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
