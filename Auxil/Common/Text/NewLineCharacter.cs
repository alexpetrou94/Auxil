namespace Auxil.Common.Text;

/// <summary>Identifies the newline style used in a piece of text.</summary>
[PublicAPI]
public enum NewLineCharacter
{
    /// <summary>No newline characters present.</summary>
    None,
    /// <summary>Line feed only (<c>\n</c>).</summary>
    LineFeed,
    /// <summary>Carriage return only (<c>\r</c>).</summary>
    CarriageReturn,
    /// <summary>Carriage return followed by line feed (<c>\r\n</c>).</summary>
    CarriageReturnLineFeed,
    /// <summary>More than one newline style present.</summary>
    Mixed
}
