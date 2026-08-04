namespace CoreMcp.Tools.FileSystem.FsMove;

public sealed record FsMoveArguments(
    string SourcePath,
    string DestinationPath,
    bool Overwrite = false);
