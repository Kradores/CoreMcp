using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Tools;

namespace CoreMcp.Tools.Echo;

public sealed class EchoTool
    : IMcpTool<EchoArguments>
{
    public ToolDescriptor Definition =>
        new(Name: "echo",
            Description: "Echoes the supplied message.");

    public Task<CallToolResponse> ExecuteAsync(
        EchoArguments arguments,
        CancellationToken cancellationToken)
    {
        Console.Error.WriteLine($"EchoTool: {arguments.Message}");

        return Task.FromResult(
            new CallToolResponse(
                Content:
                [
                    new TextContent(arguments.Message)
                ]));
    }
}