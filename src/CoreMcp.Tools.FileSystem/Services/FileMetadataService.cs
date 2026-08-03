using CoreMcp.Tools.FileSystem.FsMetadata;

namespace CoreMcp.Tools.FileSystem.Services;

public sealed class FileMetadataService
{
    public Task<FsMetadataResult> GetAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fullPath = Path.GetFullPath(path);

        if (File.Exists(fullPath))
        {
            var file = new FileInfo(fullPath);
            return Task.FromResult(CreateResult(file, file.Length));
        }

        if (Directory.Exists(fullPath))
        {
            var directory = new DirectoryInfo(fullPath);
            return Task.FromResult(CreateResult(directory, null));
        }

        throw new FileNotFoundException(
            "The specified file or directory does not exist.",
            fullPath);
    }

    private static FsMetadataResult CreateResult(
        FileSystemInfo item,
        long? size)
        => new(
            Path: item.FullName,
            Type: item is DirectoryInfo ? "directory" : "file",
            Size: size,
            CreatedAtUtc: item.CreationTimeUtc,
            LastModifiedAtUtc: item.LastWriteTimeUtc,
            LastAccessedAtUtc: item.LastAccessTimeUtc,
            Attributes: item.Attributes.ToString());
}
