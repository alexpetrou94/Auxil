namespace Auxil.CodeGeneration.Common.Model.Statements;

/// <summary>
/// A while loop. The condition is a raw expression.
/// </summary>
[PublicAPI]
public sealed class CodeWhile : CodeStatement
{
    /// <summary>The condition, as a raw expression.</summary>
    public required string Condition { get; init; }

    /// <summary>The loop body statements.</summary>
    public List<CodeStatement> Body { get; } = [];
}
