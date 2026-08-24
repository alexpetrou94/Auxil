using JetBrains.Annotations;
using Auxil.Utilities;

namespace Auxil.Tests.Utilities;

[TestSubject(typeof(FileWatcher))]
public class FileWatcherIgnoreTest
{
    [Fact]
    public void EmptyList_IgnoresNothing()
    {
        FileWatcher.IsIgnored("a.txt", []).ShouldBeFalse();
    }

    [Fact]
    public void ExactFileMatch_IsIgnored()
    {
        FileWatcher.IsIgnored("src/generated.cs", ["src/generated.cs"]).ShouldBeTrue();
    }

    [Fact]
    public void DirectoryPrefix_IsIgnored()
    {
        FileWatcher.IsIgnored("bin/x/a.dll", ["bin"]).ShouldBeTrue();
        FileWatcher.IsIgnored("bin", ["bin"]).ShouldBeTrue();
    }

    [Fact]
    public void SimilarSegmentName_IsNotIgnored()
    {
        FileWatcher.IsIgnored("bin2/x/a.dll", ["bin"]).ShouldBeFalse();
        FileWatcher.IsIgnored("src/generated.cs.bak", ["src/generated.cs"]).ShouldBeFalse();
        FileWatcher.IsIgnored("src/generated.csx", ["src/generated.cs"]).ShouldBeFalse();
    }

    [Fact]
    public void Matching_IsCaseInsensitive()
    {
        FileWatcher.IsIgnored("BIN/X/A.DLL", ["bin"]).ShouldBeTrue();
        FileWatcher.IsIgnored("src/Generated.cs", ["SRC/generated.cs"]).ShouldBeTrue();
    }

    [Fact]
    public void BackslashEntries_AreNormalized()
    {
        FileWatcher.IsIgnored("bin/obj/x/a.dll", ["bin\\obj"]).ShouldBeTrue();
    }

    [Fact]
    public void TrailingSlashEntry_IsAccepted()
    {
        FileWatcher.IsIgnored("bin/x/a.dll", ["bin/"]).ShouldBeTrue();
    }

    [Fact]
    public void EmptyEntry_IsSkipped()
    {
        FileWatcher.IsIgnored("a.txt", [""]).ShouldBeFalse();
    }

    [Fact]
    public void AnyEntryMatching_Ignores()
    {
        FileWatcher.IsIgnored("bin/x/a.dll", ["obj", "bin"]).ShouldBeTrue();
        FileWatcher.IsIgnored("obj/x/a.dll", ["obj", "bin"]).ShouldBeTrue();
        FileWatcher.IsIgnored("src/a.cs", ["obj", "bin"]).ShouldBeFalse();
    }
}
