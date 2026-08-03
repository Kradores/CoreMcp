namespace CoreMcp.Tools.FileSystem.FsMetadata;

public sealed record FsMetadataResult(
    string Path,
    string Type,
    long? Size,
    DateTime CreatedAtUtc,
    DateTime LastModifiedAtUtc,
    DateTime LastAccessedAtUtc,
    string Attributes);
