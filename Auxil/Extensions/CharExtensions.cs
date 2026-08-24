namespace Auxil.Extensions;

/// <summary>Provides convenience members for working with characters.</summary>
[PublicAPI]
public static class CharExtensions
{
    /// <summary>Whether the character is a lowercase letter.</summary>
    public static bool IsLower(this char c) => char.IsLower(c);

    /// <summary>Whether the character is an uppercase letter.</summary>
    public static bool IsUpper(this char c) => char.IsUpper(c);

    /// <summary>Whether the character is a decimal digit.</summary>
    public static bool IsDigit(this char c) => char.IsDigit(c);

    /// <summary>Whether the character is a control character.</summary>
    public static bool IsControlChar(this char c) => char.IsControl(c);

    /// <summary>Whether the character is white space.</summary>
    public static bool IsWhiteSpace(this char c) => char.IsWhiteSpace(c);

    /// <summary>Whether the character is a letter.</summary>
    public static bool IsLetter(this char c) => char.IsLetter(c);

    /// <summary>Whether the character is a letter or a decimal digit.</summary>
    public static bool IsLetterOrDigit(this char c) => char.IsLetterOrDigit(c);

    /// <summary>Returns the lowercase invariant form of the character.</summary>
    public static char ToLowerInvariant(this char c) => char.ToLowerInvariant(c);

    /// <summary>Returns the uppercase invariant form of the character.</summary>
    public static char ToUpperInvariant(this char c) => char.ToUpperInvariant(c);

    /// <summary>Returns a string containing the character repeated the given number of times.</summary>
    /// <param name="c">The character to repeat.</param>
    /// <param name="count">The number of repetitions; values below one produce an empty string.</param>
    public static string Repeat(this char c, int count)
        => count <= 0 ? string.Empty : new string(c, count);
}
