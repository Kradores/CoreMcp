namespace CoreMcp.Tools.FileSystem.FsMove;

public sealed record FsMoveResult(
    string SourcePath,
    string DestinationPath,
    string Type,
    bool Overwritten);
