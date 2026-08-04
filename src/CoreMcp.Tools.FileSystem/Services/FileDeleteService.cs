using CoreMcp.Tools.FileSystem.FsDelete;
using CoreMcp.Tools.FileSystem.Internal;

namespace CoreMcp.Tools.FileSystem.Services;

public sealed class FileDeleteService
{
    private readonly FileSystemAccessPolicy _accessPolicy;

    public FileDeleteService(FileSystemAccessPolicy accessPolicy)
    {
        _accessPolicy = accessPolicy;
    }

    public Task<FsDeleteResult> DeleteAsync(
        string path,
        bool recursive,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        cancellationToken.ThrowIfCancellationRequested();

        var fullPath = _accessPolicy.Normalize(path);
        _accessPolicy.EnsureMutationAllowed(fullPath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);

            return Task.FromResult(
                new FsDeleteResult(
                    Path: fullPath,
                    Type: "file",
                    Recursive: false));
        }

        if (!Directory.Exists(fullPath))
        {
            throw new FileNotFoundException(
                "The specified file or directory does not exist.",
                fullPath);
        }

        if (!recursive && Directory.EnumerateFileSystemEntries(fullPath).Any())
        {
            throw new IOException(
                $"The directory '{fullPath}' is not empty. Set recursive to true to delete it and all of its contents.");
        }

        Directory.Delete(fullPath, recursive);

        return Task.FromResult(
            new FsDeleteResult(
                Path: fullPath,
                Type: "directory",
                Recursive: recursive));
    }
}
