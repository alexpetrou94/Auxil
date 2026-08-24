using JetBrains.Annotations;
using Auxil.Utilities;

namespace Auxil.Tests.Utilities;

[TestSubject(typeof(FileUtility))]
public class FileUtilityTest : IDisposable
{
    private readonly string _sourceDirectory = Path.Combine(Path.GetTempPath(), $"uphoric-test-src-{Guid.NewGuid():N}");
    private readonly string _destinationDirectory = Path.Combine(Path.GetTempPath(), $"uphoric-test-dst-{Guid.NewGuid():N}");

    public FileUtilityTest()
    {
        Directory.CreateDirectory(_sourceDirectory);
        File.WriteAllText(Path.Combine(_sourceDirectory, "root.txt"), "root");
        Directory.CreateDirectory(Path.Combine(_sourceDirectory, "sub"));
        File.WriteAllText(Path.Combine(_sourceDirectory, "sub", "nested.txt"), "nested");
    }

    public void Dispose()
    {
        if (Directory.Exists(_sourceDirectory))
        {
            Directory.Delete(_sourceDirectory, true);
        }

        if (Directory.Exists(_destinationDirectory))
        {
            Directory.Delete(_destinationDirectory, true);
        }
    }

    [Fact]
    public void CopyDirectory_CopiesFullTree()
    {
        FileUtility.CopyDirectory(_sourceDirectory, _destinationDirectory).ShouldBeTrue();

        File.ReadAllText(Path.Combine(_destinationDirectory, "root.txt")).ShouldBe("root");
        File.ReadAllText(Path.Combine(_destinationDirectory, "sub", "nested.txt")).ShouldBe("nested");
    }

    [Fact]
    public void CopyDirectory_MissingSourceReturnsFalse()
    {
        FileUtility.CopyDirectory(Path.Combine(_sourceDirectory, "missing"), _destinationDirectory).ShouldBeFalse();
    }

    [Fact]
    public void CopyDirectory_ExistingDestinationWithoutOverwriteReturnsFalse()
    {
        Directory.CreateDirectory(_destinationDirectory);

        FileUtility.CopyDirectory(_sourceDirectory, _destinationDirectory).ShouldBeFalse();
    }

    [Fact]
    public void CopyDirectory_OverwriteCopiesOverExistingFiles()
    {
        Directory.CreateDirectory(_destinationDirectory);
        File.WriteAllText(Path.Combine(_destinationDirectory, "root.txt"), "stale");

        FileUtility.CopyDirectory(_sourceDirectory, _destinationDirectory, overwrite: true).ShouldBeTrue();

        File.ReadAllText(Path.Combine(_destinationDirectory, "root.txt")).ShouldBe("root");
    }

    [Fact]
    public void CopyDirectory_RejectsSourceAndDescendantDestinations()
    {
        Should.Throw<ArgumentException>(() =>
            FileUtility.CopyDirectory(_sourceDirectory, _sourceDirectory));
        Should.Throw<ArgumentException>(() =>
            FileUtility.CopyDirectory(_sourceDirectory, Path.Combine(_sourceDirectory, "nested-copy")));
    }

    [Fact]
    public async Task CopyDirectoryAsync_CopiesFullTree()
    {
        (await FileUtility.CopyDirectoryAsync(_sourceDirectory, _destinationDirectory)).ShouldBeTrue();

        File.ReadAllText(Path.Combine(_destinationDirectory, "root.txt")).ShouldBe("root");
        File.ReadAllText(Path.Combine(_destinationDirectory, "sub", "nested.txt")).ShouldBe("nested");
    }

    [Fact]
    public async Task CopyDirectoryAsync_MissingSourceReturnsFalse()
    {
        (await FileUtility.CopyDirectoryAsync(Path.Combine(_sourceDirectory, "missing"), _destinationDirectory))
            .ShouldBeFalse();
    }

    [Fact]
    public async Task CopyDirectoryAsync_ExistingDestinationWithoutOverwriteReturnsFalse()
    {
        Directory.CreateDirectory(_destinationDirectory);

        (await FileUtility.CopyDirectoryAsync(_sourceDirectory, _destinationDirectory)).ShouldBeFalse();
    }

    [Fact]
    public async Task CopyDirectoryAsync_OverwriteCopiesOverExistingFiles()
    {
        Directory.CreateDirectory(_destinationDirectory);
        File.WriteAllText(Path.Combine(_destinationDirectory, "root.txt"), "stale");

        (await FileUtility.CopyDirectoryAsync(_sourceDirectory, _destinationDirectory, overwrite: true)).ShouldBeTrue();

        File.ReadAllText(Path.Combine(_destinationDirectory, "root.txt")).ShouldBe("root");
    }

    [Fact]
    public async Task CopyDirectoryAsync_RejectsSourceAndDescendantDestinations()
    {
        await Should.ThrowAsync<ArgumentException>(() =>
            FileUtility.CopyDirectoryAsync(_sourceDirectory, _sourceDirectory));
        await Should.ThrowAsync<ArgumentException>(() =>
            FileUtility.CopyDirectoryAsync(_sourceDirectory, Path.Combine(_sourceDirectory, "nested-copy")));
    }

    [Fact]
    public async Task CopyDirectoryAsync_ThrowsWhenCancelled()
    {
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(() =>
            FileUtility.CopyDirectoryAsync(_sourceDirectory, _destinationDirectory, cancellationToken: cancellation.Token));
    }

    [Fact]
    public void CopyDirectory_ThrowsOnNullArguments()
    {
        Should.Throw<ArgumentNullException>(() => FileUtility.CopyDirectory(null!, _destinationDirectory));
        Should.Throw<ArgumentNullException>(() => FileUtility.CopyDirectory(_sourceDirectory, null!));
    }
}
