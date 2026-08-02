using CoreMcp.Tools.FileSystem.FsGrep;
using CoreMcp.Tools.FileSystem.Internal;

namespace CoreMcp.Tools.FileSystem.Services;

public sealed class FileGrepService
{
    public async Task<FsGrepResult> SearchAsync(
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

        var matches = new List<FsGrepMatch>();

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
                    .EnumerateFiles()
                    .Where(f => !FileSystemIgnoreRules.ShouldSkip(f))
                    .OrderBy(f => f.Name))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!TextFileDetector.IsTextFile(file))
                        continue;

                    if (file.Length > FileSystemLimits.MaxReadFileSize)
                        continue;

                    var text = await FileSystemTextReader.ReadAllTextAsync(
                        file.FullName,
                        cancellationToken);

                    var relativePath =
                        Path.GetRelativePath(
                            root.FullName,
                            file.FullName);

                    var lines = text.Replace("\r\n", "\n").Split('\n');

                    for (var i = 0; i < lines.Length; i++)
                    {
                        if (!lines[i].Contains(
                            pattern,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        matches.Add(
                            new FsGrepMatch(
                                relativePath,
                                i + 1,
                                lines[i].Trim()));

                        if (matches.Count >= FileSystemLimits.MaxGrepResults)
                        {
                            truncated = true;
                            goto Done;
                        }
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

        return new FsGrepResult(
            matches,
            truncated);
    }
}