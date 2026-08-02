using CoreMcp.Tools.FileSystem.FsSearch;
using CoreMcp.Tools.FileSystem.Internal;

namespace CoreMcp.Tools.FileSystem.Services;

public sealed class FileSearchService
{
    public Task<FsSearchResult> SearchAsync(
        string rootPath,
        string pattern,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var root = new DirectoryInfo(rootPath);

        if (!root.Exists)
            throw new DirectoryNotFoundException(rootPath);

        var queue = new Queue<DirectoryInfo>();

        queue.Enqueue(root);

        var files = new List<FsSearchItem>();

        var truncated = false;

        while (queue.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var directory = queue.Dequeue();

            try
            {
                foreach (var child in directory
                    .EnumerateDirectories()
                    .Where(d => !FileSystemIgnoreRules.ShouldSkip(d))
                    .OrderBy(d => d.Name))
                {
                    queue.Enqueue(child);
                }

                foreach (var file in directory
                    .EnumerateFiles(pattern)
                    .Where(f => !FileSystemIgnoreRules.ShouldSkip(f))
                    .OrderBy(f => f.Name))
                {
                    if (FileSystemIgnoreRules.ShouldSkip(file))
                        continue;

                    var relativePath =
                        Path.GetRelativePath(
                            root.FullName,
                            file.FullName);

                    files.Add(
                        new FsSearchItem(
                            relativePath,
                            file.Length));

                    if (files.Count >= FileSystemLimits.MaxSearchResults)
                    {
                        truncated = true;
                        goto Done;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (IOException)
            {
            }
        }

    Done:

        files.Sort(
            (x, y) => StringComparer.OrdinalIgnoreCase.Compare(
                x.Path,
                y.Path));

        return Task.FromResult(
            new FsSearchResult(
                files,
                truncated));
    }
}