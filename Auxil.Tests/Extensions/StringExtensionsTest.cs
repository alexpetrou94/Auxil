using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Auxil.Common;
using Auxil.Common.Text;
using Auxil.Extensions;

namespace Auxil.Tests.Extensions;

[TestSubject(typeof(StringExtensions))]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
public class StringExtensionsTest
{
    public static TheoryData<string, string, bool, List<int>> GetAllIndexesOfData => new()
    {
        { "HeLLoWorld", "L", true, [2, 3, 8] },
        { "HeLLoWorld", "L", false, [2, 3] },
        { "HeLLoWorld", "l", false, [8] },
        { "", "a", false, [] },
        { "%%Hello%World%%", "%%", false, [0, 13] },
        { "%%Hello%World%%", "%%", true, [0, 13] }
    };

    public static TheoryData<string, char[], ContextualStringSplitOptions, string[]> GetContextualSplitData => new()
    {
        {
            "A normal split.",
            [' '],
            new ContextualStringSplitOptions(),
            ["A", "normal", "split."]
        },
        {
            "A normal split, with-dash.",
            [' ', '-'],
            new ContextualStringSplitOptions(),
            ["A", "normal", "split,", "with", "dash."]
        },
        {
            "A  normal split.",
            [' '],
            new ContextualStringSplitOptions(),
            ["A", "", "normal", "split."]
        },
        {
            "A    normal    split.",
            [' '],
            new ContextualStringSplitOptions { RemoveEmptyEntries = true },
            ["A", "normal", "split."]
        },
        {
            "Split with numbers12345",
            [' '],
            new ContextualStringSplitOptions { RemoveEmptyEntries = true, IncludeNumbers = true },
            ["Split", "with", "numbers", "12345"]
        },
        {
            "Split with acronymsABCT",
            [' '],
            new ContextualStringSplitOptions { RemoveEmptyEntries = true, IncludeAcronyms = true },
            ["Split", "with", "acronyms", "ABCT"]
        },
        {
            "Split with both acronymsABCT123 and numbers123ABC",
            [' '],
            new ContextualStringSplitOptions { RemoveEmptyEntries = true, IncludeAcronyms = true, IncludeNumbers = true },
            ["Split", "with", "both", "acronyms", "ABCT", "123", "and", "numbers", "123", "ABC"]
        },
        {
            "Split with acronymsABCTand",
            [' '],
            new ContextualStringSplitOptions { RemoveEmptyEntries = true, IncludeAcronyms = true },
            ["Split", "with", "acronyms", "ABCT", "and"]
        },
        {
            "Split with ABC123DEF",
            [' '],
            new ContextualStringSplitOptions { IncludeAcronyms = true },
            ["Split", "with", "ABC123DEF"]
        }
    };

    [Theory]
    [MemberData(nameof(GetAllIndexesOfData), MemberType = typeof(StringExtensionsTest))]
    public void AllIndexesOf(string text, string subString, bool ignoreCase, IEnumerable<int> expected)
    {
        text.AllIndexesOf(subString, ignoreCase).ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public void ContextualSplit_WithoutOptions()
    {
        "A normal split.".ContextualSplit([' ']).ShouldBe([ "A", "normal", "split." ]);
    }

    [Fact]
    public void ContextualSplit_TrimsEntries()
    {
        "  A normal  split.  ".ContextualSplit([' '], new ContextualStringSplitOptions
        {
            RemoveEmptyEntries = true,
            TrimEntries = true
        }).ShouldBe([ "A", "normal", "split." ]);
    }

    [Fact]
    public void ContextualSplit_DefaultsSeparatorsToSpaceWhenEmpty()
    {
        "A B CD".ContextualSplit([], new ContextualStringSplitOptions
        {
            IncludeAcronyms = true
        }).ShouldBe([ "A", "B", "CD" ]);
    }

    [Theory]
    [MemberData(nameof(GetContextualSplitData), MemberType = typeof(StringExtensionsTest))]
    public void ContextualSplit(string text, char[] separators, ContextualStringSplitOptions options, string[] expected)
    {
        text.ContextualSplit(separators, options).ShouldBe(expected);
    }

    [Theory]
    [InlineData("hello", "Hello")]
    [InlineData("5times", "5times")]
    [InlineData("test this", "Test this")]
    [InlineData("", "")]
    [InlineData("t", "T")]
    [InlineData("éclair", "Éclair")]
    public void Capitalize(string text, string expected)
    {
        text.Capitalize().ShouldBe(expected);
    }

    [Fact]
    public void Capitalize_IsCultureInvariant()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

            "istanbul".Capitalize().ShouldBe("Istanbul");
            "hello".Capitalize().ShouldBe("Hello");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Theory]
    [InlineData("HELLO", true)]
    [InlineData("Hello", false)]
    [InlineData("HELLO1", true)]
    [InlineData("HELLO WORLD", true)]
    [InlineData("123", true)]
    [InlineData("ÉCLAIR", true)]
    [InlineData("", true)]
    public void IsAllUpper(string text, bool expected)
    {
        text.IsAllUpper.ShouldBe(expected);
    }

