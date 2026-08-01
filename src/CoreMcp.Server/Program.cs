using CoreMcp.Protocol;
using CoreMcp.Protocol.Initialize;
using CoreMcp.Protocol.Transport;
using CoreMcp.Server;
using CoreMcp.Server.Handlers;
using CoreMcp.Server.JsonRpc;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton(new McpTransport(
    Console.OpenStandardInput(),
    Console.OpenStandardOutput()));

services.AddSingleton<McpServer>();

services.AddSingleton<JsonRpcDispatcher>();

services.AddSingleton<
    IMcpMethodHandler<InitializeRequest, InitializeResponse>,
    InitializeHandler>();

services.AddSingleton<IHandlerAdapter>(
    sp => new HandlerAdapter<
        InitializeRequest,
        InitializeResponse>(
        sp.GetRequiredService<
            IMcpMethodHandler<InitializeRequest, InitializeResponse>>()));

services.AddSingleton<
    IMcpMethodHandler<InitializedNotification, EmptyResult>,
    InitializedNotificationHandler>();

services.AddSingleton<IHandlerAdapter>(sp =>
    new HandlerAdapter<
        InitializedNotification,
        EmptyResult>(
        sp.GetRequiredService<
            IMcpMethodHandler<
                InitializedNotification,
                EmptyResult>>()));

using var provider = services.BuildServiceProvider();

var server = provider.GetRequiredService<McpServer>();

await server.RunAsync();