namespace CoreMcp.Tools.FileSystem.FsCopy;

public sealed record FsCopyArguments(
    string SourcePath,
    string DestinationPath,
    bool Overwrite = false);
