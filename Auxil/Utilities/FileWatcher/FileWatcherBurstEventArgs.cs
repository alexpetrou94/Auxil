namespace Auxil.Utilities;

/// <summary>Carries the batch of changes delivered by <see cref="FileWatcher.Burst"/>.</summary>
[PublicAPI]
public sealed class FileWatcherBurstEventArgs : EventArgs
{
    internal FileWatcherBurstEventArgs(IReadOnlyList<FileWatcherEventArgs> events)
    {
        Events = events;
    }

    /// <summary>The changes in this batch, in approximate chronological order.</summary>
    public IReadOnlyList<FileWatcherEventArgs> Events { get; }
}
