using System;
using System.Collections.Generic;
using System.Text;

namespace CoreMcp.Protocol.Initialize;

public sealed record InitializedNotification
    : IMcpRequest;
