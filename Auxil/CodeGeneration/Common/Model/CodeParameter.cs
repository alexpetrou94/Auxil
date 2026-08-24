namespace Auxil.CodeGeneration.Common.Model;

/// <summary>
/// A method/function/constructor/signal parameter.
/// </summary>
[PublicAPI]
public sealed class CodeParameter : CodeNode
{
    /// <summary>The parameter name.</summary>
    public required string Name { get; init; }

    /// <summary>The parameter type, if any.</summary>
    public CodeType? Type { get; set; }

    /// <summary>
    /// Whether the parameter may be absent. TypeScript renders it as
    /// p?: T; C# renders the type as nullable (T?).
    /// </summary>
    public bool Nullable { get; set; }

    /// <summary>An optional default value, as a raw expression.</summary>
    public string? DefaultValue { get; set; }
}
