using System;
using System.Collections.Generic;
using System.Text;

namespace CoreMcp.Protocol.Messages;

public abstract class JsonRpcMessage
{
    public required string JsonRpc { get; init; }
}
