using CoreMcp.Protocol.Exceptions;

namespace CoreMcp.Tools.FileSystem.Internal;

/// <summary>
/// Protects host directories that must never be changed through filesystem tools.
/// </summary>
public sealed class FileSystemAccessPolicy
{
    public const string AdditionalRestrictedDirectoriesEnvironmentVariable =
        "CORE_MCP_RESTRICTED_DIRECTORIES";

    private readonly string[] _restrictedRoots;

    public FileSystemAccessPolicy(IEnumerable<string> restrictedDirectories)
    {
        ArgumentNullException.ThrowIfNull(restrictedDirectories);

        _restrictedRoots = restrictedDirectories
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(Path.GetFullPath)
            .Select(TrimEndingDirectorySeparator)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static FileSystemAccessPolicy CreateDefault()
    {
        var defaultDirectories = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            Environment.GetFolderPath(Environment.SpecialFolder.SystemX86),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
        };

        var configuredDirectories =
            (Environment.GetEnvironmentVariable(
                AdditionalRestrictedDirectoriesEnvironmentVariable) ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new FileSystemAccessPolicy(
            defaultDirectories.Concat(configuredDirectories));
    }

    public string Normalize(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Path.GetFullPath(path);
    }

    public void EnsureMutationAllowed(string path)
    {
        var normalizedPath = Normalize(path);
        var resolvedPath = ResolveReparsePoints(normalizedPath);

        foreach (var restrictedRoot in _restrictedRoots)
        {
            var resolvedRoot = ResolveReparsePoints(restrictedRoot);

            if (PathsOverlap(resolvedPath, resolvedRoot))
            {
                throw new InvalidParamsException(
                    $"Mutating '{normalizedPath}' is not allowed because it overlaps a restricted system directory.");
            }
        }
    }

    private static bool PathsOverlap(string firstPath, string secondPath)
        => IsSameOrDescendant(firstPath, secondPath) ||
           IsSameOrDescendant(secondPath, firstPath);

    private static bool IsSameOrDescendant(string candidatePath, string parentPath)
    {
        if (string.Equals(candidatePath, parentPath, StringComparison.OrdinalIgnoreCase))
            return true;

        var parentWithSeparator = Path.EndsInDirectorySeparator(parentPath)
            ? parentPath
            : parentPath + Path.DirectorySeparatorChar;

        return candidatePath.StartsWith(
            parentWithSeparator,
            StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveReparsePoints(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var root = Path.GetPathRoot(fullPath)!;
        var relativePath = fullPath[root.Length..];
        var segments = relativePath.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);

        var resolved = root;

        for (var index = 0; index < segments.Length; index++)
        {
            var candidate = Path.Combine(resolved, segments[index]);

            if (!File.Exists(candidate) && !Directory.Exists(candidate))
            {
                return Path.GetFullPath(
                    Path.Combine(resolved, Path.Combine(segments[index..])));
            }

            var item = Directory.Exists(candidate)
                ? (FileSystemInfo)new DirectoryInfo(candidate)
                : new FileInfo(candidate);

            if (item.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                var target = item.ResolveLinkTarget(returnFinalTarget: true);

                if (target is null)
                {
                    throw new InvalidParamsException(
                        $"Unable to resolve reparse point '{candidate}'.");
                }

                resolved = Path.GetFullPath(target.FullName);
            }
            else
            {
                resolved = candidate;
            }
        }

        return Path.GetFullPath(resolved);
    }

    private static string TrimEndingDirectorySeparator(string path)
    {
        var root = Path.GetPathRoot(path);

        return string.Equals(path, root, StringComparison.OrdinalIgnoreCase)
            ? path
            : Path.TrimEndingDirectorySeparator(path);
    }
}
