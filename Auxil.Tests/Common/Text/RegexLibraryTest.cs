using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Auxil.Common.Text;

namespace Auxil.Tests.Common.Text;

[TestSubject(typeof(RegexLibrary))]
public class RegexLibraryTest
{
    private static bool Matches(string pattern, string input) => Regex.IsMatch(input, pattern);

    private static bool MatchesFull(string pattern, string input) => Regex.IsMatch(input, $"^(?:{pattern})$");

    [Theory]
    [InlineData("a")]
    [InlineData("Z")]
    [InlineData("é")]
    [InlineData("β")]
    public void AnyLetter_MatchesUnicodeLetters(string input)
    {
        Matches(RegexLibrary.AnyLetter, input).ShouldBeTrue();
    }

    [Theory]
    [InlineData("1")]
    [InlineData(" ")]
    [InlineData("-")]
    public void AnyLetter_DoesNotMatchNonLetters(string input)
    {
        Matches(RegexLibrary.AnyLetter, input).ShouldBeFalse();
    }

    [Theory]
    [InlineData("a", true)]
    [InlineData("é", true)]
    [InlineData("A", false)]
    [InlineData("5", false)]
    public void AnyLowerLetter(string input, bool expected)
    {
        Matches(RegexLibrary.AnyLowerLetter, input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("A", true)]
    [InlineData("É", true)]
    [InlineData("a", false)]
    [InlineData("5", false)]
    public void AnyUpperLetter(string input, bool expected)
    {
        Matches(RegexLibrary.AnyUpperLetter, input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("5", true)]
    [InlineData("-", true)]
    [InlineData("a", false)]
    public void AnyNonLetter(string input, bool expected)
    {
        Matches(RegexLibrary.AnyNonLetter, input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("5", true)]
    [InlineData("a", false)]
    [InlineData(" ", false)]
    public void AnyDigit(string input, bool expected)
    {
        Matches(RegexLibrary.AnyDigit, input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("a", true)]
    [InlineData("5", false)]
    public void AnyNonDigit(string input, bool expected)
    {
        Matches(RegexLibrary.AnyNonDigit, input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("a", true)]
    [InlineData("5", true)]
    [InlineData("_", true)]
    [InlineData(" ", false)]
    [InlineData("-", false)]
    public void AnyWord(string input, bool expected)
    {
        Matches(RegexLibrary.AnyWord, input).ShouldBe(expected);
    }

    [Theory]
    [InlineData(" ", true)]
    [InlineData("\t", true)]
    [InlineData("a", false)]
    public void AnyWhiteSpace(string input, bool expected)
    {
        Matches(RegexLibrary.AnyWhiteSpace, input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("42", true)]
    [InlineData("-7", true)]
    [InlineData("4.2", false)]
    [InlineData("abc", false)]
    public void Integer(string input, bool expected)
    {
        MatchesFull(RegexLibrary.Integer, input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("42", true)]
    [InlineData("-7", false)]
    public void UnsignedInteger(string input, bool expected)
    {
        MatchesFull(RegexLibrary.UnsignedInteger, input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("3.14", true)]
    [InlineData("-2.5", true)]
    [InlineData("42", true)]
    [InlineData("abc", false)]
    public void Float(string input, bool expected)
    {
        MatchesFull(RegexLibrary.Float, input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("#fff", true)]
    [InlineData("#aabbcc", true)]
    [InlineData("#FF0000", true)]
    [InlineData("#ggg", false)]
    [InlineData("#aabb", false)]
    public void HexColor(string input, bool expected)
    {
        MatchesFull(RegexLibrary.HexColor, input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("a", true)]
    [InlineData("F", true)]
    [InlineData("9", true)]
    [InlineData("g", false)]
    public void HexDigit(string input, bool expected)
    {
        Matches(RegexLibrary.HexDigit, input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("foo", true)]
    [InlineData("_bar", true)]
    [InlineData("x1", true)]
    [InlineData("1x", false)]
    [InlineData("a-b", false)]
    public void Identifier(string input, bool expected)
    {
        MatchesFull(RegexLibrary.Identifier, input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("\"hello\"", true)]
    [InlineData("\"a\\\"b\"", true)]
    [InlineData("hello", false)]
    public void WithinDoubleQuotes(string input, bool expected)
    {
        Matches(RegexLibrary.WithinDoubleQuotes, input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("'hello'", true)]
    [InlineData("hello", false)]
    public void WithinSingleQuotes(string input, bool expected)
    {
        Matches(RegexLibrary.WithinSingleQuotes, input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("U.S.A", true)]
    [InlineData("U.S.A.", true)]
    [InlineData("hello", false)]
    public void DottedAbbreviation(string input, bool expected)
    {
        Matches(RegexLibrary.DottedAbbreviation, input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("\n", true)]
    [InlineData("\r", true)]
    [InlineData(" ", false)]
    public void LineBreak(string input, bool expected)
    {
        Matches(RegexLibrary.LineBreak, input).ShouldBe(expected);
    }

    [Theory]
    [InlineData(" ", true)]
    [InlineData("\t", true)]
    [InlineData("\n", false)]
    public void SpaceOrTab(string input, bool expected)
    {
        Matches(RegexLibrary.SpaceOrTab, input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("\t", true)]
    [InlineData(" ", false)]
    public void Tab(string input, bool expected)
    {
        Matches(RegexLibrary.Tab, input).ShouldBe(expected);
    }

    [Theory]
    [InlineData(",", true)]
    [InlineData("!", true)]
    [InlineData("(", true)]
    [InlineData("a", false)]
    [InlineData("5", false)]
    public void Punctuation(string input, bool expected)
    {
        Matches(RegexLibrary.Punctuation, input).ShouldBe(expected);
    }

    [Fact]
    public void Anchors_AnchorPatterns()
    {
        Regex anchored = new(RegexLibrary.StartOfString + "a" + RegexLibrary.EndOfString);
        Regex absoluteAnchored = new(RegexLibrary.AbsoluteEndOfString);

        anchored.IsMatch("a").ShouldBeTrue();
        anchored.IsMatch("xa").ShouldBeFalse();
        absoluteAnchored.IsMatch("x").ShouldBeTrue();
    }

    [Fact]
    public void Boundary_MatchesWordBoundaries()
    {
        Regex regex = new($"{RegexLibrary.Boundary}word{RegexLibrary.Boundary}");

        regex.IsMatch("a word here").ShouldBeTrue();
        regex.IsMatch("sword").ShouldBeFalse();
    }

    [Fact]
    public void Quantifiers_AreSingleCharacters()
    {
        RegexLibrary.OneOrMore.ShouldBe('+');
        RegexLibrary.ZeroOrMore.ShouldBe('*');
        RegexLibrary.ZeroOrOne.ShouldBe('?');
    }
}
