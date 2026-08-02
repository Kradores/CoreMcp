namespace CoreMcp.Tools.FileSystem.FsGrep;

public sealed record FsGrepMatch(
    string Path,
    int Line,
    string Text);
