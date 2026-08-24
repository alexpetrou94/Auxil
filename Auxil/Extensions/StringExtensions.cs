using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Auxil.Common.Text;
using Auxil.Utilities;

namespace Auxil.Extensions;

/// <summary>Provides a rich set of string manipulation helpers, including case conversions, text analysis, and splitting.</summary>
[PublicAPI]
public static class StringExtensions
{
    private const string BracesRegexPattern = @"\{([^}]+)\}";
    private const string QuotesRegexPattern = "\"([^\"]*)\"|'([^']*)'";
    private static readonly char[] CommonSeparators = [' ', '-', '_'];
    private static readonly List<char> Punctuations =
    [
        '.', ',', ':', ';', '?', '!', '"', '\'', '(', ')', '[', ']', '{', '}',
        '-', '_', '/', '\\', '|', '+', '=', '*', '&', '%', '$', '#', '@', '^', '~'
    ];

    /// <summary>Returns all zero-based indexes where the substring occurs in the text.</summary>
    /// <param name="text">The text to search.</param>
    /// <param name="subString">The substring to find.</param>
    /// <param name="ignoreCase">Whether to match case-insensitively.</param>
    public static List<int> AllIndexesOf(this string text, string subString, bool ignoreCase = false)
    {
        List<int> result = [];

        if (text.IsNullOrEmpty())
        {
            return result;
        }

        int index = 0;

        StringComparison stringComparison = ignoreCase
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        while ((index = text.IndexOf(subString, index, stringComparison)) != -1)
        {
            result.Add(index);
            index++;
        }

        return result;
    }

    /// <summary>Returns the text with the first character uppercased using the invariant culture.</summary>
    /// <param name="text">The text to capitalize.</param>
    public static string Capitalize(this string text)
        => text.IsNullOrEmpty() ? string.Empty : char.ToUpperInvariant(text[0]) + text[1..];

    /// <summary>Returns the text with the value appended.</summary>
    /// <param name="text">The base text.</param>
    /// <param name="value">The value to append.</param>
    public static string Suffix(this string text, string value) => $"{text}{value}";

    /// <summary>Returns the value with the text appended.</summary>
    /// <param name="text">The base text.</param>
    /// <param name="value">The value to prepend.</param>
    public static string Prefix(this string text, string value) => $"{value}{text}";

    /// <summary>Returns the index just before the first boundary character at or after the start index.</summary>
    /// <param name="text">The text to scan.</param>
    /// <param name="startIndex">The index to start scanning at.</param>
    /// <param name="boundaryCharacters">Additional characters that end a word.</param>
    /// <returns>The end boundary index, or <c>-1</c> when the start index is invalid.</returns>
    public static int GetEndBoundary(this string text, int startIndex, HashSet<char> boundaryCharacters)
    {
        bool isInvalid = startIndex >= text.Length
            || startIndex < 0
            || char.IsWhiteSpace(text[startIndex])
            || boundaryCharacters.Contains(text[startIndex]);

        if (isInvalid)
        {
            return -1;
        }

        for (int i = startIndex; i < text.Length; i++)
        {
            char character = text[i];

            if (char.IsWhiteSpace(character) || boundaryCharacters.Contains(character))
            {
                return i - 1;
            }
        }

        return text.Length - 1;
    }

    /// <summary>Returns the index just before the first white space at or after the start index.</summary>
    /// <param name="text">The text to scan.</param>
    /// <param name="startIndex">The index to start scanning at.</param>
    public static int GetEndBoundary(this string text, int startIndex)
        => text.GetEndBoundary(startIndex, []);

