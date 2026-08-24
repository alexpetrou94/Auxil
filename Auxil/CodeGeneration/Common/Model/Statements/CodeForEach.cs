namespace Auxil.CodeGeneration.Common.Model.Statements;

/// <summary>
/// A foreach/for-in loop over an iterable.
/// </summary>
[PublicAPI]
public sealed class CodeForEach : CodeStatement
{
    /// <summary>The loop variable name.</summary>
    public required string Variable { get; init; }

    /// <summary>The iterable, as a raw expression.</summary>
    public required string Iterable { get; init; }

    /// <summary>The loop body statements.</summary>
    public List<CodeStatement> Body { get; } = [];
}
