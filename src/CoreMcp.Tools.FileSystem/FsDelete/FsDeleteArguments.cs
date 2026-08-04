namespace CoreMcp.Tools.FileSystem.FsDelete;

public sealed record FsDeleteArguments(
    string Path,
    bool Recursive = false);
