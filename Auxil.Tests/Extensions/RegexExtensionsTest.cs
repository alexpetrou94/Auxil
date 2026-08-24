using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Auxil.Common.Text;
using Auxil.Extensions;

namespace Auxil.Tests.Extensions;

[TestSubject(typeof(RegexExtensions))]
public class RegexExtensionsTest
{
    private static readonly Regex NamedGroupRegex = new(@"(?<word>\w+)");

    [Fact]
    public void BetterMatch_RecordsMatchWithNamedGroups()
    {
        RegexResult result = NamedGroupRegex.BetterMatch("hello world");

        result.Success.ShouldBeTrue();
        result.StartIndex.ShouldBe(0);
        result.EndIndex.ShouldBe(5);
        result.Pattern.ShouldBe(NamedGroupRegex.ToString());
        result.InputText.ShouldBe("hello world");
        result.GetCompleteMatch().ShouldBe("hello");
        result.GetString("word").ShouldBe("hello");
    }

    [Fact]
    public void BetterMatch_NoMatchReturnsFailureResult()
    {
        RegexResult result = NamedGroupRegex.BetterMatch("...");

        result.Success.ShouldBeFalse();
        result.GetCompleteMatch().ShouldBe(string.Empty);
    }

    [Fact]
    public void BetterMatch_UnmatchedOptionalGroupRecordsEmptyString()
    {
        Regex regex = new(@"(?<first>\w+)? (?<second>\w+)");

        RegexResult result = regex.BetterMatch(" only");

        result.GetString("first").ShouldBe(string.Empty);
        result.GetString("second").ShouldBe("only");
    }

    [Fact]
    public void BetterMatches_ReturnsAllMatches()
    {
        Regex regex = new(@"\d+");

        List<RegexResult> results = regex.BetterMatches("a1b22c333");

        results.Count.ShouldBe(3);
        results[0].GetCompleteMatch().ShouldBe("1");
        results[1].GetCompleteMatch().ShouldBe("22");
        results[2].GetCompleteMatch().ShouldBe("333");
        results[2].StartIndex.ShouldBe(6);
        results[2].EndIndex.ShouldBe(9);
    }

    [Fact]
    public void BetterMatches_NoMatchesReturnsEmptyList()
    {
        Regex regex = new(@"\d+");

        regex.BetterMatches("abc").ShouldBeEmpty();
    }

    [Fact]
    public void BetterMatches_RecordsGroupNamesPerMatch()
    {
        Regex regex = new("(?<letter>[a-z])");

        List<RegexResult> results = regex.BetterMatches("ab");

        results[0].GetString("letter").ShouldBe("a");
        results[1].GetString("letter").ShouldBe("b");
    }

    [Fact]
    public void ThrowsOnNullArguments()
    {
        Regex regex = new(@"\d+");
        Regex? nullRegex = null;

        Should.Throw<ArgumentNullException>(() => nullRegex!.BetterMatch("input"));
        Should.Throw<ArgumentNullException>(() => regex.BetterMatch(null!));
        Should.Throw<ArgumentNullException>(() => nullRegex!.BetterMatches("input"));
        Should.Throw<ArgumentNullException>(() => regex.BetterMatches(null!));
    }
}
