using CoreMcp.Protocol.Exceptions;
using CoreMcp.Protocol.Messages;

namespace CoreMcp.Server.ErrorHandling;

public static class JsonRpcExceptionMapper
{
    public static JsonRpcResponse Map(
        JsonRpcRequest? request,
        Exception exception)
    {
        if (exception is McpException mcp)
        {
            return JsonRpcResponse.Failure(
                request?.Id,
                mcp.Error);
        }

        return JsonRpcResponse.Failure(
            request?.Id,
            JsonRpcErrors.Internal(exception));
    }
}
