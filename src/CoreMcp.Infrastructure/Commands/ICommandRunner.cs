namespace CoreMcp.Infrastructure.Commands;

public interface ICommandRunner
{
    Task<CommandResult> ExecuteAsync(
        string fileName,
        string arguments,
        CancellationToken cancellationToken);
}
