using System.Text.Json;
using CoreMcp.Framework.Tools;
using CoreMcp.Infrastructure.Commands;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Tools;

namespace CoreMcp.Tools.System.Drives;

public sealed class SystemDrivesTool
    : IMcpTool<SystemDrivesArguments>
{
    private readonly ICommandRunner _commandRunner;

    private const string Script = """
    @(
        Get-PSDrive -PSProvider FileSystem |
        Select-Object Name, Used, Free
    ) | ConvertTo-Json -Compress
    """;

    public SystemDrivesTool(
        ICommandRunner commandRunner)
    {
        _commandRunner = commandRunner;
    }

    public ToolDescriptor Definition => new(
        Name: "system_drives",
        Description: "Gets information about local file system drives.");

    public async Task<CallToolResponse> ExecuteAsync(
        SystemDrivesArguments arguments,
        CancellationToken cancellationToken)
    {
        var result = await _commandRunner.ExecutePowerShellAsync(
            Script,
            cancellationToken);

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Failed to get system drives. Exit code: {result.ExitCode}. Error: {result.StandardError}");
        }

        var drives = JsonSerializer.Deserialize<List<DriveInformation>>(
            result.StandardOutput,
            JsonRpcSerializer.Options);

        return new CallToolResponse(
        [
            new TextContent(
                JsonSerializer.Serialize(
                    drives,
                    JsonRpcSerializer.Options))
        ]);
    }
}