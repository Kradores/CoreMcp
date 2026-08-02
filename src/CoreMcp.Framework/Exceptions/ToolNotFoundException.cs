using CoreMcp.Protocol.Exceptions;
using CoreMcp.Protocol.Messages;

namespace CoreMcp.Framework.Exceptions;

public sealed class ToolNotFoundException
    : McpException
{
    public ToolNotFoundException(string tool)
        : base(JsonRpcErrors.InvalidParams($"Tool '{tool}' was not found."))
    {
    }
}
