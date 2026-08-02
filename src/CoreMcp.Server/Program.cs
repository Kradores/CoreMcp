using CoreMcp.Framework.DependencyInjection;
using CoreMcp.Framework.Dispatcher;
using CoreMcp.Framework.Tools;
using CoreMcp.Infrastructure.Commands;
using CoreMcp.Protocol;
using CoreMcp.Protocol.Transport;
using CoreMcp.Protocol.Transport.Framing;
using CoreMcp.Server;
using CoreMcp.Server.Handlers;
using CoreMcp.Tools.Echo;
using CoreMcp.Tools.System.Drives;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<ContentLengthFramer>();
services.AddSingleton<NdjsonFramer>();

services.AddSingleton(sp =>
    new BufferedBinaryReader(
        Console.OpenStandardInput()));
services.AddSingleton<TransportFramerFactory>();

services.AddSingleton(sp =>
    new McpTransport(
        Console.OpenStandardOutput(),
        sp.GetRequiredService<TransportFramerFactory>(),
        sp.GetRequiredService<BufferedBinaryReader>()));

services.AddSingleton<McpServer>();

services
    .AddSingleton<JsonRpcDispatcher>()
    .AddSingleton<IToolRegistry, ToolRegistry>()
    .AddSingleton<ICommandRunner, ProcessCommandRunner>()

    .AddMcpHandler<InitializeHandler>()
    .AddMcpHandler<InitializedNotificationHandler>()
    .AddMcpHandler<ToolsListHandler>()
    .AddMcpHandler<ToolsCallHandler>()
    .AddMcpTool<EchoTool>()
    .AddMcpTool<SystemDrivesTool>();

using var provider = services.BuildServiceProvider();

var server = provider.GetRequiredService<McpServer>();

await server.RunAsync();