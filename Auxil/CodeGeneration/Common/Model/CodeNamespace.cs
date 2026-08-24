namespace Auxil.CodeGeneration.Common.Model;

/// <summary>
/// A namespace declaration: file-scoped "namespace X;" in C#.
/// Languages without namespaces (GDScript, TypeScript) skip it with a warning.
/// </summary>
[PublicAPI]
public sealed class CodeNamespace : CodeNode
{
    /// <summary>The namespace name.</summary>
    public required string Name { get; init; }

    /// <summary>The namespace contents in declaration order.</summary>
    public List<CodeNode> Children { get; } = [];
}
