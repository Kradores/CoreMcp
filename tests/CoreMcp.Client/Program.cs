using CoreMcp.Client;
using CoreMcp.Protocol;
using CoreMcp.Protocol.Initialize;
using CoreMcp.Protocol.Messages;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Transport;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

var solutionRoot = SolutionLocator.FindSolutionRoot().FullName;

var serverExe = Path.Combine(
    solutionRoot,
    "src",
    "CoreMcp.Server",
    "bin",
    "Debug",
    "net10.0",
    "CoreMcp.Server.exe");

var process = new Process();

process.StartInfo.FileName = serverExe;
process.StartInfo.Arguments = "";

process.StartInfo.RedirectStandardInput = true;
process.StartInfo.RedirectStandardOutput = true;
process.StartInfo.RedirectStandardError = true;

process.StartInfo.UseShellExecute = false;

process.Start();

_ = Task.Run(async () =>
{
    while (!process.StandardError.EndOfStream)
    {
        var line = await process.StandardError.ReadLineAsync();

        if (line is not null)
            Console.ForegroundColor = ConsoleColor.DarkYellow;

        Console.WriteLine($"[SERVER] {line}");

        Console.ResetColor();
    }
});

var transport = new McpTransport(
    process.StandardOutput.BaseStream,
    process.StandardInput.BaseStream);

var initialize = new InitializeRequest(
    McpProtocol.ProtocolVersion,
    new ClientCapabilities(),
    new ClientInformation(
        "CoreMcp.Client",
        "1.0.0"));

var request = new JsonRpcRequest
{
    JsonRpc = McpProtocol.JsonRpcVersion,
    Id = JsonSerializer.SerializeToElement(1),
    Method = "initialize",
    Parameters = JsonSerializer.SerializeToElement(initialize)
};

var client = new McpClient(transport);

var response = await client.SendAsync(request);

Console.WriteLine(JsonSerializer.Serialize(response));

var initialized = new JsonRpcRequest
{
    JsonRpc = McpProtocol.JsonRpcVersion,
    Method = "notifications/initialized",
    Parameters = JsonSerializer.SerializeToElement(
        new InitializedNotification())
};

Console.WriteLine("Sending notification...");

await transport.WriteMessageAsync(
    JsonRpcSerializer.Serialize(initialized));

Console.WriteLine("Notification sent.");
Console.WriteLine("Waiting 2 seconds...");

await Task.Delay(2000);

Console.WriteLine("No response received (expected).");
