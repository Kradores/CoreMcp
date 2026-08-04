using CoreMcp.Protocol.Exceptions;
using CoreMcp.Tools.FileSystem.Internal;
using Xunit;

namespace CoreMcp.Tools.FileSystem.Tests;

public sealed class FileSystemAccessPolicyTests : IDisposable
{
    private readonly string _testRoot = Path.Combine(
        Path.GetTempPath(),
        $"CoreMcp.FileSystemAccessPolicyTests.{Guid.NewGuid():N}");

    [Fact]
    public void Blocks_a_path_inside_a_restricted_directory()
    {
        var restricted = Path.Combine(_testRoot, "restricted");
        var policy = CreatePolicy(restricted);

        var exception = Assert.Throws<InvalidParamsException>(
            () => policy.EnsureMutationAllowed(Path.Combine(restricted, "settings.json")));

        Assert.Contains("restricted system directory", exception.Message);
    }

    [Fact]
    public void Blocks_a_parent_of_a_restricted_directory()
    {
        var restricted = Path.Combine(_testRoot, "restricted");
        var policy = CreatePolicy(restricted);

        Assert.Throws<InvalidParamsException>(
            () => policy.EnsureMutationAllowed(_testRoot));
    }

    [Fact]
    public void Blocks_traversal_that_resolves_into_a_restricted_directory()
    {
        var restricted = Path.Combine(_testRoot, "restricted");
        var policy = CreatePolicy(restricted);
        var traversalPath = Path.Combine(_testRoot, "safe", "..", "restricted", "file.txt");

        Assert.Throws<InvalidParamsException>(
            () => policy.EnsureMutationAllowed(traversalPath));
    }

    [Fact]
    public void Allows_an_unrelated_directory()
    {
        var restricted = Path.Combine(_testRoot, "restricted");
        var allowed = Path.Combine(_testRoot, "allowed", "file.txt");
        var policy = CreatePolicy(restricted);

        policy.EnsureMutationAllowed(allowed);
    }

    [Fact]
    public void Blocks_a_junction_that_targets_a_restricted_directory()
    {
        var restricted = Path.Combine(_testRoot, "restricted");
        var junction = Path.Combine(_testRoot, "junction");
        Directory.CreateDirectory(restricted);

        try
        {
            Directory.CreateSymbolicLink(junction, restricted);
        }
        catch (Exception exception) when (
            exception is UnauthorizedAccessException or IOException)
        {
            return;
        }

        var policy = CreatePolicy(restricted);

        Assert.Throws<InvalidParamsException>(
            () => policy.EnsureMutationAllowed(Path.Combine(junction, "file.txt")));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testRoot))
            Directory.Delete(_testRoot, recursive: true);
    }

    private FileSystemAccessPolicy CreatePolicy(string restricted)
    {
        Directory.CreateDirectory(_testRoot);
        return new FileSystemAccessPolicy([restricted]);
    }
}
