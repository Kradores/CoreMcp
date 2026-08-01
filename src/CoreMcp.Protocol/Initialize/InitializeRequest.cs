using CoreMcp.Protocol;
using CoreMcp.Protocol.Initialize;
using System.Text.Json;

public sealed record InitializeRequest(
    string ProtocolVersion,
    ClientCapabilities Capabilities,
    ClientInformation ClientInfo
) : IMcpRequest;