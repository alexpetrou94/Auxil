using System.Text.Json;

namespace Auxil.Utilities;

// The class keeps the Utilities namespace on purpose: a `Json` class inside a `.Json`
// namespace would be shadowed by the namespace at call sites (C# name resolution).
/// <summary>Provides JSON serialization helpers built on System.Text.Json.</summary>
[PublicAPI]
public static class Json
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>Serializes the value to an indented JSON string.</summary>
    /// <param name="value">The value to serialize.</param>
    /// <param name="options">Optional serializer options; defaults to indented output.</param>
    public static string Serialize<T>(T? value, JsonSerializerOptions? options = null)
        => JsonSerializer.Serialize(value, options ?? DefaultOptions);

    /// <summary>Deserializes a JSON string into a value.</summary>
    /// <param name="jsonText">The JSON text.</param>
    public static T? Deserialize<T>(string jsonText)
    {
        ArgumentNullException.ThrowIfNull(jsonText);

        return JsonSerializer.Deserialize<T>(jsonText);
    }

    /// <summary>Deserializes JSON that is either a single object or an array into a list.</summary>
    /// <param name="jsonText">The JSON text.</param>
    /// <returns>The deserialized items, or an empty list for empty, whitespace, or <c>null</c> JSON.</returns>
    public static List<T> DeserializeSingleOrList<T>(string jsonText)
    {
        ArgumentNullException.ThrowIfNull(jsonText);

        if (string.IsNullOrWhiteSpace(jsonText))
        {
            return [];
        }

        using JsonDocument document = JsonDocument.Parse(jsonText);

        if (document.RootElement.ValueKind == JsonValueKind.Array)
        {
            return JsonSerializer.Deserialize<List<T>>(jsonText)!;
        }

        if (document.RootElement.ValueKind == JsonValueKind.Object)
        {
            return [JsonSerializer.Deserialize<T>(jsonText)!];
        }

        return [];
    }
}
