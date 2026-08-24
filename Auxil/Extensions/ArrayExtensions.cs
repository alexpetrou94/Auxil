namespace Auxil.Extensions;

/// <summary>Provides convenience members for working with arrays.</summary>
[PublicAPI]
public static class ArrayExtensions
{
    /// <summary>Returns a new array with the element appended.</summary>
    /// <param name="array">The source array.</param>
    /// <param name="element">The element to append.</param>
    public static T[] Add<T>(this T[] array, T element)
    {
        ArgumentNullException.ThrowIfNull(array);

        return [.. array, element];
    }

    /// <summary>Returns a new array with the elements appended.</summary>
    /// <param name="array">The source array.</param>
    /// <param name="args">The elements to append.</param>
    public static T[] Add<T>(this T[] array, params T[] args)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentNullException.ThrowIfNull(args);

        return [.. array, .. args];
    }

    /// <summary>Returns a new array with the other array's elements appended.</summary>
    /// <param name="array">The source array.</param>
    /// <param name="otherArray">The array whose elements are appended.</param>
    public static T[] AddRange<T>(this T[] array, T[] otherArray)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentNullException.ThrowIfNull(otherArray);

        return [.. array, .. otherArray];
    }

    /// <summary>Returns the characters as an array of single-character strings.</summary>
    /// <param name="charArray">The characters to convert.</param>
    public static string[] ToStringArray(this char[] charArray)
        => Array.ConvertAll(charArray, c => c.ToString());
}
