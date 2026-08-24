namespace Auxil.Utilities;

/// <summary>Carries a failure reported by the underlying filesystem watcher.</summary>
[PublicAPI]
public sealed class FileWatcherErrorEventArgs : EventArgs
{
    internal FileWatcherErrorEventArgs(Exception exception)
    {
        Exception = exception;
    }

    /// <summary>The exception that occurred.</summary>
    public Exception Exception { get; }

    /// <summary>True when the watcher's internal buffer overflowed and events were lost.</summary>
    public bool IsBufferOverflow => Exception is InternalBufferOverflowException;
}
