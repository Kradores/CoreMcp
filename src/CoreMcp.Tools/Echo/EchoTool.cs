using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Tools;
using CoreMcp.Tools.Echo;
using System.Text.Json;

namespace CoreMcp.Tools.Echo;

public sealed class EchoTool
    : IMcpTool<EchoArguments>
{
    public ToolDefinition Definition =>
        new(
            Name: "echo",
            Description: "Echoes the supplied message.",
            InputSchema: JsonSerializer.SerializeToElement(new
            {
                type = "object",
                properties = new
                {
                    message = new
                    {
                        type = "string"
                    }
                },
                required = new[] { "message" }
            }));

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