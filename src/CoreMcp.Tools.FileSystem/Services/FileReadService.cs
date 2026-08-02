using System.Text;
using CoreMcp.Tools.FileSystem.Internal;
using CoreMcp.Tools.FileSystem.Models;

namespace CoreMcp.Tools.FileSystem.Services;

public sealed class FileReadService
{
    public async Task<FileReadResult> ReadAsync(
        string path,
        int? maxCharacters,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException(path);

        if (!TextFileDetector.IsTextFile(path))
            throw new InvalidOperationException(
                "Binary files cannot be read.");

        var file = new FileInfo(path);

        var text = await File.ReadAllTextAsync(
            path,
            Encoding.UTF8,
            cancellationToken);

        var limit = maxCharacters ?? FileSystemLimits.MaxCharacters;

        var truncated = text.Length > limit;

        if (truncated)
            text = text[..limit];

        return new FileReadResult(
            Path: path,
            Size: file.Length,
            Truncated: truncated,
            Content: text);
    }
}