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

    public static Task<string> ReadAllTextAsync()
    {
        throw new NotImplementedException();
    }
}
