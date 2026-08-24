using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Auxil.Common.Text;

namespace Auxil.Tests.Common.Text;

[TestSubject(typeof(RegexBuilder))]
public class RegexBuilderTest
{
    [Fact]
    public void AnyBetween_MatchesEachAlternate()
    {
        RegexBuilder builder = new RegexBuilder()
            .StartOfString()
            .AnyBetween("cat", "dog")
            .EndOfString();

        builder.IsMatch("cat").ShouldBeTrue();
        builder.IsMatch("dog").ShouldBeTrue();
        builder.IsMatch("bird").ShouldBeFalse();
        builder.IsMatch("cat|dog").ShouldBeFalse();
    }

    [Fact]
    public void AnyBetween_EscapesMetacharactersInAlternates()
    {
        RegexBuilder builder = new RegexBuilder()
            .StartOfString()
            .AnyBetween("a.b", "c*d")
            .EndOfString();

        builder.IsMatch("a.b").ShouldBeTrue();
        builder.IsMatch("axb").ShouldBeFalse();
        builder.IsMatch("cd").ShouldBeFalse();
    }

    [Fact]
    public void GetRegex_CacheRespectsOptionChanges()
    {
        RegexBuilder builder = new RegexBuilder().Text("hello");

        Regex caseSensitive = builder.GetRegex();
        caseSensitive.IsMatch("HELLO").ShouldBeFalse();

        builder.SetRegexOptions(RegexOptions.IgnoreCase);

        Regex caseInsensitive = builder.GetRegex();
        caseInsensitive.IsMatch("HELLO").ShouldBeTrue();
    }

    [Fact]
    public void GetRegex_CacheRespectsPatternChanges()
    {
        RegexBuilder builder = new RegexBuilder().Text("hello");

        Regex first = builder.GetRegex();
        builder.Clear().Text("world");

        Regex second = builder.GetRegex();

        second.IsMatch("world").ShouldBeTrue();
        second.IsMatch("hello").ShouldBeFalse();
        first.ShouldNotBeSameAs(second);
    }

    [Fact]
    public void NonCapture_BuilderOverloadAppendsSingleGroup()
    {
        RegexBuilder inner = new RegexBuilder().AnyDigit().OneOrMore();

        RegexBuilder builder = new RegexBuilder()
            .StartOfString()
            .NonCapture(inner)
            .EndOfString();

        builder.GetRegexPattern().ShouldBe(@"\A(?:\d+)\Z");
        builder.IsMatch("123").ShouldBeTrue();
        builder.IsMatch("abc").ShouldBeFalse();
    }

    [Fact]
    public void NonCapture_TextOverloadEscapesByDefault()
    {
        RegexBuilder builder = new RegexBuilder().NonCapture("a.b");

        builder.GetRegexPattern().ShouldBe(@"(?:a\.b)");
        builder.IsMatch("a.b").ShouldBeTrue();
        builder.IsMatch("axb").ShouldBeFalse();
    }

    [Fact]
    public void NonCapture_TextOverloadCanSkipEscaping()
    {
        RegexBuilder builder = new RegexBuilder().NonCapture(@"\d+", escape: false);

        builder.IsMatch("123").ShouldBeTrue();
    }

    [Fact]
    public void IsMatch_ReturnsTrueForMatchingInput()
    {
        RegexBuilder builder = new RegexBuilder().AnyDigit().Exactly(3);

        builder.IsMatch("123").ShouldBeTrue();
        builder.IsMatch("abc").ShouldBeFalse();
    }

    [Fact]
    public void IsMatch_ThrowsOnNull()
    {
        RegexBuilder builder = new RegexBuilder().AnyDigit();

        Should.Throw<ArgumentNullException>(() => builder.IsMatch(null!));
    }

    [Fact]
    public void ToString_ReturnsPattern()
    {
        RegexBuilder builder = new RegexBuilder().Text("hello");

        builder.ToString().ShouldBe("hello");
        builder.ToString().ShouldBe(builder.GetRegexPattern());
    }

    [Fact]
    public void CopyConstructor_CopiesPatternAndOptions()
    {
        RegexBuilder original = new RegexBuilder()
            .Text("hello")
            .SetRegexOptions(RegexOptions.IgnoreCase);

        RegexBuilder copy = new(original);

        copy.GetRegexPattern().ShouldBe("hello");
        copy.IsMatch("HELLO").ShouldBeTrue();
    }

    [Fact]
    public void CopyConstructor_ThrowsOnNull()
    {
        Should.Throw<ArgumentNullException>(() => new RegexBuilder(null!));
    }

    [Fact]
    public void Between_AppliesRangeQuantifier()
    {
        RegexBuilder builder = new RegexBuilder()
            .StartOfString()
            .Text("a")
            .Between(2, 4)
            .EndOfString();

        builder.IsMatch("aa").ShouldBeTrue();
        builder.IsMatch("aaaa").ShouldBeTrue();
        builder.IsMatch("a").ShouldBeFalse();
        builder.IsMatch("aaaaa").ShouldBeFalse();
    }

