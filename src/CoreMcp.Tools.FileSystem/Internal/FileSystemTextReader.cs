using System.Text;

namespace CoreMcp.Tools.FileSystem.Internal;

internal static class FileSystemTextReader
{
    public static bool IsBinary(string path)
    {
        using var stream = File.OpenRead(path);

        Span<byte> buffer = stackalloc byte[1024];

        var read = stream.Read(buffer);

        for (var i = 0; i < read; i++)
        {
            if (buffer[i] == 0)
                return true;
        }

        return false;
    }

    public static async Task<string> ReadAllTextAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        return await File.ReadAllTextAsync(
            path,
            Encoding.UTF8,
            cancellationToken);
    }
}