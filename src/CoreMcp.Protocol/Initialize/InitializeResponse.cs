using CoreMcp.Protocol;
using CoreMcp.Protocol.Initialize;

public sealed record InitializeResponse(
    string ProtocolVersion,
    ServerCapabilities Capabilities,
    ServerInformation ServerInfo) : IMcpResult;