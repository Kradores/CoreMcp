namespace CoreMcp.Tools.FileSystem.FsDelete;

public sealed record FsDeleteResult(
    string Path,
    string Type,
    bool Recursive);
