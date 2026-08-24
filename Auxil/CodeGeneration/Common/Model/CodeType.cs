namespace Auxil.CodeGeneration.Common.Model;

/// <summary>
/// A type reference with a canonical name.
/// Language renderers map the canonical name through the language's
/// type map (e.g. "string" -> String in GDScript, string in TypeScript).
/// Unknown names (custom classes, engine types) pass through unchanged.
/// </summary>
[PublicAPI]
public sealed record CodeType(string Name)
{
    /// <summary>Creates a type reference from a canonical type name.</summary>
    /// <param name="name">The canonical type name.</param>
    public static CodeType Of(string name)
    {
        return new CodeType(name);
    }

    /// <summary>The canonical type name.</summary>
    public override string ToString()
    {
        return Name;
    }
}

/// <summary>
/// Canonical built-in type names understood by the type maps.
/// </summary>
[PublicAPI]
public static class CodeTypes
{
    /// <summary>The string type.</summary>
    public static CodeType String => new("string");

    /// <summary>The int type.</summary>
    public static CodeType Int => new("int");

    /// <summary>The float type.</summary>
    public static CodeType Float => new("float");

    /// <summary>The double type.</summary>
    public static CodeType Double => new("double");

    /// <summary>The bool type.</summary>
    public static CodeType Bool => new("bool");

    /// <summary>The void type.</summary>
    public static CodeType Void => new("void");

    /// <summary>The object type.</summary>
    public static CodeType Object => new("object");

    /// <summary>The array type.</summary>
    public static CodeType Array => new("array");

    /// <summary>The dictionary/map type.</summary>
    public static CodeType Dictionary => new("dictionary");

    /// <summary>The null type.</summary>
    public static CodeType Null => new("null");

    /// <summary>The dynamic/untyped type.</summary>
    public static CodeType Dynamic => new("dynamic");
}
