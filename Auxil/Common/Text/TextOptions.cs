namespace Auxil.Common.Text;

/// <summary>Options controlling text analysis operations such as word and sentence counting.</summary>
[PublicAPI]
public record TextOptions
{
    /// <summary>Excludes end-of-line characters from character counts.</summary>
    public bool ExcludeEndLineCharacters { get; init; } = true;
    /// <summary>Additional characters to exclude from character counts.</summary>
    public IReadOnlyList<char> CharactersToExclude { get; init; } = [];
    /// <summary>Treats words differing only by case as distinct when counting unique words and frequencies.</summary>
    public bool CountCaseSensitiveWords { get; init; }
    /// <summary>Counts hyphenated words as a single word.</summary>
    public bool CountHyphenatedWordAsSingle { get; init; } = true;
    /// <summary>Counts numeric tokens as words.</summary>
    public bool CountNumbers { get; init; } = true;
    /// <summary>Ignores sentence-ending punctuation followed by a lowercase word when counting sentences.</summary>
    public bool ExcludeLowerCharacterAfterSentenceEnd { get; init; } = true;
}
