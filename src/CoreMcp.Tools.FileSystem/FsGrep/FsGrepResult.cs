namespace CoreMcp.Tools.FileSystem.FsGrep;

public sealed record FsGrepResult(
    IReadOnlyList<FsGrepMatch> Matches,
    bool Truncated);
