using System.Diagnostics.CodeAnalysis;

namespace Auxil.Common.Text;

/// <summary>Pre-built regular expression fragments for common patterns.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[PublicAPI]
public static class RegexLibrary
{
    /// <summary>The absolute end-of-string anchor (<c>\z</c>).</summary>
    public const string AbsoluteEndOfString = @"\z";
    /// <summary>The any-character token (<c>.</c>).</summary>
    public const char AnyCharacter = '.';
    /// <summary>The any-digit token (<c>\d</c>).</summary>
    public const string AnyDigit = @"\d";
    /// <summary>The any-letter class (<c>\p{L}</c>).</summary>
    public const string AnyLetter = @"\p{L}";
    /// <summary>The any-lowercase-letter class (<c>\p{Ll}</c>).</summary>
    public const string AnyLowerLetter = @"\p{Ll}";
    /// <summary>The any-non-digit token (<c>\D</c>).</summary>
    public const string AnyNonDigit = @"\D";
    /// <summary>The any-non-letter class (<c>\P{L}</c>).</summary>
    public const string AnyNonLetter = @"\P{L}";
    /// <summary>The any-non-lowercase-letter class (<c>\P{Ll}</c>).</summary>
    public const string AnyNonLowerLetter = @"\P{Ll}";
    /// <summary>The any-non-uppercase-letter class (<c>\P{Lu}</c>).</summary>
    public const string AnyNonUpperLetter = @"\P{Lu}";
    /// <summary>The any-non-white-space token (<c>\S</c>).</summary>
    public const string AnyNonWhiteSpace = @"\S";
    /// <summary>The any-non-word token (<c>\W</c>).</summary>
    public const string AnyNonWord = @"\W";
    /// <summary>The any-uppercase-letter class (<c>\p{Lu}</c>).</summary>
    public const string AnyUpperLetter = @"\p{Lu}";
    /// <summary>The any-white-space token (<c>\s</c>).</summary>
    public const string AnyWhiteSpace = @"\s";
    /// <summary>The any-word token (<c>\w</c>).</summary>
    public const string AnyWord = @"\w";
    /// <summary>The word boundary token (<c>\b</c>).</summary>
    public const string Boundary = @"\b";

    /// <summary>Matches dotted abbreviations such as <c>U.S.A</c>.</summary>
    public const string DottedAbbreviation = @"(?>[A-Za-z]\.){2,}[A-Za-z]?";
    /// <summary>The end-of-string anchor (<c>\Z</c>).</summary>
    public const string EndOfString = @"\Z";
    /// <summary>Matches floating point numbers such as <c>-2.5</c>.</summary>
    public const string Float = @"-?\d+(?:\.\d+)*";
    /// <summary>Matches hex colors such as <c>#fff</c> or <c>#aabbcc</c>.</summary>
    public const string HexColor = $"#(?:{HexDigit}{{6}}|{HexDigit}{{3}})";
    /// <summary>The hex digit class (<c>[A-Fa-f0-9]</c>).</summary>
    public const string HexDigit = "[A-Fa-f0-9]";
    /// <summary>Matches identifier-like tokens such as <c>foo_bar</c>.</summary>
    public const string Identifier = "[a-zA-Z_][a-zA-Z0-9_]*";
    /// <summary>Matches text within double quotes.</summary>
    public const string WithinDoubleQuotes = "\"(?<content>.*?[^\\\\])?\"";
    /// <summary>Matches text within single quotes.</summary>
    public const string WithinSingleQuotes = @"'(?<content>.*?[^\\])?'";

    /// <summary>Matches integer numbers such as <c>-7</c>.</summary>
    public const string Integer = @"-?\d+";
    /// <summary>The line break class (<c>[\r\n]</c>).</summary>
    public const string LineBreak = @"[\r\n]";
    /// <summary>The non-word-boundary token (<c>\B</c>).</summary>
    public const string NonBoundary = @"\B";

    /// <summary>The one-or-more quantifier (<c>+</c>).</summary>
    public const char OneOrMore = '+';
    /// <summary>Matches punctuation and symbol characters.</summary>
    public const string Punctuation = @"[\p{P}\p{S}]";
    /// <summary>The space-or-tab class (<c>[ \t]</c>).</summary>
    public const string SpaceOrTab = @"[ \t]";

    /// <summary>The start-of-string anchor (<c>\A</c>).</summary>
    public const string StartOfString = @"\A";
    /// <summary>The tab token (<c>\t</c>).</summary>
    public const string Tab = @"\t";
    /// <summary>Matches unsigned integers such as <c>42</c>.</summary>
    public const string UnsignedInteger = @"\d+";
    /// <summary>The white-space class (<c>[ \t\r\n]</c>).</summary>
    public const string WhiteSpace = @"[ \t\r\n]";
    /// <summary>The zero-or-more quantifier (<c>*</c>).</summary>
    public const char ZeroOrMore = '*';
    /// <summary>The zero-or-one quantifier (<c>?</c>).</summary>
    public const char ZeroOrOne = '?';
}
