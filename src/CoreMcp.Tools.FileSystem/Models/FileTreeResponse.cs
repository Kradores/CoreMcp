using CoreMcp.Protocol;

namespace CoreMcp.Tools.FileSystem.Models;

public sealed record FileTreeResponse(
    FileTreeNode Root
) : IMcpResult;
