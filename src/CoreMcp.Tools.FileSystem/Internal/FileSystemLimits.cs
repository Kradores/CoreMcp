namespace CoreMcp.Tools.FileSystem.Internal;

internal static class FileSystemLimits
{
    public const long MaxReadFileSize = 512 * 1024;
    public const int MaxTreeDepth = 10;
    public const int MaxEntriesPerDirectory = 1000;
    public const int MaxSearchResults = 200;
    public const int MaxGrepResults = 200;
    public const int MaxCharacters = 50_000;
}
