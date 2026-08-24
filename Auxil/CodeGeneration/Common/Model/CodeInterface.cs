namespace Auxil.CodeGeneration.Common.Model;

/// <summary>
/// An interface declaration.
/// </summary>
[PublicAPI]
public sealed class CodeInterface : CodeMember
{
    /// <summary>The interface name.</summary>
    public required string Name { get; init; }

    /// <summary>Base interface names the interface extends.</summary>
    public List<string> ExtendsTypes { get; } = [];

    /// <summary>The generic type parameters.</summary>
    public List<CodeGenericParameter> Generics { get; } = [];

    /// <summary>The interface members in declaration order.</summary>
    public List<CodeNode> Members { get; } = [];
}
