namespace Auxil.CodeGeneration.Common.Model.Statements;

/// <summary>
/// An object literal statement (TypeScript object literal). The prefix
/// is leading text written before the opening brace (e.g. "return" for
/// "return { ... };"); members are comma-separated and the closing brace
/// carries the statement terminator. Members can be raw expressions,
/// nested object literals, or function values.
/// </summary>
[PublicAPI]
public sealed class CodeObjectLiteral : CodeStatement
{
    /// <summary>Leading text before the opening brace, if any (e.g. "return").</summary>
    public string? Prefix { get; set; }

    /// <summary>The literal members in declaration order.</summary>
    public List<CodeObjectMember> Members { get; } = [];
}

/// <summary>
/// A member of an object literal: a name and exactly one value kind —
/// a raw expression, a nested object literal, or a function value.
/// </summary>
[PublicAPI]
public sealed class CodeObjectMember
{
    /// <summary>The member name, written verbatim.</summary>
    public required string Name { get; init; }

    /// <summary>The member value as a raw expression, if any.</summary>
    public string? Expression { get; set; }

    /// <summary>The member value as a nested object literal, if any.</summary>
    public CodeObjectLiteral? Object { get; set; }

    /// <summary>The member value as a function, if any.</summary>
    public CodeNestedFunction? Function { get; set; }
}
