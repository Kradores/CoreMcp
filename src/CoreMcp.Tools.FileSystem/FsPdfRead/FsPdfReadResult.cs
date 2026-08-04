namespace CoreMcp.Tools.FileSystem.FsPdfRead;

public sealed record FsPdfReadResult(
    string Path,
    long Size,
    int PageCount,
    int StartPage,
    int EndPage,
    bool Truncated,
    string Content);
