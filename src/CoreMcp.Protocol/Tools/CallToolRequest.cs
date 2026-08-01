using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CoreMcp.Protocol.Tools;

public sealed record CallToolRequest(
    string Name,
    JsonElement Arguments) : IMcpRequest;
