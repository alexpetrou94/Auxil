using JetBrains.Annotations;
using Auxil.Common.Cli;

namespace Auxil.Tests.Common.Cli;

[TestSubject(typeof(CliResult))]
public class CliResultTest
{
    [Fact]
    public void RecordSemantics_EqualResultsAreEqual()
    {
        CliResult a = new()
        {
            ExitCode = 0,
            Output = "out",
            ErrorOutput = "err",
            StartTime = new DateTime(2024, 1, 1),
            ExitTime = new DateTime(2024, 1, 1, 0, 0, 1)
        };

        CliResult b = new()
        {
            ExitCode = 0,
            Output = "out",
            ErrorOutput = "err",
            StartTime = new DateTime(2024, 1, 1),
            ExitTime = new DateTime(2024, 1, 1, 0, 0, 1)
        };

        a.ShouldBe(b);
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void Success_IsTrueForZeroExitCode()
    {
        CliResult result = new() { ExitCode = 0 };

        result.Success.ShouldBeTrue();
    }

    [Fact]
    public void Success_IsFalseForNonZeroExitCode()
    {
        CliResult result = new() { ExitCode = 7 };

        result.Success.ShouldBeFalse();
    }

    [Fact]
    public void Duration_SpansStartAndExitTime()
    {
        CliResult result = new()
        {
            StartTime = new DateTime(2024, 1, 1, 0, 0, 0),
            ExitTime = new DateTime(2024, 1, 1, 0, 0, 5)
        };

        result.Duration.ShouldBe(TimeSpan.FromSeconds(5));
    }
}