    /// <summary>Detects the newline style used in the text.</summary>
    /// <param name="text">The text to inspect.</param>
    public static NewLineCharacter GetNewLineType(this string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        ReadOnlySpan<char> span = text.AsSpan();
        bool hasLineFeed = false;
        bool hasCarriageReturn = false;
        bool hasCarriageReturnLineFeed = false;

        for (int i = 0; i < span.Length; i++)
        {
            switch (span[i])
            {
                case '\n':
                    hasLineFeed = true;
                    break;

                case '\r' when i + 1 < span.Length && span[i + 1] == '\n':
                    hasCarriageReturnLineFeed = true;
                    i++;
                    break;

                case '\r':
                    hasCarriageReturn = true;
                    break;
            }
        }

        int distinctTypes = (hasLineFeed ? 1 : 0) + (hasCarriageReturn ? 1 : 0) + (hasCarriageReturnLineFeed ? 1 : 0);

        if (distinctTypes > 1)
        {
            return NewLineCharacter.Mixed;
        }

        if (hasCarriageReturnLineFeed)
        {
            return NewLineCharacter.CarriageReturnLineFeed;
        }

        if (hasLineFeed)
        {
            return NewLineCharacter.LineFeed;
        }

        if (hasCarriageReturn)
        {
            return NewLineCharacter.CarriageReturn;
        }

        return NewLineCharacter.None;
    }

    /// <summary>Removes all occurrences of the given characters.</summary>
    /// <param name="text">The text to modify.</param>
    /// <param name="charsToRemove">The characters to remove.</param>
    public static string Remove(this string text, char[] charsToRemove)
    {
        StringBuilder sb = new(text);

        foreach (char c in charsToRemove)
        {
            sb.Replace(c.ToString(), string.Empty);
        }

        return sb.ToString();
    }

    /// <summary>Removes all occurrences of the given substring.</summary>
    /// <param name="text">The text to modify.</param>
    /// <param name="textToRemove">The substring to remove.</param>
    public static string Remove(this string text, string textToRemove)
        => text.Replace(textToRemove, string.Empty);

    /// <summary>Returns the index just after the last boundary character at or before the start index.</summary>
    /// <param name="text">The text to scan.</param>
    /// <param name="startIndex">The index to start scanning at.</param>
    /// <param name="boundaryCharacters">Additional characters that end a word.</param>
    /// <returns>The start boundary index, or <c>-1</c> when the start index is invalid.</returns>
    public static int GetStartBoundary(this string text, int startIndex, HashSet<char> boundaryCharacters)
    {
        bool isInvalid = startIndex >= text.Length
            || startIndex < 0
            || char.IsWhiteSpace(text[startIndex])
            || boundaryCharacters.Contains(text[startIndex]);

        if (isInvalid)
        {
            return -1;
        }

        for (int i = startIndex; i >= 0; i--)
        {
            char character = text[i];

            if (char.IsWhiteSpace(character) || boundaryCharacters.Contains(character))
            {
                return i + 1;
            }
        }

        return 0;
    }

    /// <summary>Returns the index just after the last white space at or before the start index.</summary>
    /// <param name="text">The text to scan.</param>
    /// <param name="startIndex">The index to start scanning at.</param>
    public static int GetStartBoundary(this string text, int startIndex)
        => text.GetStartBoundary(startIndex, []);

    /// <summary>Returns the text inside the first pair of curly braces.</summary>
    /// <param name="input">The text to search.</param>
    public static string GetTextInCurlyBraces(this string input)
        => Regex.Match(input, BracesRegexPattern).Groups[1].Value;

    /// <summary>Returns the text inside the first quoted segment, double or single quotes.</summary>
    /// <param name="input">The text to search.</param>
    public static string GetTextInQuotes(this string input)
        => Regex.Match(input, QuotesRegexPattern)
            .Groups.Cast<Group>()
            .Skip(1)
            .FirstOrDefault(g => g.Success)?.Value ?? string.Empty;

    /// <summary>Whether the text consists only of letters and digits.</summary>
    /// <param name="text">The text to check.</param>
    public static bool IsAlphanumeric(this string text) => text.All(char.IsLetterOrDigit);

