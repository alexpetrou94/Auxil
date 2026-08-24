namespace Auxil.Utilities;

/// <summary>Provides file and directory copy helpers.</summary>
[PublicAPI]
public static class FileUtility
{
    /// <summary>Copies a directory tree to a destination without deleting existing content.</summary>
    /// <param name="source">The source directory.</param>
    /// <param name="destination">The destination directory.</param>
    /// <param name="overwrite">Whether existing files in the destination may be overwritten.</param>
    /// <returns><c>false</c> when the source is missing or the destination exists without overwrite; otherwise <c>true</c>.</returns>
    public static bool CopyDirectory(string source, string destination, bool overwrite = false)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        source = Path.GetFullPath(source);
        destination = Path.GetFullPath(destination);

        if (!Directory.Exists(source))
        {
            return false;
        }

        ValidateDestination(source, destination);

        if (!overwrite && Directory.Exists(destination))
        {
            return false;
        }

        Directory.CreateDirectory(destination);

        foreach (string file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(source, file);
            string destinationFile = Path.Combine(destination, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
            File.Copy(file, destinationFile, overwrite);
        }

        return true;
    }

    /// <summary>Asynchronously copies a directory tree to a destination without deleting existing content.</summary>
    /// <param name="source">The source directory.</param>
    /// <param name="destination">The destination directory.</param>
    /// <param name="overwrite">Whether existing files in the destination may be overwritten.</param>
    /// <param name="cancellationToken">Cancels the copy operation.</param>
    /// <returns><c>false</c> when the source is missing or the destination exists without overwrite; otherwise <c>true</c>.</returns>
    public static async Task<bool> CopyDirectoryAsync(string source, string destination, bool overwrite = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        source = Path.GetFullPath(source);
        destination = Path.GetFullPath(destination);

        if (!Directory.Exists(source))
        {
            return false;
        }

        ValidateDestination(source, destination);
        cancellationToken.ThrowIfCancellationRequested();

        if (!overwrite && Directory.Exists(destination))
        {
            return false;
        }

        Directory.CreateDirectory(destination);

        foreach (string file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            string relativePath = Path.GetRelativePath(source, file);
            string destinationFile = Path.Combine(destination, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);

            await using FileStream sourceStream = File.OpenRead(file);
            FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;
            await using FileStream destinationStream = new(destinationFile, mode, FileAccess.Write, FileShare.None);

            await sourceStream.CopyToAsync(destinationStream, cancellationToken).ConfigureAwait(false);
        }

        return true;
    }

    private static void ValidateDestination(string source, string destination)
    {
        string relativePath = Path.GetRelativePath(source, destination);
        bool isSamePath = relativePath == ".";
        bool isDescendant = relativePath != ".."
            && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            && !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
            && !Path.IsPathRooted(relativePath);

        if (isSamePath || isDescendant)
        {
            throw new ArgumentException(
                "The destination must not be the source directory or a descendant of it.",
                nameof(destination));
        }
    }
}
