using System.Diagnostics.CodeAnalysis;

namespace Auxil.Extensions;

/// <summary>Provides cloning helpers for objects.</summary>
[PublicAPI]
public static class ObjectExtensions
{
    /// <summary>Creates a deep clone of the object, including nested objects.</summary>
    /// <returns>The cloned object, or <c>null</c> when the source is <c>null</c>.</returns>
    [return: NotNullIfNotNull(nameof(obj))]
    public static TObject? DeepClone<TObject>(this TObject obj) => FastCloner.FastCloner.DeepClone(obj);

    /// <summary>Creates a shallow clone of the object, sharing nested object references.</summary>
    /// <returns>The cloned object, or <c>null</c> when the source is <c>null</c>.</returns>
    [return: NotNullIfNotNull(nameof(obj))]
    public static TObject? ShallowClone<TObject>(this TObject obj) => FastCloner.FastCloner.ShallowClone(obj);
}
