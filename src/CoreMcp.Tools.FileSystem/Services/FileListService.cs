using CoreMcp.Tools.FileSystem.FsList;
using CoreMcp.Tools.FileSystem.Internal;

namespace CoreMcp.Tools.FileSystem.Services;

public sealed class FileListService
{
    public async Task<FsListResult> ListAsync(
    string path,
    CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var directory = new DirectoryInfo(path);

        if (!directory.Exists)
            throw new DirectoryNotFoundException(path);

        var directories = new List<string>();
        var files = new List<string>();

        var truncated = false;
        var count = 0;

        foreach (var child in directory.EnumerateDirectories())
        {
            if (FileSystemIgnoreRules.ShouldSkip(child))
                continue;

            directories.Add(child.Name);

            count++;

            if (count >= FileSystemLimits.MaxEntriesPerDirectory)
            {
                truncated = true;
                break;
            }
        }

        if (!truncated)
        {
            foreach (var file in directory.EnumerateFiles())
            {
                if (FileSystemIgnoreRules.ShouldSkip(file))
                    continue;

                files.Add(file.Name);

                count++;

                if (count >= FileSystemLimits.MaxEntriesPerDirectory)
                {
                    truncated = true;
                    break;
                }
            }
        }

        directories.Sort(StringComparer.OrdinalIgnoreCase);
        files.Sort(StringComparer.OrdinalIgnoreCase);

        return new FsListResult(
            directory.FullName,
            directories,
            files,
            truncated);
    }
}
