namespace CoreMcp.Tools.FileSystem.Internal;

internal static class FileSystemIgnoreRules
{
    private static readonly HashSet<string> IgnoredDirectories =
    [
        ".git",
        ".vs",
        ".idea",
        ".vscode",

        "bin",
        "obj",

        "node_modules",

        "packages",

        "TestResults",

        ".next",
        ".nuxt",
        ".angular",

        "dist",
        "build",

        ".cache",

        ".turbo",

        ".parcel-cache",

        ".terraform",

        ".venv",
        "venv",

        "__pycache__"
    ];

    private static readonly HashSet<string> IgnoredFiles =
    [
        ".DS_Store",
        "Thumbs.db",

        ".env",
        ".env.local",
        ".env.development",
        ".env.production"
    ];

    private static readonly HashSet<string> IgnoredExtensions =
    [
        ".dll",
        ".exe",
        ".pdb",
        ".cache",
        ".class",
        ".pyc",
        ".o",
        ".so",
        ".dylib",
        ".zip",
        ".tar",
        ".gz",
        ".7z"
    ];

    public static bool IgnoreDirectory(string name)
        => IgnoredDirectories.Contains(name);

    public static bool IgnoreFile(string name)
        => IgnoredFiles.Contains(name);

    public static bool IgnoredExtension(string name)
        => IgnoredExtensions.Contains(name);

    public static bool ShouldSkip(FileSystemInfo info)
    {
        if (info.Attributes.HasFlag(FileAttributes.ReparsePoint))
            return true;

        if (info is DirectoryInfo)
            return IgnoredDirectories.Contains(info.Name);

        if (info is FileInfo file)
            return IgnoredExtensions.Contains(file.Extension);

        return IgnoredFiles.Contains(info.Name);
    }
}
