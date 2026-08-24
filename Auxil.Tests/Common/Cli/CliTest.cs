using JetBrains.Annotations;
using Auxil.Common.Cli;

namespace Auxil.Tests.Common.Cli;

[TestSubject(typeof(Auxil.Common.Cli.Cli))]
public class CliTest
{
    [Fact]
    public void Execute_ReturnsZeroExitCodeForSuccessfulCommand()
    {
        CliResult result = new Auxil.Common.Cli.Cli()
            .Call("dotnet")
            .SetArguments("--version")
            .Execute();

        result.ExitCode.ShouldBe(0);
        result.Success.ShouldBeTrue();
        result.Output.ShouldNotBeEmpty();
    }

    [Fact]
    public void Execute_ReturnsNonZeroExitCodeForFailingCommand()
    {
        CliResult result = new Auxil.Common.Cli.Cli()
            .Call("dotnet")
            .SetArguments("definitely-not-a-real-command-xyz")
            .Execute();

        result.ExitCode.ShouldNotBe(0);
        result.Success.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSameResultsAsSync()
    {
        CliResult result = await new Auxil.Common.Cli.Cli()
            .Call("dotnet")
            .SetArguments("--version")
            .ExecuteAsync();

        result.ExitCode.ShouldBe(0);
        result.Output.ShouldNotBeEmpty();
    }

    [Fact]
    public void Execute_CapturesErrorOutput()
    {
        CliResult result = new Auxil.Common.Cli.Cli()
            .Call("dotnet")
            .SetArguments("definitely-not-a-real-command-xyz")
            .Execute();

        result.ErrorOutput.ShouldNotBeEmpty();
    }

    [Fact]
    public void Execute_RaisesOutputEvent()
    {
        Auxil.Common.Cli.Cli cli = new Auxil.Common.Cli.Cli()
            .Call("dotnet")
            .SetArguments("--version");

        int outputEvents = 0;
        cli.OutputReceived += (_, _) => outputEvents++;

        cli.Execute();

        outputEvents.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Execute_RaisesErrorEvent()
    {
        Auxil.Common.Cli.Cli cli = new Auxil.Common.Cli.Cli()
            .Call("dotnet")
            .SetArguments("definitely-not-a-real-command-xyz");

        int errorEvents = 0;
        cli.ErrorOutputReceived += (_, _) => errorEvents++;

        cli.Execute();

        errorEvents.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void SetArguments_WithParamsUsesArgumentList()
    {
        CliResult result = new Auxil.Common.Cli.Cli()
            .Call("dotnet")
            .SetArguments("--version")
            .Execute();

        result.ExitCode.ShouldBe(0);
        result.Output.ShouldNotBeEmpty();
    }

    [Fact]
    public void SetArguments_WithQuotedArgumentPreservesSpaces()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        CliResult result = new Auxil.Common.Cli.Cli()
            .Call("cmd")
            .SetArguments("/c", "echo", "hello world")
            .Execute();

        result.ExitCode.ShouldBe(0);
        result.Output.ShouldContain("hello world");
    }

    [Fact]
    public void SetWorkingDirectory_IsApplied()
    {
        CliResult result = new Auxil.Common.Cli.Cli()
            .Call("dotnet")
            .SetArguments("--version")
            .SetWorkingDirectory(Path.GetTempPath())
            .Execute();

        result.ExitCode.ShouldBe(0);
    }

    [Fact]
    public void SetEnvironmentVariable_IsPassedToProcess()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        CliResult result = new Auxil.Common.Cli.Cli()
            .Call("cmd")
            .SetArguments("/c", "echo", "%UPHORIC_TEST_VAR%")
            .SetEnvironmentVariable("UPHORIC_TEST_VAR", "hello")
            .Execute();

        result.ExitCode.ShouldBe(0);
        result.Output.ShouldContain("hello");
    }

    [Fact]
    public void Execute_ThrowsTimeoutExceptionWhenProcessExceedsTimeout()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        Should.Throw<TimeoutException>(() => new Auxil.Common.Cli.Cli()
            .Call("ping")
            .SetArguments("-n", "5", "127.0.0.1")
            .ExecuteAsync(timeout: TimeSpan.FromSeconds(1))
            .GetAwaiter()
            .GetResult());
    }

    [Fact]
    public void ExecuteAsync_ThrowsOperationCanceledExceptionWhenCancelled()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        using CancellationTokenSource cancellation = new();
        cancellation.CancelAfter(TimeSpan.FromMilliseconds(300));

        Should.Throw<OperationCanceledException>(() => new Auxil.Common.Cli.Cli()
            .Call("ping")
            .SetArguments("-n", "5", "127.0.0.1")
            .ExecuteAsync(cancellation.Token, TimeSpan.FromSeconds(10))
            .GetAwaiter()
            .GetResult());
    }

    [Fact]
    public void FluentSetters_ThrowOnNull()
    {
        Auxil.Common.Cli.Cli cli = new();

        Should.Throw<ArgumentNullException>(() => cli.Call(null!));
        Should.Throw<ArgumentNullException>(() => cli.SetArguments((string)null!));
        Should.Throw<ArgumentNullException>(() => cli.SetArguments((string[])null!));
        Should.Throw<ArgumentNullException>(() => cli.SetWorkingDirectory(null!));
        Should.Throw<ArgumentNullException>(() => cli.SetEnvironmentVariable(null!, "value"));
        Should.Throw<ArgumentNullException>(() => cli.SetEnvironmentVariable("name", null!));
    }

    [Fact]
    public void FluentSetters_UpdateProperties()
    {
        Auxil.Common.Cli.Cli cli = new();

        cli.SetCreateNoWindow(false).CreateNoWindow.ShouldBeFalse();
        cli.SetUseShellExecute(true).UseShellExecute.ShouldBeTrue();
        cli.SetRedirectStandardError(false).RedirectStandardError.ShouldBeFalse();
        cli.SetRedirectStandardOutput(false).RedirectStandardOutput.ShouldBeFalse();
        cli.SetWorkingDirectory("C:\\temp").WorkingDirectory.ShouldBe("C:\\temp");
    }
}
