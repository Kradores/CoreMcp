namespace CoreMcp.Tools.FileSystem.FsList;

public sealed record FsListResult(
    string Path,
    IReadOnlyList<string> Directories,
    IReadOnlyList<string> Files,
    bool Truncated);