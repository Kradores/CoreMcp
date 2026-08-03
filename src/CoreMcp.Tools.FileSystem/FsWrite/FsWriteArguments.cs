namespace CoreMcp.Tools.FileSystem.FsWrite;

public sealed record FsWriteArguments(
    string Path,
    string Content,
    bool Overwrite = false);
