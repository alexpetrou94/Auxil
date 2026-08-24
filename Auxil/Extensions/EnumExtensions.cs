using System.Collections.Concurrent;
using System.Reflection;

namespace Auxil.Extensions;

/// <summary>Provides convenience members for working with enums.</summary>
[PublicAPI]
public static class EnumExtensions
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, string>> StringValueCache = new();

    /// <summary>Returns the <see cref="StringValueAttribute"/> value for the enum member, or an empty string when absent.</summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="enum">The enum member.</param>
    public static string GetStringValue<TEnum>(this TEnum @enum) where TEnum : struct, Enum
    {
        IReadOnlyDictionary<string, string> stringValues = StringValueCache.GetOrAdd(typeof(TEnum), BuildStringValueMap);

        return stringValues.TryGetValue(@enum.ToString(), out string? value) ? value : string.Empty;
    }

    /// <summary>Returns the underlying <see cref="int"/> value of the enum member.</summary>
    /// <param name="enum">The enum member.</param>
    public static int ToInt(this Enum @enum) => Convert.ToInt32(@enum);

    private static IReadOnlyDictionary<string, string> BuildStringValueMap(Type enumType)
    {
        Dictionary<string, string> map = [];

        foreach (FieldInfo field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            StringValueAttribute? attribute = field.GetCustomAttribute<StringValueAttribute>();

            map[field.Name] = attribute?.StringValue ?? string.Empty;
        }

        return map;
    }
}
