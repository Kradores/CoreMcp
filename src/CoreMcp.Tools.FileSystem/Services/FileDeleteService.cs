using CoreMcp.Tools.FileSystem.FsDelete;

namespace CoreMcp.Tools.FileSystem.Services;

public sealed class FileDeleteService
{
    public Task<FsDeleteResult> DeleteAsync(
        string path,
        bool recursive,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        cancellationToken.ThrowIfCancellationRequested();

        var fullPath = Path.GetFullPath(path);

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
