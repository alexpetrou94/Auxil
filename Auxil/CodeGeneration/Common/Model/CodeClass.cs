namespace Auxil.CodeGeneration.Common.Model;

/// <summary>
/// A class declaration. Members is an ordered list that may contain
/// fields, signals, enums, methods, constructors, and inner classes.
/// </summary>
[PublicAPI]
public sealed class CodeClass : CodeMember
{
    /// <summary>The class name.</summary>
    public required string Name { get; init; }

    /// <summary>The base class name, if any.</summary>
    public string? ExtendsType { get; set; }

    /// <summary>Interface names implemented by the class.</summary>
    public List<string> ImplementsTypes { get; } = [];

    /// <summary>The generic type parameters.</summary>
    public List<CodeGenericParameter> Generics { get; } = [];

    /// <summary>The class members in declaration order.</summary>
    public List<CodeNode> Members { get; } = [];
}
