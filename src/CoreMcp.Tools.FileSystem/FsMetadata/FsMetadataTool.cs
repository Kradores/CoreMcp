using System.Text.Json;
using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Tools;
using CoreMcp.Tools.FileSystem.Services;

namespace CoreMcp.Tools.FileSystem.FsMetadata;

public sealed class FsMetadataTool : IMcpTool<FsMetadataArguments>
{
    private readonly FileMetadataService _service;

    public FsMetadataTool(FileMetadataService service)
    {
        _service = service;
    }

    public ToolDescriptor Definition => new(
        Name: "fs_metadata",
        Description:
        """
        Returns metadata for one file or directory without reading its contents.
        Use this tool to determine an item's normalized path, type, size, timestamps and filesystem attributes.
        Directory sizes are not calculated and are returned as null.
        Use fs_read to retrieve the contents of a text file, or fs_list to inspect a directory's immediate contents.
        """);

    public async Task<CallToolResponse> ExecuteAsync(
        FsMetadataArguments arguments,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetAsync(
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
