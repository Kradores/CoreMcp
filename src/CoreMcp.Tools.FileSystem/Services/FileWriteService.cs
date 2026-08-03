using System.Text;
using CoreMcp.Tools.FileSystem.FsWrite;

namespace CoreMcp.Tools.FileSystem.Services;

public sealed class FileWriteService
{
    public async Task<FsWriteResult> WriteAsync(
        string path,
        string content,
        bool overwrite,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);

        var fullPath = Path.GetFullPath(path);
        var parentPath = Path.GetDirectoryName(fullPath);

        if (string.IsNullOrEmpty(parentPath) || !Directory.Exists(parentPath))
        {
            throw new DirectoryNotFoundException(
                $"The parent directory for '{fullPath}' does not exist.");
        }

        if (Directory.Exists(fullPath))
        {
            throw new IOException(
                $"Cannot write to '{fullPath}' because it is a directory.");
        }

        var existed = File.Exists(fullPath);

        if (existed && !overwrite)
        {
            throw new IOException(
                $"The file '{fullPath}' already exists. Set overwrite to true to replace it.");
        }

        var mode = overwrite ? FileMode.Create : FileMode.CreateNew;

        await using var stream = new FileStream(
            fullPath,
            mode,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            useAsync: true);

        await using var writer = new StreamWriter(
            stream,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        await writer.WriteAsync(content.AsMemory(), cancellationToken);
        await writer.FlushAsync(cancellationToken);

        return new FsWriteResult(
            Path: fullPath,
            BytesWritten: Encoding.UTF8.GetByteCount(content),
            Created: !existed,
            Overwritten: existed);
    }
}
