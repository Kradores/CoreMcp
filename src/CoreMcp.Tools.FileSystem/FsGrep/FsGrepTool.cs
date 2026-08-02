using System.Text.Json;
using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Tools;
using CoreMcp.Tools.FileSystem.Services;

namespace CoreMcp.Tools.FileSystem.FsGrep;

public sealed class FsGrepTool
    : IMcpTool<FsGrepArguments>
{
    private readonly FileGrepService _service;

    public FsGrepTool(
        FileGrepService service)
    {
        _service = service;
    }

    public ToolDescriptor Definition => new(
        Name: "fs_grep",
        Description:
        """
        Searches recursively for text within files.
        Use this tool to locate implementations, method calls, symbols, configuration values, TODO comments or any other text inside a project.
        Unlike fs_search, which searches filenames, fs_grep searches the contents of text files.
        Returns the matching file, line number and matching line.
        Ignored directories, hidden files, binary files and very large files are skipped automatically.
        After locating relevant files, use fs_read to inspect the complete file if additional context is needed.
        """);

    public async Task<CallToolResponse> ExecuteAsync(
        FsGrepArguments arguments,
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