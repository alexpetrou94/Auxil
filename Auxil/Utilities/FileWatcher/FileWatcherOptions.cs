namespace Auxil.Utilities;

/// <summary>Configuration for <see cref="FileWatcher"/>.</summary>
[PublicAPI]
public sealed class FileWatcherOptions
{
    /// <summary>
    /// The wildcard filter applied to file names, e.g. <c>*.gd</c>. Directory changes are always
    /// reported regardless of the filter; use <see cref="IgnoredPaths"/> to exclude them.
    /// </summary>
    public string Filter { get; init; } = "*";

    /// <summary>Whether changes in subdirectories raise events.</summary>
    public bool IncludeSubdirectories { get; init; } = true;

    /// <summary>The kinds of filesystem activity to observe.</summary>
    public NotifyFilters NotifyFilters { get; init; } =
        NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;

    /// <summary>How long the directory must stay quiet before a batch of changes is delivered.</summary>
    public TimeSpan Debounce { get; init; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Relative paths to ignore, compared case-insensitively. An entry matches when a relative
    /// path equals it or starts with it followed by '/' — e.g. <c>bin</c> ignores everything under
    /// <c>bin</c>, and <c>src/generated.cs</c> ignores a single file.
    /// </summary>
    public IReadOnlyList<string> IgnoredPaths { get; init; } = [];

    /// <summary>
    /// The internal event buffer size in bytes. Increase when events are lost under heavy churn;
    /// the loss surfaces through <see cref="FileWatcher.Error"/> with
    /// <see cref="FileWatcherErrorEventArgs.IsBufferOverflow"/>.
    /// </summary>
    public int InternalBufferSize { get; init; } = 8192;
}
