namespace Auxil.Common.Text;

/// <summary>Options describing a search for text enclosed between delimiters.</summary>
[PublicAPI]
public record IsBetweenSearchOptions
{
    /// <summary>The text to locate between the delimiters.</summary>
    public string TextToFind { get; init; } = string.Empty;
    /// <summary>The delimiter that must appear before the text.</summary>
    public char LeftDelimiter { get; init; }
    /// <summary>The delimiter that must appear after the text.</summary>
    public char RightDelimiter { get; init; }
    /// <summary>The character that escapes a delimiter when <see cref="CanBeEscaped"/> is enabled.</summary>
    public char EscapeChar { get; init; }
    /// <summary>The index at which to start searching for the text.</summary>
    public int StartIndex { get; init; }
    /// <summary>Treats a delimiter preceded by <see cref="EscapeChar"/> as not counting.</summary>
    public bool CanBeEscaped { get; init; }
}