    /// <summary>Collapses consecutive repeats of the character into a single occurrence.</summary>
    /// <param name="text">The text to modify.</param>
    /// <param name="character">The character to collapse.</param>
    public static string RemoveConsecutiveDuplicates(this string text, char character)
    {
        ArgumentNullException.ThrowIfNull(text);

        string characterString = character.ToString();

        return text.RegexReplace($"{characterString.RegexEscape()}{{2,}}", characterString);
    }

    /// <summary>Collapses consecutive repeats of each given character into a single occurrence.</summary>
    /// <param name="text">The text to modify.</param>
    /// <param name="characters">The characters to collapse.</param>
    public static string RemoveConsecutiveDuplicates(this string text, char[] characters)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(characters);

        foreach (char character in characters)
        {
            text = text.RemoveConsecutiveDuplicates(character);
        }

        return text;
    }

    /// <summary>Returns the number of line feed characters in the text.</summary>
    /// <param name="text">The text to inspect.</param>
    public static int EndLineCount(this string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return text.AsSpan().Count('\n');
    }

    /// <summary>Whether the text is <c>null</c> or empty.</summary>
    /// <param name="text">The text to check.</param>
    public static bool IsNullOrEmpty(this string? text) => string.IsNullOrEmpty(text);

    /// <summary>Whether the text is <c>null</c>, empty, or white space.</summary>
    /// <param name="text">The text to check.</param>
    public static bool IsNullOrWhiteSpace(this string? text) => string.IsNullOrWhiteSpace(text);

    /// <summary>Returns the leftmost characters of the text.</summary>
    /// <param name="text">The text to take from.</param>
    /// <param name="count">The number of characters to take; values below one produce an empty string.</param>
    public static string Left(this string text, int count = 1)
    {
        if (count <= 0 || text.IsNullOrEmpty())
        {
            return string.Empty;
        }

        return count > text.Length ? text : text[..count];
    }

    /// <summary>Escapes the text for use in a regular expression pattern.</summary>
    /// <param name="text">The text to escape.</param>
    public static string RegexEscape(this string text) => Regex.Escape(text);

    /// <summary>Replaces regular expression matches in the text.</summary>
    /// <param name="text">The text to modify.</param>
    /// <param name="pattern">The regular expression pattern.</param>
    /// <param name="replacement">The replacement text.</param>
    public static string RegexReplace(this string text, string pattern, string replacement)
        => Regex.Replace(text, pattern, replacement);

    /// <summary>Replaces regular expression matches in the text using the given options.</summary>
    /// <param name="text">The text to modify.</param>
    /// <param name="pattern">The regular expression pattern.</param>
    /// <param name="replacement">The replacement text.</param>
    /// <param name="options">The regular expression options.</param>
    public static string RegexReplace(this string text, string pattern, string replacement, RegexOptions options)
        => Regex.Replace(text, pattern, replacement, options);

    /// <summary>Removes all punctuation and symbol characters from the text.</summary>
    /// <param name="text">The text to modify.</param>
    public static string RemoveAllPunctuation(this string text)
        => Regex.Replace(text, RegexLibrary.Punctuation, string.Empty);

    /// <summary>Removes diacritical marks (accents) from the text.</summary>
    /// <param name="text">The text to modify.</param>
    public static string RemoveDiacritics(this string text)
    {
        string normalizedString = text.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new(text.Length);

        foreach (Rune character in normalizedString.EnumerateRunes())
        {
            UnicodeCategory unicodeCategory = Rune.GetUnicodeCategory(character);

            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(character);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>Removes characters from the left, treating a CRLF pair as a single character when requested.</summary>
    /// <param name="text">The text to modify.</param>
    /// <param name="count">The number of characters to remove.</param>
    /// <param name="treatNewLineAsSingle">Whether a CRLF pair counts as a single character.</param>
    public static string RemoveLeft(this string text, int count = 1, bool treatNewLineAsSingle = true)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (count < 1)
        {
            return text;
        }

        ReadOnlySpan<char> span = text.AsSpan();
        int index = 0;
        int removed = 0;

        while (index < span.Length && removed < count)
        {
            if (treatNewLineAsSingle && span[index] == '\r' && index + 1 < span.Length && span[index + 1] == '\n')
            {
                index += 2;
            }

            else
            {
                index++;
            }

            removed++;
        }

        return span[index..].ToString();
    }

    /// <summary>Removes characters from the right, treating a CRLF pair as a single character when requested.</summary>
    /// <param name="text">The text to modify.</param>
    /// <param name="count">The number of characters to remove.</param>
    /// <param name="treatNewLineAsSingle">Whether a CRLF pair counts as a single character.</param>
    public static string RemoveRight(this string text, int count = 1, bool treatNewLineAsSingle = true)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (count < 1)
        {
            return text;
        }

        ReadOnlySpan<char> span = text.AsSpan();
        int index = span.Length;
        int removed = 0;

        while (index > 0 && removed < count)
        {
            if (treatNewLineAsSingle && span[index - 1] == '\n' && index >= 2 && span[index - 2] == '\r')
            {
                index -= 2;
            }

            else
            {
                index--;
            }

            removed++;
        }

        return span[..index].ToString();
    }

    /// <summary>Returns the text repeated the given number of times.</summary>
    /// <param name="str">The text to repeat.</param>
    /// <param name="count">The number of repetitions; values below one produce an empty string.</param>
    public static string Repeat(this string str, int count) =>
        count <= 0
            ? string.Empty
            : new StringBuilder(str.Length * count).Insert(0, str, count).ToString();

    /// <summary>Returns the rightmost characters of the text.</summary>
    /// <param name="text">The text to take from.</param>
    /// <param name="count">The number of characters to take; values below one produce an empty string.</param>
    public static string Right(this string text, int count = 1)
    {
        if (count <= 0 || text.IsNullOrEmpty())
        {
            return string.Empty;
        }

        return count > text.Length ? text : text[^count..];
    }

    /// <summary>Converts the text to camelCase.</summary>
    /// <param name="text">The text to convert.</param>
    public static string ToCamelCase(this string text)
    {
        if (text.IsNullOrEmpty())
        {
            return string.Empty;
        }

        ContextualStringSplitOptions options = new()
        {
            IncludeAcronyms = true,
            IncludeNumbers = true,
            RemoveEmptyEntries = true,
            TrimEntries = true
        };

        string[] words = text.ContextualSplit(CommonSeparators, options);
        string firstWord = words[0].ToLower();

        return words[1..].Aggregate(firstWord, (current, word)
            => $"{current}{word.ToLower().Capitalize()}");
    }

    /// <summary>Returns the lines of the text, optionally dropping empty lines.</summary>
    /// <param name="text">The text to split.</param>
    /// <param name="removeEmptyLines">Whether empty lines are dropped.</param>
    public static List<string> GetLines(this string text, bool removeEmptyLines = true)
    {
        text = text.ReplaceLineEndings(NewLine.LineFeed);

        return text.Split(NewLine.LineFeed,
                removeEmptyLines ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None)
            .ToList();
    }

    /// <summary>Replaces backslashes with forward slashes.</summary>
    /// <param name="text">The text to convert.</param>
    public static string ToForwardSlash(this string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return text.Replace('\\', '/');
    }

    /// <summary>Parses the text as an enum member name, returning the default when parsing fails.</summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="text">The text to parse.</param>
    /// <param name="defaultEnumValue">The value returned when parsing fails.</param>
    /// <param name="ignoreCase">Whether parsing ignores case.</param>
    public static TEnum ToEnum<TEnum>(this string text, TEnum defaultEnumValue = default, bool ignoreCase = true)
        where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(text);

        return Enum.TryParse(text.Trim(), ignoreCase, out TEnum resultEnum)
            ? resultEnum
            : defaultEnumValue;
    }

    extension(string text)
    {
        /// <summary>Whether the text contains no lowercase letters.</summary>
        public bool IsAllUpper
        {
            get
            {
                foreach (char c in text)
                {
                    if (char.IsLower(c))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>Whether the text contains no uppercase letters.</summary>
        public bool IsAllLower
        {
            get
            {
                foreach (char c in text)
                {
                    if (char.IsUpper(c))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>The words of the text, with punctuation stripped but hyphens, underscores, and quotes kept.</summary>
        public List<string> Words
        {
            get
            {
                ArgumentNullException.ThrowIfNull(text);

                List<char> modifiedPunctuations = [..Punctuations];
                modifiedPunctuations.Remove('-');
                modifiedPunctuations.Remove('\'');
                modifiedPunctuations.Remove('_');

                return text.Remove(modifiedPunctuations.ToArray())
                    .Split(Array.Empty<char>(),
                        StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }
        }

        /// <summary>The lines of the text, with empty lines removed.</summary>
        public List<string> Lines
        {
            get
            {
                ArgumentNullException.ThrowIfNull(text);

                return text.ReplaceLineEndings(NewLine.LineFeed)
                    .Split(NewLine.LineFeed, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }
        }

        /// <summary>The index of the last character, or <c>-1</c> for an empty text.</summary>
        public int LastIndex
        {
            get
            {
                ArgumentNullException.ThrowIfNull(text);

                return text.Length - 1;
            }
        }

        /// <summary>Counts the characters in the text, excluding end-of-line characters and any configured exclusions.</summary>
        /// <param name="options">Counting options; defaults are used when <c>null</c>.</param>
        public int CountCharacters(TextOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(text);

            options ??= new TextOptions();

            List<string> escapedCharacters = options.CharactersToExclude
                .Select(character => character.ToString().RegexEscape())
                .ToList();

            if (options.ExcludeEndLineCharacters)
            {
                escapedCharacters.Add(
                    NewLine.LineFeed,
                    NewLine.CarriageReturn,
                    NewLine.CarriageReturnLineFeed
                );
            }

            string regexExcludeList = $"({escapedCharacters.Join('|')})";

            return Regex.Replace(text, regexExcludeList, string.Empty).Length;
        }

        /// <summary>Counts the paragraphs in the text, split on double newlines.</summary>
        public int CountParagraphs()
        {
            ArgumentNullException.ThrowIfNull(text);

            string[] paragraphs = text.Split
            (
                [
                    NewLine.CarriageReturnLineFeed.Repeat(2),
                    NewLine.LineFeed.Repeat(2)
                ],
                StringSplitOptions.RemoveEmptyEntries
            );

            return paragraphs.Length;
        }

        /// <summary>Counts the sentences in the text, designed for English input.</summary>
        /// <param name="options">Counting options; defaults are used when <c>null</c>.</param>
        public int CountSentences(TextOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(text);

            options ??= new TextOptions();

            const StringSplitOptions splitOptions = StringSplitOptions.RemoveEmptyEntries
                | StringSplitOptions.TrimEntries;

            char[] sentenceEndings = ['.', '!', '?'];

            string[] words = text.RegexReplace(RegexLibrary.DottedAbbreviation, string.Empty)
                .RemoveConsecutiveDuplicates(['.', '!', '?'])
                .Split(Array.Empty<char>(), splitOptions);

            if (words.Length == 0)
            {
                return 0;
            }

            int sentenceCount = 0;

            for (int i = 0; i < words.Length; i++)
            {
                if (!sentenceEndings.Any(end => words[i].Contains(end)))
                {
                    continue;
                }

                bool isNextWordLower = options.ExcludeLowerCharacterAfterSentenceEnd
                    && i + 1 < words.Length
                    && words[i + 1][0].IsLower();

                if (isNextWordLower)
                {
                    continue;
                }

                sentenceCount++;
            }

            if (!sentenceEndings.Any(end => words.Last().Contains(end)))
            {
                sentenceCount++;
            }

            return sentenceCount;
        }

        /// <summary>Counts the distinct words in the text.</summary>
        /// <param name="options">Counting options; defaults are used when <c>null</c>.</param>
        public int CountUniqueWords(TextOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(text);

            options ??= new TextOptions();

            return options.CountCaseSensitiveWords
                ? text.Words.Distinct().Count()
                : text.ToLower().Words.Distinct().Count();
        }

        /// <summary>Counts the words in the text.</summary>
        /// <param name="options">Counting options; defaults are used when <c>null</c>.</param>
        public int CountWords(TextOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(text);

            options ??= new TextOptions();

            text = Regex.Replace(text, @"[^\w\s-]", string.Empty);
            text = Regex.Replace(text, @"\s+", " ");

            if (!options.CountHyphenatedWordAsSingle)
            {
                text = Regex.Replace(text, "-", " ");
            }

            if (!options.CountNumbers)
            {
                text = Regex.Replace(text, "[0-9]+", string.Empty);
            }

            string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return words.Length;
        }

        /// <summary>Returns a frequency map of the words in the text.</summary>
        /// <param name="options">Counting options; defaults are used when <c>null</c>.</param>
        public Dictionary<string, int> GetWordFrequency(TextOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(text);

            options ??= new TextOptions();

            string workingText = options.CountCaseSensitiveWords ? text : text.ToLower();
            Dictionary<string, int> wordFrequency = new();

            foreach (string rawWord in workingText.Words)
            {
                string word = rawWord.Trim('\'');

                if (!wordFrequency.TryAdd(word, 1))
                {
                    wordFrequency[word]++;
                }
            }

            return wordFrequency;
        }

        /// <summary>Whether the text is enclosed between the delimiters described by the options.</summary>
        /// <param name="options">The search options.</param>
        public bool IsBetween(IsBetweenSearchOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return text.IsBetween(options.TextToFind, options.LeftDelimiter, options.RightDelimiter,
                options.StartIndex, options.CanBeEscaped, options.EscapeChar);
        }

        /// <summary>Whether the text is enclosed between the given delimiters, optionally honoring an escape character.</summary>
        /// <param name="textToFind">The text to locate.</param>
        /// <param name="leftDelimiter">The delimiter that must appear before the text.</param>
        /// <param name="rightDelimiter">The delimiter that must appear after the text.</param>
        /// <param name="startIndex">The index at which to start searching.</param>
        /// <param name="canBeEscaped">Whether a delimiter preceded by the escape character does not count.</param>
        /// <param name="escapeChar">The escape character.</param>
        public bool IsBetween(string textToFind, char leftDelimiter, char rightDelimiter,
            int startIndex = 0, bool canBeEscaped = false, char escapeChar = '\0')
        {
            ArgumentNullException.ThrowIfNull(text);

            if (textToFind.IsNullOrEmpty())
            {
                return false;
            }

            int index = text.IndexOf(textToFind, startIndex, StringComparison.Ordinal);

            if (index == -1)
            {
                return false;
            }

            ReadOnlySpan<char> span = text.AsSpan();
            bool foundLeftDelimiter = false;

            for (int i = index - 1; i >= 0; i--)
            {
                if (span[i] != leftDelimiter)
                {
                    continue;
                }

                if (canBeEscaped && i > 0 && span[i - 1] == escapeChar)
                {
                    i--;
                    continue;
                }

                foundLeftDelimiter = true;
                break;
            }

            if (!foundLeftDelimiter)
            {
                return false;
            }

            for (int i = index + textToFind.Length; i < span.Length; i++)
            {
                if (span[i] != rightDelimiter)
                {
                    continue;
                }

                if (canBeEscaped && i > 0 && span[i - 1] == escapeChar)
                {
                    continue;
                }

                return true;
            }

            return false;
        }
    }
    /// <summary>Converts the text to PascalCase.</summary>
    /// <param name="text">The text to convert.</param>
    public static string ToPascalCase(this string text)
    {
        if (text.IsNullOrEmpty())
        {
            return string.Empty;
        }

        ContextualStringSplitOptions options = new()
        {
            IncludeAcronyms = true,
            IncludeNumbers = true,
            RemoveEmptyEntries = true,
            TrimEntries = true
        };

        string[] words = text.ContextualSplit(CommonSeparators, options);

        return words.Aggregate(string.Empty, (current, word)
            => current + word.ToLower().Capitalize());
    }

    /// <summary>Converts the text to snake_case.</summary>
    /// <param name="text">The text to convert.</param>
    public static string ToSnakeCase(this string text)
    {
        if (text.IsNullOrEmpty())
        {
            return string.Empty;
        }

        ContextualStringSplitOptions options = new()
        {
            IncludeAcronyms = true,
            IncludeNumbers = true,
            RemoveEmptyEntries = true,
            TrimEntries = true
        };

        string[] words = text.ContextualSplit(CommonSeparators, options);

        return words.Aggregate((current, word) => $"{current}_{word}".ToLower());
    }

    /// <summary>Splits the text on the given separators using default options.</summary>
    /// <param name="text">The text to split.</param>
    /// <param name="separators">The separator characters; an empty set defaults to space.</param>
    public static string[] ContextualSplit(this string text, char[] separators)
        => ContextualSplit(text, separators, new ContextualStringSplitOptions());

    /// <summary>Splits the text on the given separators, optionally splitting acronym and number runs.</summary>
    /// <param name="text">The text to split.</param>
    /// <param name="separators">The separator characters; an empty set defaults to space.</param>
    /// <param name="options">The split options.</param>
    public static string[] ContextualSplit(this string text, char[] separators, ContextualStringSplitOptions options)
    {
        if (options is { IncludeAcronyms: false, IncludeNumbers: false })
        {
            StringSplitOptions stringSplitOptions = StringSplitOptions.None;

            stringSplitOptions = options.RemoveEmptyEntries
                ? stringSplitOptions | StringSplitOptions.RemoveEmptyEntries
                : stringSplitOptions;

            stringSplitOptions = options.TrimEntries
                ? stringSplitOptions | StringSplitOptions.TrimEntries
                : stringSplitOptions;

            return text.Split(separators, stringSplitOptions);
        }

        StringBuilder wordBuilder = new();
        List<string> result = [];
        separators = separators.IsNullOrEmpty() ? [' '] : separators;
        bool currentIsDigit = false;
        bool currentIsAcronym = false;

        foreach (char c in text)
        {
            if (separators.Contains(c))
            {
                AddWord(wordBuilder.ToString());
                wordBuilder.Clear();
                continue;
            }

            if (options.IncludeNumbers)
            {
                if (c.IsDigit())
                {
                    if (!wordBuilder.IsEmpty && !currentIsDigit)
                    {
                        AddWord(wordBuilder.ToString());
                        wordBuilder.Clear();
                    }

                    currentIsDigit = true;
                    wordBuilder.Append(c);
                    continue;
                }

                if (!c.IsDigit() && currentIsDigit)
                {
                    AddWord(wordBuilder.ToString());
                    wordBuilder.Clear();
                    currentIsDigit = false;
                }
            }

            if (options.IncludeAcronyms)
            {
                if (c.IsLetter() && c.IsUpper())
                {
                    string lastStringChar = wordBuilder.PeekBack();
                    string secondLastStringChar = wordBuilder.PeekBack(2);

                    bool addWordBeforeAcronyms = !lastStringChar.IsNullOrEmpty()
                        && !secondLastStringChar.IsNullOrEmpty()
                        && secondLastStringChar[0].IsLower()
                        && lastStringChar[0].IsUpper();

                    if (!wordBuilder.IsEmpty && addWordBeforeAcronyms)
                    {
                        AddWord(wordBuilder.ToString()[..^1]);
                        wordBuilder.Clear().Append(lastStringChar[0]).Append(c);
                        currentIsAcronym = true;
                        continue;
                    }
                }

                if (c.IsLetter() && c.IsLower() && currentIsAcronym)
                {
                    AddWord(wordBuilder.ToString());
                    wordBuilder.Clear();
                    currentIsAcronym = false;
                }
            }

            wordBuilder.Append(c);
        }

        AddWord(wordBuilder.ToString());

        return result.ToArray();

        void AddWord(string word)
        {
            word = options.TrimEntries ? word.Trim() : word;

            if (options.RemoveEmptyEntries && word.IsNullOrEmpty())
            {
                return;
            }

            result.Add(word);
        }
    }

    /// <summary>Returns the longest common subsequence of the two texts.</summary>
    /// <param name="text">The first text.</param>
    /// <param name="otherText">The second text.</param>
    public static string LongestCommonSubsequence(this string text, string otherText)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(otherText);

        int[,] lengths = new int[text.Length + 1, otherText.Length + 1];

        for (int i = 1; i <= text.Length; i++)
        {
            for (int j = 1; j <= otherText.Length; j++)
            {
                if (text[i - 1] == otherText[j - 1])
                {
                    lengths[i, j] = lengths[i - 1, j - 1] + 1;
                }

                else
                {
                    lengths[i, j] = Math.Max(lengths[i - 1, j], lengths[i, j - 1]);
                }
            }
        }

        int indexA = text.Length;
        int indexB = otherText.Length;
        int lcsLength = lengths[text.Length, otherText.Length];
        char[] lcsCharacters = new char[lcsLength];
        int writeIndex = lcsLength - 1;

        while (indexA > 0 && indexB > 0)
        {
            if (text[indexA - 1] == otherText[indexB - 1])
            {
                lcsCharacters[writeIndex--] = text[indexA - 1];
                indexA--;
                indexB--;
            }

            else if (lengths[indexA - 1, indexB] > lengths[indexA, indexB - 1])
            {
                indexA--;
            }

            else
            {
                indexB--;
            }
        }

        return new string(lcsCharacters);
    }

    /// <summary>Removes the trailing suffix when present; otherwise returns the text unchanged.</summary>
    /// <param name="text">The text to modify.</param>
    /// <param name="suffix">The suffix to remove.</param>
    public static string RemoveSuffix(this string text, string suffix)
    {
        int length = text.LastIndexOf(suffix, StringComparison.Ordinal);
        return length < 0 ? text : text[..length];
    }

    /// <summary>Removes the leading prefix when present; otherwise returns the text unchanged.</summary>
    /// <param name="text">The text to modify.</param>
    /// <param name="prefix">The prefix to remove.</param>
    public static string RemovePrefix(this string text, string prefix)
        => text.StartsWith(prefix) ? text[prefix.Length..] : text;

    /// <summary>Returns the text wrapped in the same delimiter on both sides.</summary>
    /// <param name="text">The text to wrap.</param>
    /// <param name="startDelimiter">The delimiter used on both sides.</param>
    public static string SurroundWith(this string text, string startDelimiter)
        => SurroundWith(text, startDelimiter, startDelimiter);

    /// <summary>Returns the text wrapped between the two delimiters.</summary>
    /// <param name="text">The text to wrap.</param>
    /// <param name="startDelimiter">The leading delimiter.</param>
    /// <param name="endDelimiter">The trailing delimiter.</param>
    public static string SurroundWith(this string text, string startDelimiter, string endDelimiter)
        => $"{startDelimiter}{text}{endDelimiter}";

    /// <summary>Parses the text into a value of the given parsable type using the current culture.</summary>
    /// <typeparam name="T">The parsable type.</typeparam>
    /// <param name="text">The text to parse.</param>
    public static T Parse<T>(this string text) where T : IParsable<T>
        => T.Parse(text, CultureInfo.CurrentCulture);

    /// <summary>Tries to parse the text into a value of the given parsable type using the current culture.</summary>
    /// <typeparam name="T">The parsable type.</typeparam>
    /// <param name="text">The text to parse.</param>
    /// <param name="value">The parsed value on success.</param>
    public static bool TryParse<T>(this string text, [NotNullWhen(true)] out T? value) where T : IParsable<T>
        => T.TryParse(text, CultureInfo.CurrentCulture, out value);
}
