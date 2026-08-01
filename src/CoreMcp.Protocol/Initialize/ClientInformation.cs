using System;
using System.Collections.Generic;
using System.Text;

namespace CoreMcp.Protocol.Initialize
{
    public sealed record ClientInformation(
    string Name,
    string Version);
}
