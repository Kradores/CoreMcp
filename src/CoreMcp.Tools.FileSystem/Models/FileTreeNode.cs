namespace CoreMcp.Tools.FileSystem.Models;

public sealed record FileTreeNode(
    string Name,
    FileNodeType Type,
    long? Size = null,
    IReadOnlyList<FileTreeNode>? Children = null,
    bool Truncated = false);
