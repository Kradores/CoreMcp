using CoreMcp.Tools.FileSystem.FsMkdir;
using CoreMcp.Tools.FileSystem.Internal;

namespace CoreMcp.Tools.FileSystem.Services;

public sealed class DirectoryCreateService
{
    private readonly FileSystemAccessPolicy _accessPolicy;

    public DirectoryCreateService(FileSystemAccessPolicy accessPolicy)
    {
        _accessPolicy = accessPolicy;
    }

    public Task<FsMkdirResult> CreateAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        cancellationToken.ThrowIfCancellationRequested();

        var fullPath = _accessPolicy.Normalize(path);
        _accessPolicy.EnsureMutationAllowed(fullPath);

        if (File.Exists(fullPath))
        {
            throw new IOException(
                $"Cannot create directory '{fullPath}' because a file already exists at that path.");
        }

        var existed = Directory.Exists(fullPath);

        Directory.CreateDirectory(fullPath);

        return Task.FromResult(
            new FsMkdirResult(
                Path: fullPath,
                Created: !existed));
    }
}
