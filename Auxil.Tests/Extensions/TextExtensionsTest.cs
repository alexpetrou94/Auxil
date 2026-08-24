using JetBrains.Annotations;
using Auxil.Common.Text;
using Auxil.Extensions;

namespace Auxil.Tests.Extensions;

[TestSubject(typeof(StringExtensions))]
public class TextExtensionsTest
{
    private static IsBetweenSearchOptions Options(string textToFind, char leftDelimiter, char rightDelimiter,
        bool canBeEscaped = false, char escapeChar = '\0')
        => new()
        {
            TextToFind = textToFind,
            LeftDelimiter = leftDelimiter,
            RightDelimiter = rightDelimiter,
            CanBeEscaped = canBeEscaped,
            EscapeChar = escapeChar
        };

    [Theory]
    [InlineData("hello world", 2)]
    [InlineData("hello  world", 2)]
    [InlineData("hello, world!", 2)]
    [InlineData("", 0)]
    [InlineData("   ", 0)]
    [InlineData("hello-world", 1)]
    [InlineData("hello123", 1)]
    public void CountWords(string text, int expected)
    {
        text.CountWords().ShouldBe(expected);
    }

    [Theory]
    [InlineData("hello123", false, 1)]
    [InlineData("0", false, 0)]
    [InlineData("hello 0", false, 1)]
    [InlineData("hello 0", true, 2)]
    [InlineData("hello123", true, 1)]
    public void CountWords_WithCountNumbers(string text, bool countNumbers, int expected)
    {
        TextOptions options = new()
        {
            CountNumbers = countNumbers
        };

        text.CountWords(options).ShouldBe(expected);
    }

    [Fact]
    public void CountWords_CountHyphenatedWordAsSingle()
    {
        TextOptions asSingle = new()
        {
            CountHyphenatedWordAsSingle = true
        };

        TextOptions asSeparate = new()
        {
            CountHyphenatedWordAsSingle = false
        };

        "hello-world".CountWords(asSingle).ShouldBe(1);
        "hello-world".CountWords(asSeparate).ShouldBe(2);
    }

    [Fact]
    public void CountWords_ThrowsOnNull()
    {
        Should.Throw<ArgumentNullException>(() => ((string?)null)!.CountWords());
    }

    [Theory]
    [InlineData("hello", 5)]
    [InlineData("hello world", 11)]
    [InlineData("", 0)]
    public void CountCharacters(string text, int expected)
    {
        text.CountCharacters().ShouldBe(expected);
    }

    [Fact]
    public void CountCharacters_ExcludesEndLineCharactersByDefault()
    {
        "a\nb".CountCharacters().ShouldBe(2);
        "a\nb".CountCharacters(new TextOptions { ExcludeEndLineCharacters = false }).ShouldBe(3);
    }

    [Fact]
    public void CountCharacters_ExcludesCustomCharacters()
    {
        TextOptions options = new()
        {
            CharactersToExclude = ['.', ',']
        };

        "a.b,c".CountCharacters(options).ShouldBe(3);
    }

    [Theory]
    [InlineData("a\n\nb", 2)]
    [InlineData("a\nb", 1)]
    [InlineData("", 0)]
    public void CountParagraphs(string text, int expected)
    {
        text.CountParagraphs().ShouldBe(expected);
    }

    [Theory]
    [InlineData("Hello world. How are you?", 2)]
    [InlineData("Hello world", 1)]
    [InlineData("Wow! Really? Amazing.", 3)]
    [InlineData("", 0)]
    public void CountSentences(string text, int expected)
    {
        text.CountSentences().ShouldBe(expected);
    }

    [Fact]
    public void CountSentences_SkipsSentenceEndFollowedByLowercaseWord()
    {
        "Is this it. yes".CountSentences().ShouldBe(1);
    }

    [Fact]
    public void CountSentences_CountsSentenceEndFollowedByLowercaseWordWhenNotExcluded()
    {
        TextOptions options = new()
        {
            ExcludeLowerCharacterAfterSentenceEnd = false
        };

        "Is this it. yes".CountSentences(options).ShouldBe(2);
    }

    [Fact]
    public void CountUniqueWords_CountsDistinctWords()
    {
        "hello hello world".CountUniqueWords().ShouldBe(2);
    }

    [Fact]
    public void CountUniqueWords_RespectsCaseSensitivity()
    {
        TextOptions caseInsensitive = new()
        {
            CountCaseSensitiveWords = false
        };

        TextOptions caseSensitive = new()
        {
            CountCaseSensitiveWords = true
        };

        "Hello hello HELLO".CountUniqueWords(caseInsensitive).ShouldBe(1);
        "Hello hello HELLO".CountUniqueWords(caseSensitive).ShouldBe(3);
    }

