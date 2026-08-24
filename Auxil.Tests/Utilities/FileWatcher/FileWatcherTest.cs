using JetBrains.Annotations;
using Auxil.Utilities;

namespace Auxil.Tests.Utilities;

[TestSubject(typeof(FileWatcher))]
public class FileWatcherTest : IDisposable
{
    private sealed class Harness : IDisposable
    {
        private readonly List<FileWatcherBurstEventArgs> _bursts = [];

        public string Root { get; }
        public FileWatcher Watcher { get; }

        public Harness(FileWatcherOptions? options = null)
        {
            Root = Path.Combine(Path.GetTempPath(), $"uphoric-fw-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Root);
            Watcher = new FileWatcher(Root,
                options ?? new FileWatcherOptions { Debounce = TimeSpan.FromMilliseconds(40) });
            Watcher.Burst += (_, e) =>
            {
                lock (_bursts)
                {
                    _bursts.Add(e);
                }
            };
            Watcher.Start();
        }

        public IReadOnlyList<FileWatcherBurstEventArgs> Bursts
        {
            get
            {
                lock (_bursts)
                {
                    return _bursts.ToArray();
                }
            }
        }

        public async Task WaitForBurstCountAsync(int minimum, TimeSpan? timeout = null)
        {
            DateTime deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(10));
            while (Bursts.Count < minimum)
            {
                if (DateTime.UtcNow >= deadline)
                {
                    throw new TimeoutException($"Expected {minimum} burst(s) within {timeout}, got {Bursts.Count}.");
                }

                await Task.Delay(20);
            }
        }

        public async Task WaitForPathsAsync(IReadOnlyCollection<string> paths, TimeSpan? timeout = null)
        {
            DateTime deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(10));
            while (true)
            {
                HashSet<string> seen = Bursts.SelectMany(b => b.Events).Select(e => e.RelativePath).ToHashSet();
                if (paths.All(seen.Contains))
                {
                    return;
                }

                if (DateTime.UtcNow >= deadline)
                {
                    throw new TimeoutException($"Expected paths [{string.Join(", ", paths)}], got [{string.Join(", ", seen)}].");
                }

                await Task.Delay(20);
            }
        }

