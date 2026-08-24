using System.Diagnostics;
using System.Text;

namespace Auxil.Common.Cli;

/// <summary>A fluent builder for executing command-line processes with output capture.</summary>
[PublicAPI]
public class Cli
{
    private readonly List<string> _argumentList = [];
    private readonly Dictionary<string, string> _environmentVariables = [];

    /// <summary>The executable to run.</summary>
    public string Command { get; set; } = string.Empty;
    /// <summary>The raw argument string passed to the process.</summary>
    public string Arguments { get; set; } = string.Empty;
    /// <summary>The working directory of the process.</summary>
    public string WorkingDirectory { get; set; } = string.Empty;
    /// <summary>Whether to use the shell to launch the process.</summary>
    public bool UseShellExecute { get; set; }
    /// <summary>Whether the process runs without a console window.</summary>
    public bool CreateNoWindow { get; set; } = true;
    /// <summary>Whether standard error is redirected and captured.</summary>
    public bool RedirectStandardError { get; set; } = true;
    /// <summary>Whether standard output is redirected and captured.</summary>
    public bool RedirectStandardOutput { get; set; } = true;
    /// <summary>Raised for each line of standard output.</summary>
    public event EventHandler<string>? OutputReceived;
    /// <summary>Raised for each line of standard error output.</summary>
    public event EventHandler<string>? ErrorOutputReceived;

    /// <summary>Sets the executable to run.</summary>
    /// <param name="command">The executable.</param>
    public Cli Call(string command)
    {
        ArgumentNullException.ThrowIfNull(command);

        Command = command;

        return this;
    }

    /// <summary>Executes the process synchronously and returns the captured result.</summary>
    public CliResult Execute() => ExecuteAsync().GetAwaiter().GetResult();

    /// <summary>Executes the process and returns the captured result.</summary>
    /// <param name="cancellationToken">Cancels the wait for the process.</param>
    /// <param name="timeout">The maximum time to wait; when exceeded the process is killed and a <see cref="TimeoutException"/> is thrown.</param>
    public async Task<CliResult> ExecuteAsync(CancellationToken cancellationToken = default, TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(Command);

        Process process = new();

        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = Command,
                CreateNoWindow = CreateNoWindow,
                WorkingDirectory = WorkingDirectory,
                RedirectStandardOutput = RedirectStandardOutput,
                RedirectStandardError = RedirectStandardError,
                UseShellExecute = UseShellExecute
            };

            if (Arguments.Length > 0)
            {
                startInfo.Arguments = Arguments;
            }

            foreach (string argument in _argumentList)
            {
                startInfo.ArgumentList.Add(argument);
            }

            foreach (KeyValuePair<string, string> variable in _environmentVariables)
            {
                startInfo.Environment[variable.Key] = variable.Value;
            }

            process.StartInfo = startInfo;

            StringBuilder outputBuilder = new();
            StringBuilder errorsBuilder = new();

            process.OutputDataReceived += (_, args) =>
            {
                outputBuilder.AppendWithNewLine(args.Data);
                OnOutputReceived(args.Data);
            };

            process.ErrorDataReceived += (_, args) =>
            {
                errorsBuilder.AppendWithNewLine(args.Data);
                OnErrorOutputReceived(args.Data);
            };

            process.Start();

            // StartTime/ExitTime read /proc/<pid>/stat on Unix, which is gone after the
            // process is reaped by WaitForExitAsync — capture the start time while the
            // process is alive and derive the exit time from a stopwatch instead.
            DateTime startTime = process.StartTime;
            Stopwatch stopwatch = Stopwatch.StartNew();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using CancellationTokenSource? timeoutSource = timeout is null
                ? null
                : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            if (timeoutSource is not null)
            {
                timeoutSource.CancelAfter(timeout!.Value);
            }

            try
            {
                await process.WaitForExitAsync(timeoutSource?.Token ?? cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (timeoutSource is not null && timeoutSource.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                TryKillProcess(process);

                throw new TimeoutException($"The process '{Command}' did not finish within the configured timeout.");
            }

            // Settles the process object on Unix (WaitForExitAsync releases the handle there,
            // making ExitCode/StartTime/ExitTime throw) and drains the async output handlers.
            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
            process.WaitForExit();
            stopwatch.Stop();

            return new CliResult
            {
                Output = outputBuilder.ToString(),
                ErrorOutput = errorsBuilder.ToString(),
                ExitCode = process.ExitCode,
                StartTime = startTime,
                ExitTime = startTime + stopwatch.Elapsed
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            TryKillProcess(process);

            throw;
        }
        finally
        {
            process.Dispose();
        }
    }

    /// <summary>Sets the working directory of the process.</summary>
    /// <param name="workingDirectory">The working directory.</param>
    public Cli SetWorkingDirectory(string workingDirectory)
    {
        ArgumentNullException.ThrowIfNull(workingDirectory);

        WorkingDirectory = workingDirectory;

        return this;
    }

    /// <summary>Sets the raw argument string; clears any argument list set via the params overload.</summary>
    /// <param name="arguments">The argument string.</param>
    public Cli SetArguments(string arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        Arguments = arguments;
        _argumentList.Clear();

        return this;
    }

    /// <summary>Sets the arguments via the process argument list, avoiding quoting bugs; clears any raw argument string.</summary>
    /// <param name="arguments">The arguments.</param>
    public Cli SetArguments(params string[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        Arguments = string.Empty;
        _argumentList.Clear();
        _argumentList.AddRange(arguments);

        return this;
    }

    /// <summary>Sets an environment variable for the process.</summary>
    /// <param name="name">The variable name.</param>
    /// <param name="value">The variable value.</param>
    public Cli SetEnvironmentVariable(string name, string value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);

        _environmentVariables[name] = value;

        return this;
    }

    /// <summary>Sets whether the process runs without a console window.</summary>
    /// <param name="createNoWindow">Whether to suppress the console window.</param>
    public Cli SetCreateNoWindow(bool createNoWindow)
    {
        CreateNoWindow = createNoWindow;

        return this;
    }

    /// <summary>Sets whether the process is launched through the shell.</summary>
    /// <param name="useShellExecute">Whether to use the shell.</param>
    public Cli SetUseShellExecute(bool useShellExecute)
    {
        UseShellExecute = useShellExecute;

        return this;
    }

    /// <summary>Sets whether standard error is captured.</summary>
    /// <param name="redirectStandardError">Whether to redirect standard error.</param>
    public Cli SetRedirectStandardError(bool redirectStandardError)
    {
        RedirectStandardError = redirectStandardError;

        return this;
    }

    /// <summary>Sets whether standard output is captured.</summary>
    /// <param name="redirectStandardOutput">Whether to redirect standard output.</param>
    public Cli SetRedirectStandardOutput(bool redirectStandardOutput)
    {
        RedirectStandardOutput = redirectStandardOutput;

        return this;
    }

    private void OnOutputReceived(string? message)
    {
        OutputReceived?.Invoke(this, message ?? string.Empty);
    }

    private void OnErrorOutputReceived(string? message)
    {
        ErrorOutputReceived?.Invoke(this, message ?? string.Empty);
    }

    private static void TryKillProcess(Process process)
    {
        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
        }
    }
}
