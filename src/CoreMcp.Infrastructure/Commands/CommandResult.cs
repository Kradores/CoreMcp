namespace CoreMcp.Infrastructure.Commands;

public sealed record CommandResult(
    int ExitCode,
    string StandardOutput,
    string StandardError);
