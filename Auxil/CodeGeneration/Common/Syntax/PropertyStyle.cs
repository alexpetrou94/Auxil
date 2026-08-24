namespace Auxil.CodeGeneration.Common.Syntax;

/// <summary>
/// How a property declaration is rendered.
/// </summary>
[PublicAPI]
public enum PropertyStyle
{
    /// <summary>Auto-accessors with optional accessor blocks, e.g. C#.</summary>
    AutoAccessors,

    /// <summary>Plain field-like declaration, e.g. TypeScript.</summary>
    AsField,

    /// <summary>Variable with optional get/set blocks, e.g. GDScript.</summary>
    BlockForm
}
