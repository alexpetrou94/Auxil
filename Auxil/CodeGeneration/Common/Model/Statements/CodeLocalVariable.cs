namespace Auxil.CodeGeneration.Common.Model.Statements;

/// <summary>
/// A local variable declaration inside a body: var/let/const with an
/// optional type and initializer.
/// </summary>
[PublicAPI]
public sealed class CodeLocalVariable : CodeStatement
{
    /// <summary>The variable name.</summary>
    public required string Name { get; init; }

    /// <summary>The variable type, if any.</summary>
    public CodeType? Type { get; init; }

    /// <summary>An optional initializer, as a raw expression.</summary>
    public string? Value { get; init; }

    /// <summary>Whether this is a constant declaration.</summary>
    public bool IsConst { get; init; }
}