        public void Dispose()
        {
            Watcher.Dispose();
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, true);
            }
        }
    }

    private readonly Harness _harness = new();

    public void Dispose()
    {
        _harness.Dispose();
    }

    [Fact]
    public void Start_ThrowsWhenDirectoryMissing()
    {
        string missing = Path.Combine(Path.GetTempPath(), $"uphoric-fw-missing-{Guid.NewGuid():N}");
        FileWatcher watcher = new(missing);

        Should.Throw<DirectoryNotFoundException>(watcher.Start);
    }

    [Fact]
    public void StartAndStop_AreIdempotent()
    {
        _harness.Watcher.Start();
        _harness.Watcher.IsRunning.ShouldBeTrue();

        _harness.Watcher.Stop();
        _harness.Watcher.IsRunning.ShouldBeFalse();
        _harness.Watcher.Stop();
        _harness.Watcher.IsRunning.ShouldBeFalse();
    }

    [Fact]
    public void Start_AfterDispose_Throws()
    {
        _harness.Watcher.Dispose();

        Should.Throw<ObjectDisposedException>(_harness.Watcher.Start);
    }

    [Fact]
    public async Task CreatedFile_IsDeliveredWithRelativePath()
    {
        string file = Path.Combine(_harness.Root, "hello.txt");
        File.WriteAllText(file, "hi");

        await _harness.WaitForBurstCountAsync(1);

        string fullPath = Path.GetFullPath(file);
        _harness.Bursts.SelectMany(b => b.Events)
            .Any(e => e is
                {
                    Kind: FileWatcherEventKind.Created,
                    RelativePath: "hello.txt",
                    OldFullPath: null,
                    IsDirectory: false,
                } && e.FullPath == fullPath)
            .ShouldBeTrue();
    }

    [Fact]
    public async Task NestedFile_UsesForwardSlashRelativePath()
    {
        Directory.CreateDirectory(Path.Combine(_harness.Root, "sub"));
        await _harness.WaitForPathsAsync(["sub"]);

        string file = Path.Combine(_harness.Root, "sub", "nested.txt");
        File.WriteAllText(file, "hi");

        await _harness.WaitForPathsAsync(["sub/nested.txt"]);

        FileWatcherEventArgs[] delivered = _harness.Bursts
            .SelectMany(b => b.Events)
            .Where(e => e.RelativePath.EndsWith("nested.txt"))
            .ToArray();
        delivered.ShouldNotBeEmpty();
        delivered.All(e => e.RelativePath == "sub/nested.txt").ShouldBeTrue();
    }

    [Fact]
    public async Task ChangedFile_IsDelivered()
    {
        string file = Path.Combine(_harness.Root, "a.txt");
        File.WriteAllText(file, "one");
        await _harness.WaitForPathsAsync(["a.txt"]);

        File.WriteAllText(file, "two");

        DateTime deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        while (_harness.Bursts.SelectMany(b => b.Events).All(e => e.Kind != FileWatcherEventKind.Changed))
        {
            if (DateTime.UtcNow >= deadline)
            {
                throw new TimeoutException("No Changed event delivered.");
            }

            await Task.Delay(20);
        }
    }

    [Fact]
    public async Task Filter_OnlyDeliversMatchingFiles()
    {
        Harness harness = new(new FileWatcherOptions
        {
            Debounce = TimeSpan.FromMilliseconds(40),
            Filter = "*.txt",
        });
        try
        {
            File.WriteAllText(Path.Combine(harness.Root, "a.txt"), "x");
            File.WriteAllText(Path.Combine(harness.Root, "b.log"), "x");

            await harness.WaitForPathsAsync(["a.txt"]);
            await Task.Delay(400);

            harness.Bursts.SelectMany(b => b.Events).Any(e => e.RelativePath == "b.log").ShouldBeFalse();
        }
        finally
        {
            harness.Dispose();
        }
    }

    [Fact]
    public async Task IncludeSubdirectoriesFalse_DeliversOnlyRootChanges()
    {
        Harness harness = new(new FileWatcherOptions
        {
            Debounce = TimeSpan.FromMilliseconds(40),
            IncludeSubdirectories = false,
        });
        try
        {
            Directory.CreateDirectory(Path.Combine(harness.Root, "sub"));
            await harness.WaitForPathsAsync(["sub"]);

            File.WriteAllText(Path.Combine(harness.Root, "sub", "nested.txt"), "x");
            await Task.Delay(400);

            harness.Bursts.SelectMany(b => b.Events).Any(e => e.RelativePath == "sub/nested.txt").ShouldBeFalse();
        }
        finally
        {
            harness.Dispose();
        }
    }

    [Fact]
    public async Task MultipleWrites_CoalesceIntoOneBurst()
    {
        Harness slow = new(new FileWatcherOptions { Debounce = TimeSpan.FromMilliseconds(200) });
        try
        {
            for (int i = 0; i < 3; i++)
            {
                File.WriteAllText(Path.Combine(slow.Root, $"f{i}.txt"), "x");
            }

            await slow.WaitForPathsAsync(["f0.txt", "f1.txt", "f2.txt"]);
            await Task.Delay(300);

            slow.Bursts.Count.ShouldBe(1);
            slow.Bursts[0].Events.Select(e => e.RelativePath).Distinct().Count().ShouldBe(3);
        }
        finally
        {
            slow.Dispose();
        }
    }

    [Fact]
    public async Task Rename_IsDeliveredAsSingleRenamedEvent()
    {
        string from = Path.Combine(_harness.Root, "a.txt");
        string to = Path.Combine(_harness.Root, "b.txt");
        File.WriteAllText(from, "x");
        await _harness.WaitForPathsAsync(["a.txt"]);

        File.Move(from, to);

        DateTime deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        while (true)
        {
            FileWatcherEventArgs? renamed = _harness.Bursts
                .SelectMany(b => b.Events)
                .SingleOrDefault(e => e is { Kind: FileWatcherEventKind.Renamed, RelativePath: "b.txt" });
            if (renamed is not null)
            {
                renamed.OldRelativePath.ShouldBe("a.txt");
                renamed.OldFullPath.ShouldBe(Path.GetFullPath(from));
                return;
            }

            if (DateTime.UtcNow >= deadline)
            {
                throw new TimeoutException("No Renamed event delivered.");
            }

            await Task.Delay(20);
        }
    }

    [Fact]
    public async Task Rename_ToIgnoredPath_IsDropped()
    {
        Harness harness = new(new FileWatcherOptions
        {
            Debounce = TimeSpan.FromMilliseconds(40),
            IgnoredPaths = ["out.txt"],
        });
        try
        {
            string from = Path.Combine(harness.Root, "a.txt");
            string to = Path.Combine(harness.Root, "out.txt");
            File.WriteAllText(from, "x");
            await harness.WaitForPathsAsync(["a.txt"]);
            int baseline = harness.Bursts.Count;

            File.Move(from, to);
            await Task.Delay(400);

            harness.Bursts.Count.ShouldBe(baseline);
            harness.Bursts.SelectMany(b => b.Events).Any(e => e.Kind == FileWatcherEventKind.Renamed).ShouldBeFalse();
        }
        finally
        {
            harness.Dispose();
        }
    }

    [Fact]
    public async Task IgnoredSegment_IsDropped()
    {
        Harness harness = new(new FileWatcherOptions
        {
            Debounce = TimeSpan.FromMilliseconds(40),
            IgnoredPaths = ["bin"],
        });
        try
        {
            Directory.CreateDirectory(Path.Combine(harness.Root, "bin"));
            await Task.Delay(300);

            File.WriteAllText(Path.Combine(harness.Root, "bin", "a.dll"), "x");
            await Task.Delay(400);

            harness.Bursts.ShouldBeEmpty();
        }
        finally
        {
            harness.Dispose();
        }
    }

    [Fact]
    public async Task DeletedFile_IsDelivered()
    {
        string file = Path.Combine(_harness.Root, "a.txt");
        File.WriteAllText(file, "x");
        await _harness.WaitForPathsAsync(["a.txt"]);

        File.Delete(file);

        DateTime deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        while (_harness.Bursts.SelectMany(b => b.Events).All(e => e.Kind != FileWatcherEventKind.Deleted))
        {
            if (DateTime.UtcNow >= deadline)
            {
                throw new TimeoutException("No Deleted event delivered.");
            }

            await Task.Delay(20);
        }
    }

    [Fact]
    public async Task DirectoryCreated_ReportsIsDirectory()
    {
        Directory.CreateDirectory(Path.Combine(_harness.Root, "assets"));
        await _harness.WaitForPathsAsync(["assets"]);

        FileWatcherEventArgs? delivered = _harness.Bursts
            .SelectMany(b => b.Events)
            .FirstOrDefault(e => e.RelativePath == "assets");
        delivered.ShouldNotBeNull();
        delivered.IsDirectory.ShouldBeTrue();
    }

    [Fact]
    public async Task Stop_FlushesPendingBurst()
    {
        Harness slow = new(new FileWatcherOptions { Debounce = TimeSpan.FromMilliseconds(500) });
        try
        {
            string file = Path.Combine(slow.Root, "last.txt");
            File.WriteAllText(file, "x");
            await Task.Delay(300);

            slow.Watcher.Stop();

            DateTime deadline = DateTime.UtcNow + TimeSpan.FromSeconds(3);
            while (slow.Bursts.Count == 0)
            {
                if (DateTime.UtcNow >= deadline)
                {
                    throw new TimeoutException("Pending burst was not flushed on Stop.");
                }

                await Task.Delay(20);
            }

            slow.Bursts.SelectMany(b => b.Events).Any(e => e.RelativePath == "last.txt").ShouldBeTrue();
        }
        finally
        {
            slow.Dispose();
        }
    }

    [Fact]
    public async Task Bursts_AreRaisedSerially()
    {
        int concurrent = 0;
        bool overlap = false;
        FileWatcher watcher = new(_harness.Root,
            new FileWatcherOptions { Debounce = TimeSpan.FromMilliseconds(20) });
        watcher.Burst += (_, _) =>
        {
            int now = Interlocked.Increment(ref concurrent);
            if (now != 1)
            {
                Volatile.Write(ref overlap, true);
            }

            Thread.Sleep(5);
            Interlocked.Decrement(ref concurrent);
        };
        watcher.Start();

        try
        {
            List<string> paths = new();
            for (int i = 0; i < 10; i++)
            {
                string path = $"s{i}.txt";
                File.WriteAllText(Path.Combine(_harness.Root, path), "x");
                paths.Add(path);
            }

            await _harness.WaitForPathsAsync(paths, TimeSpan.FromSeconds(15));
            await Task.Delay(200);

            overlap.ShouldBeFalse();
        }
        finally
        {
            watcher.Dispose();
        }
    }

    [Fact]
    public async Task ThrowingBurstHandler_DoesNotStopFutureDispatch()
    {
        Harness harness = new(new FileWatcherOptions { Debounce = TimeSpan.FromMilliseconds(40) });
        int callbackCount = 0;
        TaskCompletionSource firstCallback = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource secondCallback = new(TaskCreationOptions.RunContinuationsAsynchronously);
        harness.Watcher.Burst += (_, _) =>
        {
            if (Interlocked.Increment(ref callbackCount) == 1)
            {
                firstCallback.SetResult();
                throw new InvalidOperationException("expected callback failure");
            }

            secondCallback.SetResult();
        };

        try
        {
            File.WriteAllText(Path.Combine(harness.Root, "first.txt"), "x");
            await firstCallback.Task.WaitAsync(TimeSpan.FromSeconds(10));

            File.WriteAllText(Path.Combine(harness.Root, "second.txt"), "x");
            await secondCallback.Task.WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            harness.Dispose();
        }
    }

    [Fact]
    public async Task Start_DuringTimedOutStopIsRejectedUntilDispatcherExits()
    {
        string root = Path.Combine(Path.GetTempPath(), $"uphoric-fw-blocked-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        FileWatcher watcher = new(root, new FileWatcherOptions { Debounce = TimeSpan.FromMilliseconds(20) });
        TaskCompletionSource callbackStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseCallback = new(TaskCreationOptions.RunContinuationsAsynchronously);
        watcher.Burst += (_, _) =>
        {
            callbackStarted.SetResult();
            releaseCallback.Task.GetAwaiter().GetResult();
        };
        watcher.Start();

        try
        {
            File.WriteAllText(Path.Combine(root, "blocked.txt"), "x");
            await callbackStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));

            await Task.Run(watcher.Stop).WaitAsync(TimeSpan.FromSeconds(5));
            Should.Throw<InvalidOperationException>(watcher.Start);

            releaseCallback.SetResult();
            DateTime deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
            while (true)
            {
                try
                {
                    watcher.Start();
                    break;
                }
                catch (InvalidOperationException) when (DateTime.UtcNow < deadline)
                {
                    await Task.Yield();
                }
            }
        }
        finally
        {
            releaseCallback.TrySetResult();
            watcher.Dispose();
            Directory.Delete(root, true);
        }
    }
}
