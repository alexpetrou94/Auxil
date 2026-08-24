namespace Auxil.CodeGeneration.Common.Model;

/// <summary>
/// A generic type parameter: T (TypeScript/C#), with optional
/// constraint and default value. Languages without generics
/// (GDScript) render nothing with a warning.
/// </summary>
[PublicAPI]
public sealed class CodeGenericParameter : CodeNode
{
    /// <summary>The type parameter name, e.g. "T".</summary>
    public required string Name { get; init; }

    /// <summary>An optional constraint (rendered per language).</summary>
    public string? Constraint { get; set; }

    /// <summary>An optional default type argument (TypeScript only).</summary>
    public string? DefaultValue { get; set; }
}
