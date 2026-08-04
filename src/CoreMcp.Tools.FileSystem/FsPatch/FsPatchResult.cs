namespace CoreMcp.Tools.FileSystem.FsPatch;

public sealed record FsPatchResult(
    string Path,
    int Replacements,
    long BytesWritten);
