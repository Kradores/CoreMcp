namespace CoreMcp.Tools.FileSystem.Models;

public sealed record FileReadResult(
    string Path,
    long Size,
    bool Truncated,
    string Content);