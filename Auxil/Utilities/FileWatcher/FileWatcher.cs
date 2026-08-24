using System.Collections.Concurrent;
using System.Diagnostics;

namespace Auxil.Utilities;

/// <summary>
/// Watches a directory and delivers batches of filesystem changes after a quiet period.
/// Bursts are raised serially on a background thread; <see cref="Stop"/> and <see cref="Dispose"/>
/// flush any pending changes as a final batch. Keep handlers fast and do not call
/// <see cref="Stop"/> or <see cref="Dispose"/> from inside a handler.
/// </summary>
[PublicAPI]
public sealed class FileWatcher : IDisposable
{
    private readonly FileWatcherOptions _options;
    private readonly ConcurrentQueue<FileWatcherEventArgs> _events = new();
    private readonly ConcurrentQueue<Exception> _errors = new();
    private readonly ManualResetEventSlim _signal = new(false);
    private readonly Lock _lifecycleLock = new();
    private FileSystemWatcher? _watcher;
    private Thread? _dispatcher;
    private volatile bool _stopping;
    private volatile bool _disposed;

    /// <summary>Creates a watcher for the given directory. The directory must exist when <see cref="Start"/> is called.</summary>
    public FileWatcher(string directory, FileWatcherOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(directory);
        Directory = Path.GetFullPath(directory);
        _options = options ?? new FileWatcherOptions();
    }

    /// <summary>The watched directory, normalized to an absolute path.</summary>
    public string Directory { get; }

    /// <summary>Whether the watcher is currently watching.</summary>
    public bool IsRunning => _watcher is { EnableRaisingEvents: true };

    /// <summary>Raised with a batch of changes after the directory has been quiet for <see cref="FileWatcherOptions.Debounce"/>.</summary>
    public event EventHandler<FileWatcherBurstEventArgs>? Burst;

    /// <summary>Raised when the underlying watcher reports an error; check <see cref="FileWatcherErrorEventArgs.IsBufferOverflow"/>.</summary>
    public event EventHandler<FileWatcherErrorEventArgs>? Error;

