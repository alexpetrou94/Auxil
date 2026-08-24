using System.Text;
using System.Text.RegularExpressions;
using Auxil.Common.Text;

namespace Auxil.Extensions;

/// <summary>Provides convenience members for working with <see cref="StringBuilder"/> instances.</summary>
[PublicAPI]
public static class StringBuilderExtensions
{
    /// <summary>Appends the character sequence for the given newline style.</summary>
    /// <param name="sb">The builder to append to.</param>
    /// <param name="newLineCharacter">The newline style to append.</param>
    public static StringBuilder NewLine(this StringBuilder sb,
        NewLineCharacter newLineCharacter = NewLineCharacter.LineFeed)
        => sb.Append(Utilities.NewLine.GetCharacterByType(newLineCharacter));

    /// <summary>Appends the value followed by a newline.</summary>
    /// <param name="sb">The builder to append to.</param>
    /// <param name="value">The value to append.</param>
    /// <param name="newLineCharacter">The newline style to append.</param>
    public static StringBuilder AppendWithNewLine(this StringBuilder sb, string? value,
        NewLineCharacter newLineCharacter = NewLineCharacter.LineFeed)
        => sb.Append(value).Append(Utilities.NewLine.GetCharacterByType(newLineCharacter));

    /// <summary>Appends only a newline.</summary>
    /// <param name="sb">The builder to append to.</param>
    /// <param name="newLineCharacter">The newline style to append.</param>
    public static StringBuilder AppendWithNewLine(this StringBuilder sb,
        NewLineCharacter newLineCharacter = NewLineCharacter.LineFeed)
        => sb.Append(Utilities.NewLine.GetCharacterByType(newLineCharacter));

    /// <summary>Removes the last characters, treating a CRLF pair as a single character when requested.</summary>
    /// <param name="sb">The builder to modify.</param>
    /// <param name="count">The number of characters to remove.</param>
    /// <param name="treatEndCharacterAsOne">Whether a trailing CRLF pair counts as a single character.</param>
    public static StringBuilder RemoveLast(this StringBuilder sb, int count = 1, bool treatEndCharacterAsOne = true)
    {
        if (count < 1)
        {
            return sb;
        }

        if (count > sb.Length)
        {
            return sb.Clear();
        }

        if (treatEndCharacterAsOne)
        {
            string sbValue = sb.ToString();
            int startIndex = count * 2 > sbValue.Length ? 0 : sbValue.Length - count * 2;
            sbValue = sbValue[startIndex..];
            int numOfWindowsEndLines = Regex.Count(sbValue, Utilities.NewLine.CarriageReturnLineFeed);
            count += numOfWindowsEndLines;
        }

        sb.Length -= count;

        return sb;
    }

    /// <summary>Appends the given number of tab characters.</summary>
    /// <param name="sb">The builder to append to.</param>
    /// <param name="level">The number of tabs to append.</param>
    public static StringBuilder Tab(this StringBuilder sb, int level = 1) => sb.Append("\t".Repeat(level));

    /// <summary>Inserts the value at the start of the builder.</summary>
    /// <param name="sb">The builder to modify.</param>
    /// <param name="value">The value to insert.</param>
    public static StringBuilder Prepend(this StringBuilder sb, string? value) => sb.Insert(0, value);

    /// <summary>Returns a new builder containing a copy of the current content.</summary>
    /// <param name="sb">The builder to copy.</param>
    public static StringBuilder Duplicate(this StringBuilder sb) => new(sb.ToString());

    /// <summary>Returns the last characters without removing them.</summary>
    /// <param name="sb">The builder to inspect.</param>
    /// <param name="count">The number of characters to peek; values out of range produce an empty string.</param>
    public static string PeekBack(this StringBuilder sb, int count = 1)
    {
        ArgumentNullException.ThrowIfNull(sb);

        if (count < 1 || count > sb.Length)
        {
            return string.Empty;
        }

        return sb.ToString(sb.Length - count, count);
    }

    extension(StringBuilder sb)
    {
        /// <summary>Whether the builder contains no characters.</summary>
        public bool IsEmpty => sb.Length == 0;
    }
}
