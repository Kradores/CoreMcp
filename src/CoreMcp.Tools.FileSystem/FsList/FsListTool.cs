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
            Lists only the immediate contents of a directory.
            Prefer this tool over fs_tree when inspecting a single folder.
            Do not use this tool for recursive exploration.

            Examples:

            User:
            Find all Dockerfiles.

            User:
            Find Program.cs.

            User:
            Find every *.csproj.

            User:
            Find appsettings*.json.
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
