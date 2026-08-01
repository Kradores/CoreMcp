using CoreMcp.Framework.Handlers;
using CoreMcp.Protocol;
using CoreMcp.Protocol.Initialize;

namespace CoreMcp.Server.Handlers;

public sealed class InitializeHandler
    : IMcpMethodHandler<InitializeRequest, InitializeResponse>
{
    public string Method => "initialize";

    public Task<InitializeResponse> HandleAsync(
        InitializeRequest request,
        CancellationToken cancellationToken)
    {
        Console.Error.WriteLine(
            $"Client: {request.ClientInfo.Name}");

        return Task.FromResult(
            new InitializeResponse(
                McpProtocol.ProtocolVersion,
                new ServerCapabilities(
                    new ToolCapabilities()),
                new ServerInformation(
                    "CoreMcp.Server",
                    "1.0.0")));
    }
}