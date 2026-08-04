namespace CoreMcp.Tools.FileSystem.FsCopy;

public sealed record FsCopyResult(
    string SourcePath,
    string DestinationPath,
    string Type,
    bool Overwritten);
