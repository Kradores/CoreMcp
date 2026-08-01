using CoreMcp.Framework.DependencyInjection;
using CoreMcp.Framework.Dispatcher;
using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Transport;
using CoreMcp.Server;
using CoreMcp.Server.Handlers;
using CoreMcp.Tools.Echo;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton(new McpTransport(
    Console.OpenStandardInput(),
    Console.OpenStandardOutput()));

services.AddSingleton<McpServer>();

services
    .AddSingleton<JsonRpcDispatcher>()
    .AddSingleton<ToolRegistry>()

    .AddMcpHandler<InitializeHandler>()
    .AddMcpHandler<InitializedNotificationHandler>()
    .AddMcpHandler<ToolsListHandler>()
    .AddMcpHandler<ToolsCallHandler>()
    .AddMcpTool<EchoTool>();

using var provider = services.BuildServiceProvider();

var server = provider.GetRequiredService<McpServer>();

await server.RunAsync();