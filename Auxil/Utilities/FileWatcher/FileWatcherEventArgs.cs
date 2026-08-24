namespace Auxil.Utilities;

/// <summary>Describes a single filesystem change observed by <see cref="FileWatcher"/>.</summary>
[PublicAPI]
public sealed class FileWatcherEventArgs : EventArgs
{
    internal FileWatcherEventArgs(FileWatcherEventKind kind, string fullPath, string relativePath,
        string? oldFullPath, string? oldRelativePath, bool isDirectory)
    {
        Kind = kind;
        FullPath = fullPath;
        RelativePath = relativePath;
        OldFullPath = oldFullPath;
        OldRelativePath = oldRelativePath;
        IsDirectory = isDirectory;
    }

    /// <summary>The kind of change.</summary>
    public FileWatcherEventKind Kind { get; }

    /// <summary>The absolute path of the changed file or directory.</summary>
    public string FullPath { get; }

    /// <summary>The path relative to the watched directory, using '/' separators.</summary>
    public string RelativePath { get; }

    /// <summary>For <see cref="FileWatcherEventKind.Renamed"/>, the previous absolute path; otherwise <c>null</c>.</summary>
    public string? OldFullPath { get; }

    /// <summary>For <see cref="FileWatcherEventKind.Renamed"/>, the previous relative path; otherwise <c>null</c>.</summary>
    public string? OldRelativePath { get; }

    /// <summary>Best-effort directory indicator checked at event time; not reliable for deleted paths.</summary>
    public bool IsDirectory { get; }
}
