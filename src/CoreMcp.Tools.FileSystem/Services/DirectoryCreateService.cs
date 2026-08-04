using CoreMcp.Tools.FileSystem.FsMkdir;

namespace CoreMcp.Tools.FileSystem.Services;

public sealed class DirectoryCreateService
{
    public Task<FsMkdirResult> CreateAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        cancellationToken.ThrowIfCancellationRequested();

        var fullPath = Path.GetFullPath(path);

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
