namespace Auxil.CodeGeneration.Common.Model.Statements;

/// <summary>
/// An if statement with optional elif branches and an optional else block.
/// Conditions are raw expressions.
/// </summary>
[PublicAPI]
public sealed class CodeIf : CodeStatement
{
    /// <summary>The condition, as a raw expression.</summary>
    public required string Condition { get; init; }

    /// <summary>The then-block statements.</summary>
    public List<CodeStatement> Then { get; } = [];

    /// <summary>The elif branches, in order.</summary>
    public List<CodeElseIf> ElseIfs { get; } = [];

    /// <summary>The else-block statements, if any.</summary>
    public List<CodeStatement> Else { get; } = [];
}

/// <summary>
/// An elif/else-if branch of a CodeIf.
/// </summary>
[PublicAPI]
public sealed class CodeElseIf
{
    /// <summary>The branch condition, as a raw expression.</summary>
    public required string Condition { get; init; }

    /// <summary>The branch statements.</summary>
    public List<CodeStatement> Body { get; } = [];
}
