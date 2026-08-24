namespace Auxil.Common.Cli;

/// <summary>The result of executing a command-line process.</summary>
[PublicAPI]
public sealed record CliResult
{
    /// <summary>The process exit code.</summary>
    public int ExitCode { get; init; }
    /// <summary>The captured standard output.</summary>
    public string Output { get; init; } = string.Empty;
    /// <summary>The captured standard error output.</summary>
    public string ErrorOutput { get; init; } = string.Empty;
    /// <summary>The time the process started.</summary>
    public DateTime StartTime { get; init; }
    /// <summary>The time the process exited.</summary>
    public DateTime ExitTime { get; init; }

    /// <summary>Whether the process exited with code <c>0</c>.</summary>
    public bool Success => ExitCode == 0;

    /// <summary>The elapsed time between the process start and exit.</summary>
    public TimeSpan Duration => ExitTime - StartTime;
}
