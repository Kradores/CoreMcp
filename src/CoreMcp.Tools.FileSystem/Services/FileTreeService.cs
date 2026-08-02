using CoreMcp.Tools.FileSystem.Internal;
using CoreMcp.Tools.FileSystem.Models;

namespace CoreMcp.Tools.FileSystem.Services;

public sealed class FileTreeService
{
    public Task<FileTreeNode> BuildTreeAsync(
    string path,
    int maxDepth,
    CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var directory = new DirectoryInfo(path);

        if (!directory.Exists)
            throw new DirectoryNotFoundException(path);

        var root = BuildDirectory(
            directory,
            depth: 0,
            maxDepth,
            cancellationToken);

        return Task.FromResult(root);
    }

    private FileTreeNode BuildDirectory(
    DirectoryInfo directory,
    int depth,
    int maxDepth,
    CancellationToken cancellationToken)
    {
        try
        {
            if (depth >= maxDepth)
            {
                return EmptyDirectory(directory);
            }

            if (directory.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                return EmptyDirectory(directory);
            }

            var directories = directory
                .EnumerateDirectories()
                .Where(d => !FileSystemIgnoreRules.ShouldSkip(d))
                .OrderBy(d => d.Name);

            var files = directory
                .EnumerateFiles()
                .Where(f => !FileSystemIgnoreRules.ShouldSkip(f))
                .OrderBy(f => f.Name);

            var children = new List<FileTreeNode>();

            var truncated = false;
            var count = 0;

            foreach (var childDirectory in directories)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (++count > FileSystemLimits.MaxEntriesPerDirectory)
                {
                    truncated = true;
                    break;
                }

                children.Add(
                    BuildDirectory(
                        childDirectory,
                        depth + 1,
                        maxDepth,
                        cancellationToken));
            }

            if (!truncated)
            {
                foreach (var file in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (++count > FileSystemLimits.MaxEntriesPerDirectory)
                    {
                        truncated = true;
                        break;
                    }

                    children.Add(
                        new FileTreeNode(
                            file.Name,
                            FileNodeType.File,
                            Size: file.Length));
                }
            }

            return new FileTreeNode(
                directory.Name,
                FileNodeType.Directory,
                Children: children,
                Truncated: truncated);
        }
        catch (UnauthorizedAccessException)
        {
            return EmptyDirectory(directory);
        }
        catch (IOException)
        {
            return EmptyDirectory(directory);
        }
    }

    private static FileTreeNode EmptyDirectory(DirectoryInfo directory)
    {
        return new FileTreeNode(
            directory.Name,
            FileNodeType.Directory,
            Children: []);
    }
}
