namespace Auxil.Extensions;

/// <summary>Provides convenience members for working with dictionaries.</summary>
[PublicAPI]
public static class DictionaryExtensions
{
    /// <summary>Returns the value for the key, or the default when the key is missing.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="dictionary">The dictionary to read from.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="defaultValue">The value returned when the key is missing.</param>
    public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        ArgumentNullException.ThrowIfNull(key);

        return dictionary.TryGetValue(key, out TValue? value) ? value : defaultValue;
    }

    /// <summary>Returns a new dictionary containing both dictionaries' entries.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="dictionary">The receiver dictionary.</param>
    /// <param name="mergeWith">The dictionary whose entries are merged in.</param>
    /// <param name="overrideKey">Whether the merged dictionary's values replace the receiver's on duplicate keys.</param>
    public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> mergeWith, bool overrideKey = false)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        ArgumentNullException.ThrowIfNull(mergeWith);

        Dictionary<TKey, TValue> result = new(dictionary);

        foreach (KeyValuePair<TKey, TValue> keyValue in mergeWith)
        {
            if (overrideKey)
            {
                result[keyValue.Key] = mergeWith[keyValue.Key];
                continue;
            }

            result.TryAdd(keyValue.Key, keyValue.Value);
        }

        return result;
    }
}
