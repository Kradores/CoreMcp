using CoreMcp.Framework.Handlers;
using CoreMcp.Protocol;
using CoreMcp.Protocol.Initialize;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreMcp.Server.Handlers;

public sealed class InitializedNotificationHandler
    : IMcpMethodHandler<
        InitializedNotification,
        EmptyResult>
{
    public string Method =>
        "notifications/initialized";

    public Task<EmptyResult> HandleAsync(
        InitializedNotification request,
        CancellationToken cancellationToken)
    {
        Console.Error.WriteLine(
            "Client initialization completed.");

        return Task.FromResult(
            EmptyResult.Instance);
    }
}
