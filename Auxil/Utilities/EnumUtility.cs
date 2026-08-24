namespace Auxil.Utilities;

/// <summary>Provides enum helper operations for a specific enum type.</summary>
/// <typeparam name="TEnum">The enum type to operate on.</typeparam>
[PublicAPI]
public static class EnumUtility<TEnum> where TEnum : struct, Enum
{
    /// <summary>Returns the names of all enum values.</summary>
    public static List<string> GetValueNames() => Enum.GetNames<TEnum>().ToList();

    /// <summary>Returns the names of all enum values except the given ones.</summary>
    /// <param name="exclude">The values to exclude, or <c>null</c> to include all.</param>
    public static List<string> GetValueNames(IEnumerable<TEnum>? exclude)
    {
        if (exclude is null)
        {
            return GetValueNames();
        }

        HashSet<string> excludedNames = exclude
            .Select(@enum => @enum.ToString())
            .ToHashSet();

        return Enum.GetNames<TEnum>()
            .Where(name => !excludedNames.Contains(name))
            .ToList();
    }
}