    [Theory]
    [InlineData("hello world", new[] { "hello", "world" })]
    [InlineData("hello, world!", new[] { "hello", "world" })]
    [InlineData("'quoted' text", new[] { "'quoted'", "text" })]
    [InlineData("hyphen-ated", new[] { "hyphen-ated" })]
    public void Words_SplitsAndStripsPunctuation(string text, string[] expected)
    {
        text.Words.ShouldBe(expected);
    }

    [Fact]
    public void GetWordFrequency_CountsOccurrences()
    {
        Dictionary<string, int> frequency = "the cat and the dog".GetWordFrequency();

        frequency.ShouldBe(new Dictionary<string, int>
        {
            ["the"] = 2,
            ["cat"] = 1,
            ["and"] = 1,
            ["dog"] = 1
        });
    }

    [Fact]
    public void GetWordFrequency_IsCaseInsensitiveByDefault()
    {
        Dictionary<string, int> frequency = "The cat the".GetWordFrequency();

        frequency["the"].ShouldBe(2);
    }

    [Fact]
    public void GetWordFrequency_IsCaseSensitiveWithOption()
    {
        TextOptions options = new()
        {
            CountCaseSensitiveWords = true
        };

        Dictionary<string, int> frequency = "The cat the".GetWordFrequency(options);

        frequency["The"].ShouldBe(1);
        frequency["the"].ShouldBe(1);
        frequency["cat"].ShouldBe(1);
    }

    [Theory]
    [InlineData("a\nb", new[] { "a", "b" })]
    [InlineData("a\n\nb", new[] { "a", "b" })]
    [InlineData("a\n", new[] { "a" })]
    [InlineData("a\r\nb", new[] { "a", "b" })]
    [InlineData("", new string[0])]
    public void Lines_RemovesEmptyLines(string text, string[] expected)
    {
        text.Lines.ShouldBe(expected);
    }

    [Theory]
    [InlineData("hello", 4)]
    [InlineData("", -1)]
    public void LastIndex(string text, int expected)
    {
        text.LastIndex.ShouldBe(expected);
    }

    [Theory]
    [InlineData("(abc)", "abc", '(', ')')]
    [InlineData("x(abc)y", "abc", '(', ')')]
    [InlineData("[hello world]", "hello world", '[', ']')]
    [InlineData("x(abc) (def)y", "def", '(', ')')]
    public void IsBetween_ReturnsTrue(string overallText, string textToFind, char leftDelimiter, char rightDelimiter)
    {
        overallText.IsBetween(textToFind, leftDelimiter, rightDelimiter).ShouldBeTrue();
    }

    [Theory]
    [InlineData("(abc", "abc", '(', ')')]
    [InlineData("abc)", "abc", '(', ')')]
    [InlineData("abc", "abc", '(', ')')]
    [InlineData("x(abc)", "def", '(', ')')]
    [InlineData("x(abc)", "abc", ')', '(')]
    public void IsBetween_ReturnsFalse(string overallText, string textToFind, char leftDelimiter, char rightDelimiter)
    {
        overallText.IsBetween(textToFind, leftDelimiter, rightDelimiter).ShouldBeFalse();
    }

    [Fact]
    public void IsBetween_EscapedLeftDelimiterNotCounted()
    {
        ("\\" + "(abc)").IsBetween("abc", '(', ')', canBeEscaped: true, escapeChar: '\\').ShouldBeFalse();
    }

    [Fact]
    public void IsBetween_EscapedRightDelimiterNotCounted()
    {
        "(abc\\)".IsBetween("abc", '(', ')', canBeEscaped: true, escapeChar: '\\').ShouldBeFalse();
    }

    [Fact]
    public void IsBetween_EscapeCharIgnoredWhenEscapingDisabled()
    {
        ("\\" + "(abc)").IsBetween("abc", '(', ')').ShouldBeTrue();
        "(abc\\)".IsBetween("abc", '(', ')').ShouldBeTrue();
    }

    [Fact]
    public void IsBetween_FindsMatchAfterStartIndex()
    {
        IsBetweenSearchOptions options = new()
        {
            TextToFind = "def",
            LeftDelimiter = '(',
            RightDelimiter = ')',
            StartIndex = 5
        };

        "(abc) (def)".IsBetween("def", '(', ')', startIndex: 5).ShouldBeTrue();
        "(abc) (def)".IsBetween("abc", '(', ')', startIndex: 5).ShouldBeFalse();
        "(abc) (def)".IsBetween(options).ShouldBeTrue();
    }

    [Fact]
    public void IsBetween_WithOptionsBag()
    {
        IsBetweenSearchOptions options = Options("abc", '(', ')');

        "(abc)".IsBetween(options).ShouldBeTrue();
    }

    [Fact]
    public void IsBetween_ReturnsFalseForEmptyTextToFind()
    {
        "(abc)".IsBetween("", '(', ')').ShouldBeFalse();
    }

    [Fact]
    public void IsBetween_ThrowsOnNullTextToFind()
    {
        Should.Throw<ArgumentNullException>(() => ((string?)null)!.IsBetween("abc", '(', ')'));
    }
}
