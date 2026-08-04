using System.Text.Json;
using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Tools;
using CoreMcp.Tools.FileSystem.Services;

namespace CoreMcp.Tools.FileSystem.FsPatch;

public sealed class FsPatchTool : IMcpTool<FsPatchArguments>
{
    private readonly FilePatchService _service;

    public FsPatchTool(FilePatchService service)
    {
        _service = service;
    }

    public ToolDescriptor Definition => new(
        Name: "fs_patch",
        Description:
        """
        Makes a precise text replacement in an existing UTF-8 text file.
        By default, oldText must occur exactly once. This prevents accidental edits when the same text appears in multiple places.
        Set replaceAll to true to replace every occurrence of oldText.
        Use fs_read first to obtain the exact text to replace. Binary files are not supported.
        """);

    public async Task<CallToolResponse> ExecuteAsync(
        FsPatchArguments arguments,
        CancellationToken cancellationToken)
    {
        var result = await _service.PatchAsync(
            arguments.Path,
            arguments.OldText,
            arguments.NewText,
            arguments.ReplaceAll,
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
