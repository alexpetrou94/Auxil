using System.Text;
using System.Text.RegularExpressions;

namespace Auxil.Common.Text;

/// <summary>A fluent builder for constructing regular expression patterns.</summary>
[PublicAPI]
public class RegexBuilder
{
    private readonly StringBuilder _sbPattern = new();
    private readonly StringBuilder _sbPrefix = new();
    private readonly StringBuilder _sbSuffix = new();
    private string _lastPattern = "__LastPattern";
    private RegexOptions _lastOptions = RegexOptions.None;
    private TimeSpan _lastTimeout = Regex.InfiniteMatchTimeout;
    private Regex _regex = new(string.Empty);
    private RegexOptions _regexOptions = RegexOptions.None;
    private TimeSpan _regexTimeout = Regex.InfiniteMatchTimeout;

    /// <summary>Creates an empty builder.</summary>
    public RegexBuilder() { }

    /// <summary>Creates a copy of the given builder, including its pattern and options.</summary>
    /// <param name="regexBuilder">The builder to copy.</param>
    public RegexBuilder(RegexBuilder regexBuilder)
    {
        ArgumentNullException.ThrowIfNull(regexBuilder);

        _sbPattern.Append(regexBuilder._sbPattern);
        _sbPrefix.Append(regexBuilder._sbPrefix);
        _sbSuffix.Append(regexBuilder._sbSuffix);
        _regexOptions = regexBuilder._regexOptions;
        _regexTimeout = regexBuilder._regexTimeout;
    }

    /// <summary>Returns the currently constructed pattern.</summary>
    public override string ToString() => GetRegexPattern();

    /// <summary>Appends the absolute end-of-string anchor (<c>\z</c>) to the pattern suffix.</summary>
    public RegexBuilder AbsoluteEndOfString()
    {
        _sbSuffix.Append(RegexLibrary.AbsoluteEndOfString);

        return this;
    }

    /// <summary>Appends an atomic group matching any one of the given alternatives.</summary>
    /// <param name="alternates">The alternatives to match; each is escaped individually.</param>
    public RegexBuilder AnyBetween(params string[] alternates)
    {
        ArgumentNullException.ThrowIfNull(alternates);

        _sbPattern.Append("(?>").Append(alternates.Select(Regex.Escape).Join('|')).Append(')');

        return this;
    }

    /// <summary>Appends the any-character token (<c>.</c>).</summary>
    public RegexBuilder AnyCharacter()
    {
        _sbPattern.Append(RegexLibrary.AnyCharacter);

        return this;
    }

    /// <summary>Appends a negated character class matching any character except the given ones.</summary>
    /// <param name="characters">The characters to exclude.</param>
    public RegexBuilder AnyCharacterBut(string characters)
    {
        ArgumentNullException.ThrowIfNull(characters);

        _sbPattern.Append("[^").Append(characters).Append(']');

        return this;
    }

    /// <summary>Appends the any-digit token (<c>\d</c>).</summary>
    public RegexBuilder AnyDigit()
    {
        _sbPattern.Append(RegexLibrary.AnyDigit);

        return this;
    }

    /// <summary>Appends the integer pattern.</summary>
    public RegexBuilder AnyInteger()
    {
        _sbPattern.Append(RegexLibrary.Integer);

        return this;
    }

    /// <summary>Appends the any-letter class (<c>\p{L}</c>).</summary>
    public RegexBuilder AnyLetter()
    {
        _sbPattern.Append(RegexLibrary.AnyLetter);

        return this;
    }

    /// <summary>Appends the any-lowercase-letter class (<c>\p{Ll}</c>).</summary>
    public RegexBuilder AnyLowerLetter()
    {
        _sbPattern.Append(RegexLibrary.AnyLowerLetter);

        return this;
    }

    /// <summary>Appends the any-non-digit token (<c>\D</c>).</summary>
    public RegexBuilder AnyNonDigit()
    {
        _sbPattern.Append(RegexLibrary.AnyNonDigit);

        return this;
    }

