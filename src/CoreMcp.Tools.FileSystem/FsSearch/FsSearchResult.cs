namespace CoreMcp.Tools.FileSystem.FsSearch;

public sealed record FsSearchResult(
    IReadOnlyList<FsSearchItem> Files,
    bool Truncated);