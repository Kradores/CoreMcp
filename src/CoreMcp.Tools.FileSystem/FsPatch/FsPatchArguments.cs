namespace CoreMcp.Tools.FileSystem.FsPatch;

public sealed record FsPatchArguments(
    string Path,
    string OldText,
    string NewText,
    bool ReplaceAll = false);
