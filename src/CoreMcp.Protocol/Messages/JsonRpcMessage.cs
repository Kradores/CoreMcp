using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CoreMcp.Protocol.Messages;

public abstract class JsonRpcMessage
{
    [JsonPropertyName("jsonrpc")]
    public required string JsonRpc { get; init; }
}
