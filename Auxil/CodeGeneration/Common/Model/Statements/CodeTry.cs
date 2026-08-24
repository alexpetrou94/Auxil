namespace Auxil.CodeGeneration.Common.Model.Statements;

/// <summary>
/// A try statement with catch blocks and an optional finally block.
/// Languages without exception handling (GDScript) skip it with a warning.
/// </summary>
[PublicAPI]
public sealed class CodeTry : CodeStatement
{
    /// <summary>The try-block statements.</summary>
    public List<CodeStatement> Body { get; } = [];

    /// <summary>The catch blocks in order.</summary>
    public List<CodeCatch> Catches { get; } = [];

    /// <summary>The finally-block statements, if any.</summary>
    public List<CodeStatement> Finally { get; } = [];
}

/// <summary>
/// A catch block of a CodeTry.
/// </summary>
[PublicAPI]
public sealed class CodeCatch
{
    /// <summary>The exception type, or null for an untyped catch.</summary>
    public CodeType? Type { get; init; }

    /// <summary>The exception variable name, or null for a bare catch.</summary>
    public string? Variable { get; init; }

    /// <summary>The catch-block statements.</summary>
    public List<CodeStatement> Body { get; } = [];
}
