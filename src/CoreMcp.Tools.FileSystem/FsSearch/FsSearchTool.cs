using System.Text.Json;
using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Tools;
using CoreMcp.Tools.FileSystem.Services;

namespace CoreMcp.Tools.FileSystem.FsSearch;

public sealed class FsSearchTool
    : IMcpTool<FsSearchArguments>
{
    private readonly FileSearchService _service;

    public FsSearchTool(
        FileSearchService service)
    {
        _service = service;
    }

    public ToolDescriptor Definition => new(
        Name: "fs_search",
        Description:
        """
        Searches recursively for files matching a filename or wildcard.

        Prefer this tool over fs_tree whenever the file name is known or partially known.

        Typical uses:
        - find Program.cs
        - find *.csproj
        - find Dockerfile
        - find appsettings*.json

        After locating files, use fs_read.
        """);

    public async Task<CallToolResponse> ExecuteAsync(
        FsSearchArguments arguments,
        CancellationToken cancellationToken)
    {
        var result = await _service.SearchAsync(
            arguments.Path,
            arguments.Pattern,
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