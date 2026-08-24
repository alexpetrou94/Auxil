namespace Auxil.CodeGeneration.Common.Model.Statements;

/// <summary>
/// A match (GDScript) / switch (TypeScript, C#) statement. Patterns are
/// raw expressions; the "_" pattern maps to the default arm.
/// </summary>
[PublicAPI]
public sealed class CodeMatch : CodeStatement
{
    /// <summary>The matched expression, as a raw expression.</summary>
    public required string Expression { get; init; }

    /// <summary>The arms in order.</summary>
    public List<CodeMatchArm> Arms { get; } = [];
}

/// <summary>
/// A single match arm: a pattern and its body.
/// </summary>
[PublicAPI]
public sealed class CodeMatchArm
{
    /// <summary>The pattern, as a raw expression; "_" maps to the default arm.</summary>
    public required string Pattern { get; init; }

    /// <summary>The arm body statements.</summary>
    public List<CodeStatement> Body { get; } = [];
}
