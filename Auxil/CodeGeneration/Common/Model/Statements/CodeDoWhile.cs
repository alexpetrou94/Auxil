namespace Auxil.CodeGeneration.Common.Model.Statements;

/// <summary>
/// A do/while loop. Languages without do-while (GDScript) skip it
/// with a warning.
/// </summary>
[PublicAPI]
public sealed class CodeDoWhile : CodeStatement
{
    /// <summary>The condition, as a raw expression.</summary>
    public required string Condition { get; init; }

    /// <summary>The loop body statements.</summary>
    public List<CodeStatement> Body { get; } = [];
}
