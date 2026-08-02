using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Tools;
using CoreMcp.Tools.FileSystem.Services;
using System.Text.Json;

namespace CoreMcp.Tools.FileSystem.FsRead;

public sealed class FsReadTool
    : IMcpTool<FsReadArguments>
{
    private readonly FileReadService _service;

    public FsReadTool(FileReadService service)
    {
        _service = service;
    }

    public ToolDescriptor Definition => new(
        Name: "fs_read",
        Description:
            """
            Reads the contents of a text file from the local file system.
            Use this tool after locating a file with fs_tree, fs_list or fs_search.
            Suitable for reading source code, configuration files, documentation, JSON, XML, YAML, Markdown and other text-based files.
            Returns the file content together with metadata indicating whether the content was truncated due to size limits.
            Do not use this tool to discover files or folders. Use fs_tree for recursive project exploration or fs_list for listing the contents of a single directory.
            Binary files such as images, executables, archives and media files are not supported.
            """);

    public async Task<CallToolResponse> ExecuteAsync(
        FsReadArguments arguments,
        CancellationToken cancellationToken)
    {
        var result = await _service.ReadAsync(
            arguments.Path,
            arguments.MaxCharacters,
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