    [Fact]
    public void Between_ValidatesArguments()
    {
        RegexBuilder builder = new RegexBuilder();

        Should.Throw<ArgumentOutOfRangeException>(() => builder.Between(3, 2));
        Should.Throw<ArgumentOutOfRangeException>(() => builder.Between(-1, 2));
    }

    [Fact]
    public void Boundary_MatchesWordBoundaries()
    {
        RegexBuilder builder = new RegexBuilder()
            .Boundary()
            .Text("word")
            .Boundary();

        builder.IsMatch("a word here").ShouldBeTrue();
        builder.IsMatch("sword").ShouldBeFalse();
    }

    [Fact]
    public void NonBoundary_MatchesNonWordBoundaries()
    {
        RegexBuilder builder = new RegexBuilder()
            .Text("or")
            .NonBoundary();

        builder.IsMatch("word").ShouldBeTrue();
        builder.IsMatch("or ").ShouldBeFalse();
    }

    [Fact]
    public void Capture_WithBuilder_CreatesNamedGroup()
    {
        RegexBuilder inner = new RegexBuilder().AnyDigit().OneOrMore();

        RegexBuilder builder = new RegexBuilder()
            .StartOfString()
            .Capture("number", inner)
            .EndOfString();

        builder.GetRegexPattern().ShouldBe(@"\A(?<number>\d+)\Z");

        RegexResult result = builder.Match("123");

        result.Success.ShouldBeTrue();
        result.GetString("number").ShouldBe("123");
    }

    [Fact]
    public void Capture_WithText_EscapesByDefault()
    {
        RegexBuilder builder = new RegexBuilder()
            .StartOfString()
            .Capture("value", "a.b")
            .EndOfString();

        builder.GetRegexPattern().ShouldBe(@"\A(?<value>a\.b)\Z");
        builder.IsMatch("a.b").ShouldBeTrue();
        builder.IsMatch("axb").ShouldBeFalse();
    }

    [Fact]
    public void Capture_WithText_CanSkipEscaping()
    {
        RegexBuilder builder = new RegexBuilder()
            .Capture("digits", @"\d+", escape: false);

        RegexResult result = builder.Match("hello 42");

        result.GetString("digits").ShouldBe("42");
    }

    [Fact]
    public void Clear_ResetsBuilder()
    {
        RegexBuilder builder = new RegexBuilder().Text("hello");

        builder.Clear();

        builder.GetRegexPattern().ShouldBe(string.Empty);
    }

    [Fact]
    public void CompleteMatch_AnchorsPattern()
    {
        RegexBuilder builder = new RegexBuilder()
            .CompleteMatch()
            .Text("abc");

        builder.IsMatch("abc").ShouldBeTrue();
        builder.IsMatch("xabc").ShouldBeFalse();
    }

    [Fact]
    public void Count_CountsOccurrences()
    {
        RegexBuilder builder = new RegexBuilder().Text("a");

        builder.Count("banana").ShouldBe(3);
    }

    [Fact]
    public void StartOfString_And_EndOfString_AnchorPattern()
    {
        RegexBuilder builder = new RegexBuilder()
            .StartOfString()
            .Text("abc")
            .EndOfString();

        builder.IsMatch("abc").ShouldBeTrue();
        builder.IsMatch("xabc").ShouldBeFalse();
        builder.IsMatch("abcx").ShouldBeFalse();
    }

    [Fact]
    public void AbsoluteEndOfString_AnchorsAtVeryEnd()
    {
        RegexBuilder builder = new RegexBuilder()
            .Text("abc")
            .AbsoluteEndOfString();

        builder.IsMatch("abc").ShouldBeTrue();
        builder.IsMatch("abc\n").ShouldBeFalse();
    }

    [Fact]
    public void Exactly_AppliesExactQuantifier()
    {
        RegexBuilder builder = new RegexBuilder()
            .StartOfString()
            .Text("a")
            .Exactly(3)
            .EndOfString();

        builder.IsMatch("aaa").ShouldBeTrue();
        builder.IsMatch("aa").ShouldBeFalse();
    }

    [Fact]
    public void ExactlyOrMore_AppliesMinimumQuantifier()
    {
        RegexBuilder builder = new RegexBuilder()
            .StartOfString()
            .Text("a")
            .ExactlyOrMore(2)
            .EndOfString();

        builder.IsMatch("aa").ShouldBeTrue();
        builder.IsMatch("aaaa").ShouldBeTrue();
        builder.IsMatch("a").ShouldBeFalse();
    }

    [Fact]
    public void OneOrMore_AppliesQuantifier()
    {
        RegexBuilder builder = new RegexBuilder()
            .StartOfString()
            .Text("a")
            .OneOrMore()
            .EndOfString();

        builder.IsMatch("a").ShouldBeTrue();
        builder.IsMatch("aaa").ShouldBeTrue();
        builder.IsMatch("").ShouldBeFalse();
    }

