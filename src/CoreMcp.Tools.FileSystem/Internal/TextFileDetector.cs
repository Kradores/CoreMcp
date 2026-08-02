namespace CoreMcp.Tools.FileSystem.Internal;

public static class TextFileDetector
{
    private static readonly HashSet<string> BinaryExtensions =
    [
        ".dll",
        ".exe",
        ".png",
        ".jpg",
        ".jpeg",
        ".gif",
        ".bmp",
        ".ico",
        ".zip",
        ".7z",
        ".rar",
        ".pdf",
        ".mp3",
        ".mp4",
        ".avi",
        ".mov",
        ".woff",
        ".woff2",
        ".ttf",
        ".eot"
    ];

    public static bool IsTextFile(string path)
    {
        return !FileSystemTextReader.IsBinary(path);
    }

    public static bool IsTextFile(FileInfo file)
    {
        return !FileSystemTextReader.IsBinary(file.FullName);
    }
}
