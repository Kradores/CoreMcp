using System.Diagnostics;

namespace CoreMcp.Infrastructure.Commands;

public sealed class ProcessCommandRunner : ICommandRunner
{
    public async Task<CommandResult> ExecuteAsync(
        string fileName,
        string arguments,
        CancellationToken cancellationToken = default)
    {
        using var process = new Process();

        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,

            RedirectStandardOutput = true,
            RedirectStandardError = true,

            UseShellExecute = false,
            CreateNoWindow = true
        };

        process.Start();

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        return new CommandResult(
            process.ExitCode,
            await stdoutTask,
            await stderrTask);
    }
}