using CoreMcp.Tools.FileSystem.FsCopy;
using CoreMcp.Tools.FileSystem.Internal;

namespace CoreMcp.Tools.FileSystem.Services;

public sealed class FileCopyService
{
    private readonly FileSystemAccessPolicy _accessPolicy;

    public FileCopyService(FileSystemAccessPolicy accessPolicy)
    {
        _accessPolicy = accessPolicy;
    }

    public Task<FsCopyResult> CopyAsync(
        string sourcePath,
        string destinationPath,
        bool overwrite,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        cancellationToken.ThrowIfCancellationRequested();

        var sourceFullPath = _accessPolicy.Normalize(sourcePath);
        var destinationFullPath = _accessPolicy.Normalize(destinationPath);
        _accessPolicy.EnsureMutationAllowed(destinationFullPath);

        if (string.Equals(
                sourceFullPath,
                destinationFullPath,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException("The source and destination paths must be different.");
        }

        var destinationParent = Path.GetDirectoryName(destinationFullPath);

        if (string.IsNullOrEmpty(destinationParent) || !Directory.Exists(destinationParent))
        {
            throw new DirectoryNotFoundException(
                $"The destination parent directory for '{destinationFullPath}' does not exist.");
        }

        var sourceIsFile = File.Exists(sourceFullPath);
        var sourceIsDirectory = Directory.Exists(sourceFullPath);

        if (!sourceIsFile && !sourceIsDirectory)
        {
            throw new FileNotFoundException(
                "The source file or directory does not exist.",
                sourceFullPath);
        }

        if (sourceIsDirectory && IsDescendantPath(sourceFullPath, destinationFullPath))
        {
            throw new IOException(
                "A directory cannot be copied into one of its own descendants.");
        }

        var destinationExists = File.Exists(destinationFullPath) || Directory.Exists(destinationFullPath);

        if (destinationExists && !overwrite)
        {
            throw new IOException(
                $"The destination '{destinationFullPath}' already exists. Set overwrite to true to replace it.");
        }

        if (sourceIsFile)
        {
            File.Copy(sourceFullPath, destinationFullPath, overwrite);
        }
        else
        {
            if (destinationExists)
                DeleteDestination(destinationFullPath);

            CopyDirectory(sourceFullPath, destinationFullPath, cancellationToken);
        }

        return Task.FromResult(
            new FsCopyResult(
                SourcePath: sourceFullPath,
                DestinationPath: destinationFullPath,
                Type: sourceIsFile ? "file" : "directory",
                Overwritten: destinationExists));
    }

    private static void CopyDirectory(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(destinationPath);

        foreach (var file in Directory.EnumerateFiles(sourcePath))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var destinationFile = Path.Combine(
                destinationPath,
                Path.GetFileName(file));

            File.Copy(file, destinationFile);
        }

        foreach (var directory in Directory.EnumerateDirectories(sourcePath))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var destinationDirectory = Path.Combine(
                destinationPath,
                Path.GetFileName(directory));

            CopyDirectory(directory, destinationDirectory, cancellationToken);
        }
    }

    private static bool IsDescendantPath(string parentPath, string candidatePath)
    {
        var parentWithSeparator = Path.EndsInDirectorySeparator(parentPath)
            ? parentPath
            : parentPath + Path.DirectorySeparatorChar;

        return candidatePath.StartsWith(
            parentWithSeparator,
            StringComparison.OrdinalIgnoreCase);
    }

    private static void DeleteDestination(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
            return;
        }

        Directory.Delete(path, recursive: true);
    }
}
