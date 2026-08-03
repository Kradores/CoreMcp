namespace CoreMcp.Tools.FileSystem.FsWrite;

public sealed record FsWriteResult(
    string Path,
    long BytesWritten,
    bool Created,
    bool Overwritten);
