namespace Auxil.CodeGeneration.Common.Model.Statements;

/// <summary>
/// A function declaration inside a body (TypeScript nested function,
/// C# local function). The signature is built from structured parts;
/// the body renders one level deeper.
/// </summary>
[PublicAPI]
public sealed class CodeNestedFunction : CodeStatement
{
    /// <summary>The function name.</summary>
    public required string Name { get; init; }

    /// <summary>The function parameters in declaration order.</summary>
    public List<CodeParameter> Parameters { get; } = [];

    /// <summary>The generic type parameters.</summary>
    public List<CodeGenericParameter> Generics { get; } = [];

    /// <summary>The return type, if any.</summary>
    public CodeType? ReturnType { get; set; }

    /// <summary>The declaration modifiers (e.g. async, static).</summary>
    public CodeModifier Modifiers { get; set; }

    /// <summary>The documentation comment rendered above the function, if any.</summary>
    public CodeComment? DocComment { get; set; }

    /// <summary>The function body, if any.</summary>
    public CodeBody? Body { get; set; }
}
