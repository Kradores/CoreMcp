namespace CoreMcp.Infrastructure.Commands;

public static class CommandRunnerExtensions
{
    public static Task<CommandResult> ExecutePowerShellAsync(
        this ICommandRunner runner,
        string script,
        CancellationToken cancellationToken)
    {
        return runner.ExecuteAsync(
            "powershell.exe",
            $"-NoProfile -Command \"{script}\"",
            cancellationToken);
    }

    public static async Task<T> ExecutePowerShellJsonAsync<T>(
    this ICommandRunner runner,
    string script,
    CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
