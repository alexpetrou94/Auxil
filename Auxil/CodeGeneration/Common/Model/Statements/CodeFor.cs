namespace Auxil.CodeGeneration.Common.Model.Statements;

/// <summary>
/// A C-style for loop: for (initializer; condition; increment).
/// Languages without C-style for (GDScript) skip it with a warning.
/// </summary>
[PublicAPI]
public sealed class CodeFor : CodeStatement
{
    /// <summary>The initializer clause, as a raw expression.</summary>
    public required string Initializer { get; init; }

    /// <summary>The condition clause, as a raw expression.</summary>
    public required string Condition { get; init; }

    /// <summary>The increment clause, as a raw expression.</summary>
    public required string Increment { get; init; }

    /// <summary>The loop body statements.</summary>
    public List<CodeStatement> Body { get; } = [];
}