    [Fact]
    public void ZeroOrMore_AppliesQuantifier()
    {
        RegexBuilder builder = new RegexBuilder()
            .StartOfString()
            .Text("a")
            .ZeroOrMore()
            .EndOfString();

        builder.IsMatch("").ShouldBeTrue();
        builder.IsMatch("aaa").ShouldBeTrue();
    }

    [Fact]
    public void ZeroOrOne_AppliesQuantifier()
    {
        RegexBuilder builder = new RegexBuilder()
            .StartOfString()
            .Text("a")
            .ZeroOrOne()
            .EndOfString();

        builder.IsMatch("").ShouldBeTrue();
        builder.IsMatch("a").ShouldBeTrue();
        builder.IsMatch("aa").ShouldBeFalse();
    }

    [Fact]
    public void Library_AppendsPatternVerbatim()
    {
        RegexBuilder builder = new RegexBuilder()
            .StartOfString()
            .Library(RegexLibrary.Identifier)
            .EndOfString();

        builder.IsMatch("foo_bar").ShouldBeTrue();
        builder.IsMatch("1foo").ShouldBeFalse();
    }

    [Fact]
    public void Text_WithBuilder_EmbedsPatternWithoutAnchors()
    {
        RegexBuilder inner = new RegexBuilder().StartOfString().Text("abc").EndOfString();

        RegexBuilder builder = new RegexBuilder()
            .Text("x")
            .Text(inner)
            .Text("y");

        builder.GetRegexPattern().ShouldBe("xabcy");
        builder.IsMatch("xabcy").ShouldBeTrue();
    }

    [Fact]
    public void Match_ReturnsFirstMatch()
    {
        RegexBuilder builder = new RegexBuilder().Text("a");

        RegexResult result = builder.Match("banana");

        result.Success.ShouldBeTrue();
        result.GetCompleteMatch().ShouldBe("a");
        result.StartIndex.ShouldBe(1);
    }

    [Fact]
    public void Matches_ReturnsAllMatches()
    {
        RegexBuilder builder = new RegexBuilder().Text("a");

        List<RegexResult> results = builder.Matches("banana");

        results.Count.ShouldBe(3);
    }

    [Fact]
    public void Matches_StripsAnchorsForMultiMatch()
    {
        RegexBuilder builder = new RegexBuilder()
            .StartOfString()
            .Text("a")
            .EndOfString();

        builder.Matches("a a a").Count.ShouldBe(3);
        builder.Match("a a a").Success.ShouldBeFalse();
    }

    [Fact]
    public void Replace_ReplacesOccurrences()
    {
        RegexBuilder builder = new RegexBuilder().Text("a");

        builder.Replace("banana", "o").ShouldBe("bonono");
    }

    [Fact]
    public void SetRegexOptions_AffectsMatching()
    {
        RegexBuilder builder = new RegexBuilder().Text("hello");

        builder.IsMatch("HELLO").ShouldBeFalse();

        builder.SetRegexOptions(RegexOptions.IgnoreCase);

        builder.IsMatch("HELLO").ShouldBeTrue();
    }

    [Fact]
    public void SetRegexTimeout_AppliesTimeoutToRegex()
    {
        RegexBuilder builder = new RegexBuilder().Text("hello");
        TimeSpan timeout = TimeSpan.FromSeconds(5);

        builder.SetRegexTimeout(timeout);

        builder.GetRegex().MatchTimeout.ShouldBe(timeout);
    }

    [Fact]
    public void GetRegex_CacheRespectsTimeoutChanges()
    {
        RegexBuilder builder = new RegexBuilder().Text("hello");

        Regex first = builder.GetRegex();
        builder.SetRegexTimeout(TimeSpan.FromSeconds(3));

        Regex second = builder.GetRegex();

        second.MatchTimeout.ShouldBe(TimeSpan.FromSeconds(3));
        first.ShouldNotBeSameAs(second);
    }

    [Fact]
    public void ThrowsOnNullStringArguments()
    {
        RegexBuilder builder = new RegexBuilder();

        Should.Throw<ArgumentNullException>(() => builder.AnyCharacterBut(null!));
        Should.Throw<ArgumentNullException>(() => builder.AnyOfCharacter(null!));
        Should.Throw<ArgumentNullException>(() => builder.Capture(null!, "x"));
        Should.Throw<ArgumentNullException>(() => builder.Capture("name", null!));
        Should.Throw<ArgumentNullException>(() => builder.Capture("name", (RegexBuilder)null!));
        Should.Throw<ArgumentNullException>(() => builder.NonCapture(null!));
        Should.Throw<ArgumentNullException>(() => builder.NonCapture((RegexBuilder)null!));
        Should.Throw<ArgumentNullException>(() => builder.Text((string)null!));
        Should.Throw<ArgumentNullException>(() => builder.Text((RegexBuilder)null!));
    }
}
