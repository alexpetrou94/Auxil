using Auxil.Common.Text;

namespace Auxil.Utilities;

/// <summary>Provides the character sequences for the supported newline styles.</summary>
[PublicAPI]
public static class NewLine
{
    /// <summary>The line feed sequence (<c>\n</c>).</summary>
    public static readonly string LineFeed = "\n";
    /// <summary>The carriage return sequence (<c>\r</c>).</summary>
    public static readonly string CarriageReturn = "\r";
    /// <summary>The carriage return and line feed sequence (<c>\r\n</c>).</summary>
    public static readonly string CarriageReturnLineFeed = "\r\n";

    /// <summary>Returns the character sequence for the given newline style.</summary>
    /// <param name="newLineCharacter">The newline style to resolve.</param>
    /// <returns>The matching character sequence.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is not a concrete newline style.</exception>
    public static string GetCharacterByType(NewLineCharacter newLineCharacter)
        => newLineCharacter switch
        {
            NewLineCharacter.LineFeed => LineFeed,
            NewLineCharacter.CarriageReturn => CarriageReturn,
            NewLineCharacter.CarriageReturnLineFeed => CarriageReturnLineFeed,
            _ => throw new ArgumentOutOfRangeException(nameof(newLineCharacter), newLineCharacter, null)
        };
}
