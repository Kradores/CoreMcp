namespace CoreMcp.Tools.FileSystem.FsRead;

public sealed record FsReadArguments(
    string Path,
    int? MaxCharacters = null);