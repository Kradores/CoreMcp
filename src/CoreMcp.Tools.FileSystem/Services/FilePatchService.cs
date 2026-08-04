using System.Text;
using CoreMcp.Tools.FileSystem.FsPatch;
using CoreMcp.Tools.FileSystem.Internal;

namespace CoreMcp.Tools.FileSystem.Services;

public sealed class FilePatchService
{
    public async Task<FsPatchResult> PatchAsync(
        string path,
        string oldText,
        string newText,
        bool replaceAll,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrEmpty(oldText);
        ArgumentNullException.ThrowIfNull(newText);

        var fullPath = Path.GetFullPath(path);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("The specified file does not exist.", fullPath);

        if (!TextFileDetector.IsTextFile(fullPath))
            throw new InvalidOperationException("Binary files cannot be patched.");

        var content = await File.ReadAllTextAsync(
            fullPath,
            Encoding.UTF8,
            cancellationToken);

        var occurrences = CountOccurrences(content, oldText);

        if (occurrences == 0)
        {
            throw new InvalidOperationException(
                "The specified oldText was not found in the file.");
        }

        if (!replaceAll && occurrences != 1)
        {
            throw new InvalidOperationException(
                $"The specified oldText occurs {occurrences} times. Set replaceAll to true to replace every occurrence.");
        }

        var updatedContent = content.Replace(
            oldText,
            newText,
            StringComparison.Ordinal);

        var temporaryPath = Path.Combine(
            Path.GetDirectoryName(fullPath)!,
            $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            await File.WriteAllTextAsync(
                temporaryPath,
                updatedContent,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                cancellationToken);

            File.Move(temporaryPath, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }

        return new FsPatchResult(
            Path: fullPath,
            Replacements: replaceAll ? occurrences : 1,
            BytesWritten: Encoding.UTF8.GetByteCount(updatedContent));
    }

    private static int CountOccurrences(string content, string value)
    {
        var count = 0;
        var startIndex = 0;

        while (true)
        {
            var index = content.IndexOf(
                value,
                startIndex,
                StringComparison.Ordinal);

            if (index < 0)
                return count;

            count++;
            startIndex = index + value.Length;
        }
    }
}