    /// <summary>Appends the any-non-letter class (<c>\P{L}</c>).</summary>
    public RegexBuilder AnyNonLetter()
    {
        _sbPattern.Append(RegexLibrary.AnyNonLetter);

        return this;
    }

    /// <summary>Appends the any-non-lowercase-letter class (<c>\P{Ll}</c>).</summary>
    public RegexBuilder AnyNonLowerLetter()
    {
        _sbPattern.Append(RegexLibrary.AnyNonLowerLetter);

        return this;
    }

    /// <summary>Appends the any-non-uppercase-letter class (<c>\P{Lu}</c>).</summary>
    public RegexBuilder AnyNonUpperLetter()
    {
        _sbPattern.Append(RegexLibrary.AnyNonUpperLetter);

        return this;
    }

    /// <summary>Appends the any-non-white-space token (<c>\S</c>).</summary>
    public RegexBuilder AnyNonWhiteSpace()
    {
        _sbPattern.Append(RegexLibrary.AnyNonWhiteSpace);

        return this;
    }

    /// <summary>Appends the any-non-word token (<c>\W</c>).</summary>
    public RegexBuilder AnyNonWord()
    {
        _sbPattern.Append(RegexLibrary.AnyNonWord);

        return this;
    }

    /// <summary>Appends a character class matching any of the given characters.</summary>
    /// <param name="characters">The characters to match.</param>
    public RegexBuilder AnyOfCharacter(string characters)
    {
        ArgumentNullException.ThrowIfNull(characters);

        _sbPattern.Append('[').Append(characters).Append(']');

        return this;
    }

    /// <summary>Appends the any-uppercase-letter class (<c>\p{Lu}</c>).</summary>
    public RegexBuilder AnyUpperLetter()
    {
        _sbPattern.Append(RegexLibrary.AnyUpperLetter);

        return this;
    }

    /// <summary>Appends the any-white-space token (<c>\s</c>).</summary>
    public RegexBuilder AnyWhiteSpace()
    {
        _sbPattern.Append(RegexLibrary.AnyWhiteSpace);

        return this;
    }

    /// <summary>Appends the any-word token (<c>\w</c>).</summary>
    public RegexBuilder AnyWord()
    {
        _sbPattern.Append(RegexLibrary.AnyWord);

        return this;
    }

    /// <summary>Appends a range quantifier matching between the given repetition counts.</summary>
    /// <param name="min">The minimum repetitions.</param>
    /// <param name="max">The maximum repetitions.</param>
    public RegexBuilder Between(int min, int max)
    {
        if (min > max)
        {
            throw new ArgumentOutOfRangeException(nameof(min), "Min value cannot be smaller than max value.");
        }

        if (min < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(min), "Min value cannot be smaller than 0.");
        }

        _sbPattern.Append('{').Append(min).Append(',').Append(max).Append('}');

