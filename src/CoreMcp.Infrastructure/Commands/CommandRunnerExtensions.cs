namespace CoreMcp.Infrastructure.Commands;

public static class CommandRunnerExtensions
{
    public static Task<CommandResult> ExecutePowerShellAsync(
        this ICommandRunner runner,
        string script,
        CancellationToken cancellationToken = default)
    {
        return runner.ExecuteAsync(
            "powershell.exe",
            $"-NoProfile -Command \"{script}\"",
            cancellationToken);
    }
}