    [Theory]
    [InlineData("hello", true)]
    [InlineData("Hello", false)]
    [InlineData("hello1", true)]
    [InlineData("hello world", true)]
    [InlineData("123", true)]
    [InlineData("éclair", true)]
    [InlineData("", true)]
    public void IsAllLower(string text, bool expected)
    {
        text.IsAllLower.ShouldBe(expected);
    }

    [Fact]
    public void IsAllUpperLower_AreCultureInvariant()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

            "HELLO".IsAllUpper.ShouldBeTrue();
            "istanbul".IsAllUpper.ShouldBeFalse();
            "istanbul".IsAllLower.ShouldBeTrue();
            "HELLO".IsAllLower.ShouldBeFalse();
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Theory]
    [InlineData("Alpha", VersionDevelopmentStage.Alpha)]
    [InlineData("alpha", VersionDevelopmentStage.Alpha)]
    [InlineData("ALPHA", VersionDevelopmentStage.Alpha)]
    [InlineData("  Beta  ", VersionDevelopmentStage.Beta)]
    [InlineData("", VersionDevelopmentStage.None)]
    [InlineData("Invalid", VersionDevelopmentStage.None)]
    public void ToEnum_ParsesByName(string text, VersionDevelopmentStage expected)
    {
        text.ToEnum<VersionDevelopmentStage>().ShouldBe(expected);
    }

    [Theory]
    [InlineData("ALPHA", true, VersionDevelopmentStage.Alpha)]
    [InlineData("ALPHA", false, VersionDevelopmentStage.None)]
    public void ToEnum_RespectsIgnoreCase(string text, bool ignoreCase, VersionDevelopmentStage expected)
    {
        text.ToEnum<VersionDevelopmentStage>(ignoreCase: ignoreCase).ShouldBe(expected);
    }

    [Fact]
    public void ToEnum_UsesDefaultWhenParseFails()
    {
        "not-an-enum".ToEnum(VersionDevelopmentStage.Beta).ShouldBe(VersionDevelopmentStage.Beta);
    }

    [Fact]
    public void ToEnum_ThrowsOnNull()
    {
        string? text = null;

        Should.Throw<ArgumentNullException>(() => text!.ToEnum<VersionDevelopmentStage>());
    }

    [Theory]
    [InlineData("hello\nworld", 1)]
    [InlineData("a\nb\nc", 2)]
    [InlineData("\n\n\n", 3)]
    [InlineData("\r\n", 1)]
    [InlineData("no newlines here", 0)]
    [InlineData("", 0)]
    public void EndLineCount(string text, int expected)
    {
        text.EndLineCount().ShouldBe(expected);
    }

    [Fact]
    public void EndLineCount_ThrowsOnNull()
    {
        string? text = null;

        Should.Throw<ArgumentNullException>(() => text!.EndLineCount());
    }

    [Theory]
    [InlineData("hello", "hello")]
    [InlineData("50times", "50_times")]
    [InlineData("test this", "test_this")]
    [InlineData("HelloWorldABC", "helloworld_abc")]
    [InlineData("Hello-WorldABC123", "hello_world_abc_123")]
    [InlineData("", "")]
    public void ToSnakeCase(string text, string expected)
    {
        text.ToSnakeCase().ShouldBe(expected);
    }

    [Theory]
    [InlineData("hello", "Hello")]
    [InlineData("50times", "50Times")]
    [InlineData("test this", "TestThis")]
    [InlineData("helloWorldABC", "HelloworldAbc")]
    [InlineData("hello-worldABC123", "HelloWorldAbc123")]
    [InlineData("", "")]
    public void ToPascalCase(string text, string expected)
    {
        text.ToPascalCase().ShouldBe(expected);
    }

    [Theory]
    [InlineData("hello", "hello")]
    [InlineData("50times", "50Times")]
    [InlineData("test this", "testThis")]
    [InlineData("helloWorldABC", "helloworldAbc")]
    [InlineData("hello-worldABC123", "helloWorldAbc123")]
    [InlineData("", "")]
    public void ToCamelCase(string text, string expected)
    {
        text.ToCamelCase().ShouldBe(expected);
    }

    [Theory]
    [InlineData("hello\n", NewLineCharacter.LineFeed)]
    [InlineData("hello\r\n", NewLineCharacter.CarriageReturnLineFeed)]
    [InlineData("hello\r", NewLineCharacter.CarriageReturn)]
    [InlineData("hello\r\n\n", NewLineCharacter.Mixed)]
    [InlineData("hello\n\r", NewLineCharacter.Mixed)]
    [InlineData("hello\r\r\n", NewLineCharacter.Mixed)]
    [InlineData("hello", NewLineCharacter.None)]
    [InlineData("\n\n", NewLineCharacter.LineFeed)]
    [InlineData("\r\n\r\n", NewLineCharacter.CarriageReturnLineFeed)]
    [InlineData("\r\r", NewLineCharacter.CarriageReturn)]
    [InlineData("\n\r\n", NewLineCharacter.Mixed)]
    public void GetNewLineType(string text, NewLineCharacter expected)
    {
        NewLineCharacter result = text.GetNewLineType();

        result.ShouldBe(expected);
    }

    [Fact]
    public void GetNewLineType_ThrowsOnNull()
    {
        string? text = null;

        Should.Throw<ArgumentNullException>(() => text!.GetNewLineType());
    }

    [Theory]
    [InlineData("hello", 2, true, "llo")]
    [InlineData("hello", 1, true, "ello")]
    [InlineData("hello", 0, true, "hello")]
    [InlineData("hello", 5, true, "")]
    [InlineData("hello", 10, true, "")]
    [InlineData("", 1, true, "")]
    [InlineData("hello\r\nworld", 6, true, "world")]
    [InlineData("hello\r\nworld", 6, false, "\nworld")]
    [InlineData("hello\nworld", 6, true, "world")]
    [InlineData("a\r\nb", 1, true, "\r\nb")]
    [InlineData("a\r\nb", 2, true, "b")]
    public void RemoveLeft(string text, int count, bool treatNewLineAsSingle, string expected)
    {
        text.RemoveLeft(count, treatNewLineAsSingle).ShouldBe(expected);
    }

    [Fact]
    public void RemoveLeft_ThrowsOnNull()
    {
        string? text = null;

        Should.Throw<ArgumentNullException>(() => text!.RemoveLeft());
    }

    [Theory]
    [InlineData("hello", 2, true, "hel")]
    [InlineData("hello", 1, true, "hell")]
    [InlineData("hello", 0, true, "hello")]
    [InlineData("hello", 5, true, "")]
    [InlineData("hello", 10, true, "")]
    [InlineData("", 1, true, "")]
    [InlineData("hello\r\nworld", 6, true, "hello")]
    [InlineData("hello\r\nworld", 6, false, "hello\r")]
    [InlineData("hello\r\nworld", 5, true, "hello\r\n")]
    [InlineData("a\r\nb", 2, true, "a")]
    [InlineData("a\r\nb", 1, true, "a\r\n")]
    public void RemoveRight(string text, int count, bool treatNewLineAsSingle, string expected)
    {
        text.RemoveRight(count, treatNewLineAsSingle).ShouldBe(expected);
    }

    [Fact]
    public void RemoveRight_ThrowsOnNull()
    {
        string? text = null;

        Should.Throw<ArgumentNullException>(() => text!.RemoveRight());
    }

    [Theory]
    [InlineData("AGGTAB", "GXTXAYB", "GTAB")]
    [InlineData("XMJYAUZ", "MZJAWXU", "MJAU")]
    [InlineData("hello", "hallo", "hllo")]
    [InlineData("abc", "abc", "abc")]
    [InlineData("abc", "ac", "ac")]
    [InlineData("abc", "xyz", "")]
    [InlineData("", "abc", "")]
    [InlineData("abc", "", "")]
    public void LongestCommonSubsequence(string text, string otherText, string expected)
    {
        text.LongestCommonSubsequence(otherText).ShouldBe(expected);
    }

    [Fact]
    public void LongestCommonSubsequence_ThrowsOnNull()
    {
        Should.Throw<ArgumentNullException>(() => ((string?)null)!.LongestCommonSubsequence("abc"));
        Should.Throw<ArgumentNullException>(() => "abc".LongestCommonSubsequence(null!));
    }

    [Theory]
    [InlineData("path\\to\\file.txt", "path/to/file.txt")]
    [InlineData("no-backslashes", "no-backslashes")]
    [InlineData("", "")]
    public void ToForwardSlash(string text, string expected)
    {
        text.ToForwardSlash().ShouldBe(expected);
    }

    [Fact]
    public void ToForwardSlash_ThrowsOnNull()
    {
        Should.Throw<ArgumentNullException>(() => ((string?)null)!.ToForwardSlash());
    }

    [Theory]
    [InlineData("hello", 'l', "helo")]
    [InlineData("aa", 'a', "a")]
    public void RemoveConsecutiveDuplicates(string text, char character, string expected)
    {
        text.RemoveConsecutiveDuplicates(character).ShouldBe(expected);
    }

    [Theory]
    [InlineData("aa..bb!!", new[] { 'a', '.', '!' }, "a.bb!")]
    public void RemoveConsecutiveDuplicates_WithCharacters(string text, char[] characters, string expected)
    {
        text.RemoveConsecutiveDuplicates(characters).ShouldBe(expected);
    }

    [Fact]
    public void RemoveConsecutiveDuplicates_ThrowsOnNull()
    {
        Should.Throw<ArgumentNullException>(() => ((string?)null)!.RemoveConsecutiveDuplicates('a'));
        Should.Throw<ArgumentNullException>(() => "abc".RemoveConsecutiveDuplicates(null!));
    }

    [Theory]
    [InlineData("42", 42)]
    [InlineData("-7", -7)]
    [InlineData("0", 0)]
    public void Parse_Integer(string text, int expected)
    {
        text.Parse<int>().ShouldBe(expected);
    }

    [Fact]
    public void Parse_Float()
    {
        "3.5".Parse<float>().ShouldBe(3.5f);
    }

    [Fact]
    public void Parse_Boolean()
    {
        "true".Parse<bool>().ShouldBeTrue();
        "false".Parse<bool>().ShouldBeFalse();
    }

    [Fact]
    public void Parse_DateOnly()
    {
        "2024-01-15".Parse<DateOnly>().ShouldBe(new DateOnly(2024, 1, 15));
    }

    [Fact]
    public void Parse_ThrowsFormatException()
    {
        Should.Throw<FormatException>(() => "not-a-number".Parse<int>());
    }

    [Fact]
    public void Parse_ThrowsOverflowException()
    {
        Should.Throw<OverflowException>(() => "99999999999999999999".Parse<int>());
    }

    [Fact]
    public void Parse_ThrowsOnNull()
    {
        Should.Throw<ArgumentNullException>(() => ((string?)null)!.Parse<int>());
    }

    [Fact]
    public void TryParse_ReturnsTrueWithValue()
    {
        "42".TryParse(out int value).ShouldBeTrue();
        value.ShouldBe(42);
    }

    [Fact]
    public void TryParse_ReturnsFalseOnInvalidInput()
    {
        "not-a-number".TryParse(out int value).ShouldBeFalse();
        value.ShouldBe(0);
    }

    [Fact]
    public void TryParse_ReturnsFalseOnNull()
    {
        ((string?)null)!.TryParse(out int value).ShouldBeFalse();
        value.ShouldBe(0);
    }

    [Theory]
    [InlineData("hello", "ll", "heo")]
    public void Remove_String(string text, string textToRemove, string expected)
    {
        text.Remove(textToRemove).ShouldBe(expected);
    }

    [Theory]
    [InlineData("abcabc", new[] { 'a', 'c' }, "bb")]
    public void Remove_Chars(string text, char[] charsToRemove, string expected)
    {
        text.Remove(charsToRemove).ShouldBe(expected);
    }

    [Theory]
    [InlineData("hello", 2, "he")]
    [InlineData("hello", 0, "")]
    [InlineData("hello", 10, "hello")]
    public void Left(string text, int count, string expected)
    {
        text.Left(count).ShouldBe(expected);
    }

    [Theory]
    [InlineData("hello", 2, "lo")]
    [InlineData("hello", 0, "")]
    [InlineData("hello", 10, "hello")]
    public void Right(string text, int count, string expected)
    {
        text.Right(count).ShouldBe(expected);
    }

    [Theory]
    [InlineData("abc123", true)]
    [InlineData("abc 123", false)]
    [InlineData("", true)]
    public void IsAlphanumeric(string text, bool expected)
    {
        text.IsAlphanumeric().ShouldBe(expected);
    }

    [Theory]
    [InlineData("hello world", 6, 6)]
    [InlineData("hello world", 3, 0)]
    public void GetStartBoundary(string text, int startIndex, int expected)
    {
        text.GetStartBoundary(startIndex).ShouldBe(expected);
    }

    [Theory]
    [InlineData("hello world", 6, 10)]
    [InlineData("hello", 0, 4)]
    public void GetEndBoundary(string text, int startIndex, int expected)
    {
        text.GetEndBoundary(startIndex).ShouldBe(expected);
    }

    [Theory]
    [InlineData("hello world", -1)]
    [InlineData("hello world", 11)]
    [InlineData("hello world", 5)]
    public void GetEndBoundary_InvalidStartReturnsMinusOne(string text, int startIndex)
    {
        text.GetEndBoundary(startIndex).ShouldBe(-1);
    }

    [Fact]
    public void GetEndBoundary_InvalidBoundaryCharacterAtStart()
    {
        "hello".GetEndBoundary(0, ['h']).ShouldBe(-1);
    }

    [Theory]
    [InlineData("hello world", -1)]
    [InlineData("hello world", 11)]
    [InlineData("hello world", 5)]
    public void GetStartBoundary_InvalidStartReturnsMinusOne(string text, int startIndex)
    {
        text.GetStartBoundary(startIndex).ShouldBe(-1);
    }

    [Fact]
    public void GetStartBoundary_InvalidBoundaryCharacterAtStart()
    {
        "hello".GetStartBoundary(0, ['h']).ShouldBe(-1);
    }

    [Theory]
    [InlineData("{hello}", "hello")]
    [InlineData("no braces", "")]
    public void GetTextInCurlyBraces(string text, string expected)
    {
        text.GetTextInCurlyBraces().ShouldBe(expected);
    }

    [Theory]
    [InlineData("\"hello\"", "hello")]
    [InlineData("no quotes", "")]
    public void GetTextInQuotes(string text, string expected)
    {
        text.GetTextInQuotes().ShouldBe(expected);
    }

    [Theory]
    [InlineData("Hello, world!", "Hello world")]
    public void RemoveAllPunctuation(string text, string expected)
    {
        text.RemoveAllPunctuation().ShouldBe(expected);
    }

    [Theory]
    [InlineData("café", "cafe")]
    [InlineData("plain", "plain")]
    public void RemoveDiacritics(string text, string expected)
    {
        text.RemoveDiacritics().ShouldBe(expected);
    }

    [Theory]
    [InlineData("ab", 3, "ababab")]
    [InlineData("ab", 0, "")]
    public void Repeat(string text, int count, string expected)
    {
        text.Repeat(count).ShouldBe(expected);
    }

    [Fact]
    public void Suffix_AppendsValue()
    {
        "hello".Suffix("!").ShouldBe("hello!");
    }

    [Fact]
    public void Prefix_PrependsValue()
    {
        "hello".Prefix(">").ShouldBe(">hello");
    }

    [Theory]
    [InlineData("hello", "\"", "\"hello\"")]
    [InlineData("hello", "[", "[hello[")]
    public void SurroundWith_UsesSameDelimiterBothSides(string text, string delimiter, string expected)
    {
        text.SurroundWith(delimiter).ShouldBe(expected);
    }

    [Theory]
    [InlineData("hello", "[", "]", "[hello]")]
    public void SurroundWith_WithEndDelimiter(string text, string startDelimiter, string endDelimiter, string expected)
    {
        text.SurroundWith(startDelimiter, endDelimiter).ShouldBe(expected);
    }

    [Theory]
    [InlineData("hello world", "hello ", "world")]
    public void RemovePrefix(string text, string prefix, string expected)
    {
        text.RemovePrefix(prefix).ShouldBe(expected);
    }

    [Theory]
    [InlineData("hello world", "world", "hello ")]
    public void RemoveSuffix(string text, string suffix, string expected)
    {
        text.RemoveSuffix(suffix).ShouldBe(expected);
    }

    [Fact]
    public void GetLines_WithOptions()
    {
        "a\n\nb".GetLines(removeEmptyLines: false).ShouldBe([ "a", "", "b" ]);
        "a\n\nb".GetLines().ShouldBe([ "a", "b" ]);
    }

    [Fact]
    public void RegexEscape_EscapesMetacharacters()
    {
        ".".RegexEscape().ShouldBe(@"\.");
    }

    [Fact]
    public void RegexReplace_ReplacesMatches()
    {
        "hello world".RegexReplace("world", "there").ShouldBe("hello there");
    }

    [Fact]
    public void RegexReplace_WithOptions()
    {
        "AbC".RegexReplace("b", "x", RegexOptions.IgnoreCase).ShouldBe("AxC");
    }

    }
