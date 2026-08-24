namespace Auxil.CodeGeneration.Common.Model;

/// <summary>
/// A method declaration: func (GDScript), method (TypeScript), method (C#).
/// </summary>
[PublicAPI]
public sealed class CodeMethod : CodeMember
{
    /// <summary>The method name.</summary>
    public required string Name { get; init; }

    /// <summary>The method parameters in declaration order.</summary>
    public List<CodeParameter> Parameters { get; } = [];

    /// <summary>The return type, if any.</summary>
    public CodeType? ReturnType { get; set; }

    /// <summary>The generic type parameters.</summary>
    public List<CodeGenericParameter> Generics { get; } = [];

    /// <summary>The method body, if any.</summary>
    public CodeBody? Body { get; set; }
}
