namespace Auxil.CodeGeneration.Common.Model;

/// <summary>
/// A constructor declaration: _init (GDScript), constructor (TypeScript),
/// ctor (C#). The renderer decides the exact keyword.
/// </summary>
[PublicAPI]
public sealed class CodeConstructor : CodeMember
{
    /// <summary>The constructor parameters in declaration order.</summary>
    public List<CodeParameter> Parameters { get; } = [];

    /// <summary>The constructor body, if any.</summary>
    public CodeBody? Body { get; set; }
}
