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
using CoreMcp.Tools.FileSystem.FsSearch;
using CoreMcp.Tools.FileSystem.FsMetadata;
using CoreMcp.Tools.FileSystem.FsWrite;
using CoreMcp.Tools.FileSystem.FsMkdir;
using CoreMcp.Tools.FileSystem.FsMove;
using CoreMcp.Tools.FileSystem.FsCopy;
using CoreMcp.Tools.FileSystem.FsDelete;
using CoreMcp.Tools.FileSystem.FsPatch;
using CoreMcp.Tools.FileSystem.Internal;

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
    .AddMcpTool<FsGrepTool>()
    .AddMcpTool<FsSearchTool>()
    .AddMcpTool<FsMetadataTool>()
    .AddMcpTool<FsWriteTool>()
    .AddMcpTool<FsMkdirTool>()
    .AddMcpTool<FsMoveTool>()
    .AddMcpTool<FsCopyTool>()
    .AddMcpTool<FsDeleteTool>()
    .AddMcpTool<FsPatchTool>();

services.AddSingleton<FileTreeService>();
services.AddSingleton(FileSystemAccessPolicy.CreateDefault());
services.AddSingleton<FileReadService>();
services.AddSingleton<FileListService>();
services.AddSingleton<FileGrepService>();
services.AddSingleton<FileSearchService>();
services.AddSingleton<FileMetadataService>();
services.AddSingleton<FileWriteService>();
services.AddSingleton<DirectoryCreateService>();
services.AddSingleton<FileMoveService>();
services.AddSingleton<FileCopyService>();
services.AddSingleton<FileDeleteService>();
services.AddSingleton<FilePatchService>();

using var provider = services.BuildServiceProvider();

var server = provider.GetRequiredService<McpServer>();

await server.RunAsync();
