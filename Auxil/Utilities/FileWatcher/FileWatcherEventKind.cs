namespace Auxil.Utilities;

/// <summary>The kind of filesystem change reported by <see cref="FileWatcher"/>.</summary>
[PublicAPI]
public enum FileWatcherEventKind
{
    /// <summary>A file or directory was created.</summary>
    Created,

    /// <summary>A file or directory was changed.</summary>
    Changed,

    /// <summary>A file or directory was deleted.</summary>
    Deleted,

    /// <summary>A file or directory was renamed.</summary>
    Renamed,
}