        return this;
    }

    /// <summary>Appends the word boundary token (<c>\b</c>).</summary>
    public RegexBuilder Boundary()
    {
        _sbPattern.Append(RegexLibrary.Boundary);

        return this;
    }

    /// <summary>Appends a named capture group around the given builder's pattern.</summary>
    /// <param name="name">The group name.</param>
    /// <param name="regexBuilder">The pattern to capture.</param>
    public RegexBuilder Capture(string name, RegexBuilder regexBuilder)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(regexBuilder);

        RegexBuilder rb = new(regexBuilder);
        rb._sbPrefix.Clear();
        rb._sbSuffix.Clear();
        _sbPattern.Append("(?<").Append(name).Append('>').Append(rb.GetRegexPattern()).Append(')');

        return this;
    }

    /// <summary>Appends a named capture group around the given text.</summary>
    /// <param name="name">The group name.</param>
    /// <param name="text">The text to capture.</param>
    /// <param name="escape">Whether the text is escaped before embedding.</param>
    public RegexBuilder Capture(string name, string text, bool escape = true)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(text);

        text = escape ? Regex.Escape(text) : text;
        _sbPattern.Append("(?<").Append(name).Append('>').Append(text).Append(')');

        return this;
    }

    /// <summary>Resets the builder to an empty pattern.</summary>
    public RegexBuilder Clear()
    {
        _sbPrefix.Clear();
        _sbPattern.Clear();
        _sbSuffix.Clear();

        return this;
    }

    /// <summary>Anchors the pattern with both start-of-string and end-of-string anchors.</summary>
    public RegexBuilder CompleteMatch()
    {
        StartOfString().EndOfString();

        return this;
    }

    /// <summary>Counts the matches in the input.</summary>
    /// <param name="input">The input text.</param>
    public int Count(string input) => GetRegex().Count(input);

    /// <summary>Appends the end-of-string anchor (<c>\Z</c>) to the pattern suffix.</summary>
    public RegexBuilder EndOfString()
    {
        _sbSuffix.Append(RegexLibrary.EndOfString);

        return this;
    }

    /// <summary>Appends an exact repetition quantifier.</summary>
    /// <param name="numOfPatterns">The exact repetition count.</param>
    public RegexBuilder Exactly(int numOfPatterns)
    {
        _sbPattern.Append('{').Append(numOfPatterns).Append('}');

        return this;
    }

    /// <summary>Appends a minimum repetition quantifier.</summary>
    /// <param name="numOfPatterns">The minimum repetition count.</param>
    public RegexBuilder ExactlyOrMore(int numOfPatterns)
    {
        _sbPattern.Append('{').Append(numOfPatterns).Append(',').Append('}');

        return this;
    }

    /// <summary>Returns the compiled regex for the current pattern, options, and timeout, caching between calls.</summary>
    public Regex GetRegex()
    {
        string pattern = GetRegexPattern();

        if (_lastPattern == pattern && _lastOptions == _regexOptions && _lastTimeout == _regexTimeout)
        {
            return _regex;
        }

        _regex = new Regex(pattern, _regexOptions, _regexTimeout);
        _lastPattern = pattern;
        _lastOptions = _regexOptions;
        _lastTimeout = _regexTimeout;

        return _regex;
    }

    /// <summary>Returns the currently constructed pattern including anchors.</summary>
    public string GetRegexPattern() =>
        new StringBuilder()
            .Append(_sbPrefix)
            .Append(_sbPattern)
            .Append(_sbSuffix)
            .ToString();

    /// <summary>Appends a regex library pattern verbatim, without escaping.</summary>
    /// <param name="regexLibraryPattern">The pattern fragment to append.</param>
    public RegexBuilder Library(string regexLibraryPattern) => Text(regexLibraryPattern, false);

    /// <summary>Appends the line break class (<c>[\r\n]</c>).</summary>
    public RegexBuilder LineBreak()
    {
        _sbPattern.Append(RegexLibrary.LineBreak);

        return this;
    }

    /// <summary>Runs the pattern and returns the first structured match.</summary>
    /// <param name="input">The input text.</param>
    public RegexResult Match(string input) => GetRegex().BetterMatch(input);

    /// <summary>Runs the pattern over all matches, stripping anchors so multi-matching works.</summary>
    /// <param name="input">The input text.</param>
    public List<RegexResult> Matches(string input)
    {
        RegexBuilder rb = new(this);
        rb._sbPrefix.Clear();
        rb._sbSuffix.Clear();

        return rb.GetRegex().BetterMatches(input);
    }

    /// <summary>Appends the non-word-boundary token (<c>\B</c>).</summary>
    public RegexBuilder NonBoundary()
    {
        _sbPattern.Append(RegexLibrary.NonBoundary);

        return this;
    }

    /// <summary>Appends a non-capturing group around the given builder's pattern.</summary>
    /// <param name="regexBuilder">The pattern to wrap.</param>
    public RegexBuilder NonCapture(RegexBuilder regexBuilder)
    {
        ArgumentNullException.ThrowIfNull(regexBuilder);

        RegexBuilder rb = new(regexBuilder);
        rb._sbPrefix.Clear();
        rb._sbSuffix.Clear();
        _sbPattern.Append("(?:").Append(rb.GetRegexPattern()).Append(')');

        return this;
    }

    /// <summary>Appends a non-capturing group around the given text.</summary>
    /// <param name="text">The text to wrap.</param>
    /// <param name="escape">Whether the text is escaped before embedding.</param>
    public RegexBuilder NonCapture(string text, bool escape = true)
    {
        ArgumentNullException.ThrowIfNull(text);

        text = escape ? Regex.Escape(text) : text;
        _sbPattern.Append("(?:").Append(text).Append(')');

        return this;
    }

    /// <summary>Appends the one-or-more quantifier (<c>+</c>).</summary>
    public RegexBuilder OneOrMore()
    {
        _sbPattern.Append(RegexLibrary.OneOrMore);

        return this;
    }

    /// <summary>Replaces matches in the input with the replacement text.</summary>
    /// <param name="input">The input text.</param>
    /// <param name="replacement">The replacement text.</param>
    public string Replace(string input, string replacement) => GetRegex().Replace(input, replacement);

    /// <summary>Sets the regex options used when compiling the pattern.</summary>
    /// <param name="options">The regex options.</param>
    public RegexBuilder SetRegexOptions(RegexOptions options)
    {
        _regexOptions = options;

        return this;
    }

    /// <summary>Sets the match timeout used when compiling the pattern.</summary>
    /// <param name="timeout">The match timeout.</param>
    public RegexBuilder SetRegexTimeout(TimeSpan timeout)
    {
        _regexTimeout = timeout;

        return this;
    }

    /// <summary>Appends the space-or-tab class (<c>[ \t]</c>).</summary>
    public RegexBuilder SpaceOrTab()
    {
        _sbPattern.Append(RegexLibrary.SpaceOrTab);

        return this;
    }

    /// <summary>Appends the start-of-string anchor (<c>\A</c>) to the pattern prefix.</summary>
    public RegexBuilder StartOfString()
    {
        _sbPrefix.Append(RegexLibrary.StartOfString);

        return this;
    }

    /// <summary>Appends the tab token (<c>\t</c>).</summary>
    public RegexBuilder Tab()
    {
        _sbPattern.Append(RegexLibrary.Tab);

        return this;
    }

    /// <summary>Whether the pattern matches anywhere in the input.</summary>
    /// <param name="text">The input text.</param>
    public bool IsMatch(string text) => GetRegex().IsMatch(text);

    /// <summary>Appends the given text, escaped by default.</summary>
    /// <param name="text">The text to append.</param>
    /// <param name="escape">Whether the text is escaped before embedding.</param>
    public RegexBuilder Text(string text, bool escape = true)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (escape)
        {
            _sbPattern.Append(Regex.Escape(text));
            return this;
        }

        _sbPattern.Append(text);
        return this;
    }

    /// <summary>Appends the given builder's pattern, without its anchors.</summary>
    /// <param name="regexBuilder">The pattern to embed.</param>
    public RegexBuilder Text(RegexBuilder regexBuilder)
    {
        ArgumentNullException.ThrowIfNull(regexBuilder);

        RegexBuilder rb = new(regexBuilder);
        rb._sbPrefix.Clear();
        rb._sbSuffix.Clear();

        _sbPattern.Append(rb.GetRegexPattern());

        return this;
    }

    /// <summary>Appends the white-space class (<c>[ \t\r\n]</c>).</summary>
    public RegexBuilder WhiteSpace()
    {
        _sbPattern.Append(RegexLibrary.WhiteSpace);

        return this;
    }

    /// <summary>Appends the zero-or-more quantifier (<c>*</c>).</summary>
    public RegexBuilder ZeroOrMore()
    {
        _sbPattern.Append(RegexLibrary.ZeroOrMore);

        return this;
    }

    /// <summary>Appends the zero-or-one quantifier (<c>?</c>).</summary>
    public RegexBuilder ZeroOrOne()
    {
        _sbPattern.Append(RegexLibrary.ZeroOrOne);

        return this;
    }
}