    /// <summary>Starts watching. No-op when already running; throws when the directory does not exist.</summary>
    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_lifecycleLock)
        {
            if (_dispatcher is not null && !IsRunning)
            {
                throw new InvalidOperationException("The previous FileWatcher dispatcher is still shutting down.");
            }

            if (IsRunning)
            {
                return;
            }

            if (!System.IO.Directory.Exists(Directory))
            {
                throw new DirectoryNotFoundException($"The directory '{Directory}' does not exist.");
            }

            _stopping = false;
            _signal.Reset();

            _watcher = new FileSystemWatcher(Directory)
            {
                Filter = _options.Filter,
                IncludeSubdirectories = _options.IncludeSubdirectories,
                NotifyFilter = _options.NotifyFilters,
                InternalBufferSize = _options.InternalBufferSize,
            };

            _watcher.Created += OnFileSystemEvent;
            _watcher.Changed += OnFileSystemEvent;
            _watcher.Deleted += OnFileSystemEvent;
            _watcher.Renamed += OnRenamed;
            _watcher.Error += OnWatcherError;

            _dispatcher = new Thread(DispatchLoop)
            {
                IsBackground = true,
                Name = "FileWatcher dispatch",
            };

            _dispatcher.Start();

            _watcher.EnableRaisingEvents = true;
        }
    }

    /// <summary>Stops watching and delivers any pending changes as a final batch. No-op when not running.</summary>
    public void Stop()
    {
        lock (_lifecycleLock)
        {
            if (_disposed || _watcher is null)
            {
                return;
            }

            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _watcher = null;

            _stopping = true;
            _signal.Set();

            Thread? dispatcher = _dispatcher;

            if (dispatcher is not null && dispatcher != Thread.CurrentThread)
            {
                dispatcher.Join(TimeSpan.FromSeconds(2));

                if (!dispatcher.IsAlive && _dispatcher == dispatcher)
                {
                    _dispatcher = null;
                }
            }
        }
    }

    /// <summary>Stops the watcher and releases its resources.</summary>
    public void Dispose()
    {
        lock (_lifecycleLock)
        {
            if (_disposed)
            {
                return;
            }

            Stop();
            _disposed = true;
        }
    }

    private void DispatchLoop()
    {
        try
        {
            while (true)
            {
                _signal.Wait();
                _signal.Reset();

                DrainErrors();

                if (_stopping && _events.IsEmpty)
                {
                    break;
                }

                List<FileWatcherEventArgs> batch = DrainEvents();

                if (batch.Count > 0)
                {
                    if (!_stopping)
                    {
                        while (WaitForMore())
                        {
                            batch.AddRange(DrainEvents());
                        }
                    }

                    RaiseBurst(batch);
                }

                if (_stopping && _events.IsEmpty && _errors.IsEmpty)
                {
                    break;
                }
            }
        }
        finally
        {
            lock (_lifecycleLock)
            {
                if (_dispatcher == Thread.CurrentThread)
                {
                    _dispatcher = null;
                }
            }
        }
    }

    private bool WaitForMore()
    {
        _signal.Reset();
        return !_stopping && _signal.Wait(_options.Debounce);
    }

    private List<FileWatcherEventArgs> DrainEvents()
    {
        List<FileWatcherEventArgs> batch = [];
        while (_events.TryDequeue(out FileWatcherEventArgs? item))
        {
            batch.Add(item);
        }

        return batch;
    }

    private void DrainErrors()
    {
        while (_errors.TryDequeue(out Exception? error))
        {
            RaiseError(error);
        }
    }

    private void OnFileSystemEvent(object sender, FileSystemEventArgs e)
    {
        FileWatcherEventKind kind = e.ChangeType switch
        {
            WatcherChangeTypes.Created => FileWatcherEventKind.Created,
            WatcherChangeTypes.Changed => FileWatcherEventKind.Changed,
            WatcherChangeTypes.Deleted => FileWatcherEventKind.Deleted,
            _ => FileWatcherEventKind.Changed,
        };

        EnqueueEvent(kind, e.FullPath, null);
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        EnqueueEvent(FileWatcherEventKind.Renamed, e.FullPath, e.OldFullPath);
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        _errors.Enqueue(e.GetException());
        _signal.Set();
    }

    private void EnqueueEvent(FileWatcherEventKind kind, string fullPath, string? oldFullPath)
    {
        string normalizedPath = Path.GetFullPath(fullPath);
        string relativePath = GetRelativePath(normalizedPath);

        if (IsIgnored(relativePath, _options.IgnoredPaths))
        {
            return;
        }

        string? normalizedOldPath = oldFullPath is null ? null : Path.GetFullPath(oldFullPath);
        string? normalizedOldRelativePath = normalizedOldPath is null ? null : GetRelativePath(normalizedOldPath);

        _events.Enqueue(new FileWatcherEventArgs(kind, normalizedPath, relativePath,
            normalizedOldPath, normalizedOldRelativePath, System.IO.Directory.Exists(normalizedPath)));
        _signal.Set();
    }

    private string GetRelativePath(string fullPath)
    {
        return Path.GetRelativePath(Directory, fullPath).ToForwardSlash();
    }

    private void RaiseBurst(List<FileWatcherEventArgs> batch)
    {
        EventHandler<FileWatcherBurstEventArgs>? handlers = Burst;
        if (handlers is null)
        {
            return;
        }

        FileWatcherBurstEventArgs eventArgs = new(batch.AsReadOnly());
        foreach (EventHandler<FileWatcherBurstEventArgs> handler in handlers.GetInvocationList())
        {
            try
            {
                handler(this, eventArgs);
            }
            catch (Exception exception)
            {
                ReportCallbackFailure(exception);
            }
        }
    }

    private void RaiseError(Exception exception)
    {
        EventHandler<FileWatcherErrorEventArgs>? handlers = Error;
        if (handlers is null)
        {
            return;
        }

        FileWatcherErrorEventArgs eventArgs = new(exception);
        foreach (EventHandler<FileWatcherErrorEventArgs> handler in handlers.GetInvocationList())
        {
            try
            {
                handler(this, eventArgs);
            }
            catch (Exception handlerException)
            {
                ReportCallbackFailure(handlerException);
            }
        }
    }

    private static void ReportCallbackFailure(Exception exception)
    {
        Trace.WriteLine($"FileWatcher callback failed: {exception}");
    }

    internal static bool IsIgnored(string relativePath, IReadOnlyList<string> ignoredPaths)
    {
        foreach (string entry in ignoredPaths)
        {
            string normalized = entry.ToForwardSlash().TrimEnd('/');

            if (normalized.Length == 0)
            {
                continue;
            }

            if (relativePath.Equals(normalized, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (relativePath.StartsWith(normalized + "/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
