namespace CoreMcp.Tools.FileSystem.FsPdfRead;

public sealed record FsPdfReadArguments(
    string Path,
    int StartPage = 1,
    int? EndPage = null,
    int? MaxCharacters = null);
