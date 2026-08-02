using CoreMcp.Framework.DependencyInjection;
using CoreMcp.Framework.Dispatcher;
using CoreMcp.Framework.Tools;
using CoreMcp.Infrastructure.Commands;
using CoreMcp.Protocol.Transport;
using CoreMcp.Protocol.Transport.Framing;
using CoreMcp.Server;
using CoreMcp.Server.Handlers;
using CoreMcp.Tools.Echo;
using CoreMcp.Tools.FileSystem.Services;
using CoreMcp.Tools.FileSystem.FsTree;
using CoreMcp.Tools.System.Drives;
using Microsoft.Extensions.DependencyInjection;
using CoreMcp.Tools.FileSystem.FsRead;
using CoreMcp.Tools.FileSystem.FsList;
using CoreMcp.Tools.FileSystem.FsGrep;

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
    .AddMcpTool<SystemDrivesTool>()
    .AddMcpTool<FsTreeTool>()
    .AddMcpTool<FsReadTool>()
    .AddMcpTool<FsListTool>()
    .AddMcpTool<FsGrepTool>();

services.AddSingleton<FileTreeService>();
services.AddSingleton<FileReadService>();
services.AddSingleton<FileListService>();
services.AddSingleton<FileGrepService>();

using var provider = services.BuildServiceProvider();

var server = provider.GetRequiredService<McpServer>();

await server.RunAsync();