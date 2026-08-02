using CoreMcp.Tools.FileSystem.Internal;

namespace CoreMcp.Tools.FileSystem.FsTree;

public sealed record FsTreeArguments(
    string Path,
    int MaxDepth = FileSystemLimits.MaxTreeDepth);
