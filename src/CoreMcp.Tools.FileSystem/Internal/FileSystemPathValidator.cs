namespace CoreMcp.Tools.FileSystem.Internal;

internal sealed class FileSystemPathValidator
{
    public string Normalize(string path)
    {
        return Path.GetFullPath(path);
    }

    public void EnsureExists(string path)
    {
        if (!File.Exists(path) &&
            !Directory.Exists(path))
        {
            throw new FileNotFoundException(path);
        }
    }
}
