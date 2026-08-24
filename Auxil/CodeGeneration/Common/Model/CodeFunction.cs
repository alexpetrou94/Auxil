namespace Auxil.CodeGeneration.Common.Model;

/// <summary>
/// A top-level function declaration (TypeScript function, Python def).
/// GDScript has no top-level functions; the renderer decides.
/// </summary>
[PublicAPI]
public sealed class CodeFunction : CodeMember
{
    /// <summary>The function name.</summary>
    public required string Name { get; init; }

    /// <summary>The function parameters in declaration order.</summary>
    public List<CodeParameter> Parameters { get; } = [];

    /// <summary>The return type, if any.</summary>
    public CodeType? ReturnType { get; set; }

    /// <summary>The generic type parameters.</summary>
    public List<CodeGenericParameter> Generics { get; } = [];

    /// <summary>The function body, if any.</summary>
    public CodeBody? Body { get; set; }
}
