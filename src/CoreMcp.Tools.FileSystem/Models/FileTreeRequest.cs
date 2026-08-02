using CoreMcp.Protocol;
using CoreMcp.Tools.FileSystem.Internal;

namespace CoreMcp.Tools.FileSystem.Models;

public sealed record FileTreeRequest(
    string Path,
    int MaxDepth = FileSystemLimits.MaxTreeDepth
) : IMcpRequest;
