namespace Auxil.CodeGeneration.Common.Model.Statements;

/// <summary>
/// A nested block with a verbatim header. The header is written exactly
/// as given (no keyword injected); the body renders one level deeper and
/// the block closes with the language's closing token, optionally
/// followed by a suffix (e.g. ";" for a TypeScript object literal
/// statement, "," for an object-literal member).
/// </summary>
[PublicAPI]
public sealed class CodeRawBlock : CodeStatement
{
    /// <summary>The block header, written exactly as given.</summary>
    public required string Header { get; init; }

    /// <summary>The block body statements.</summary>
    public List<CodeStatement> Body { get; } = [];

    /// <summary>
    /// Text appended to the closing token, or null to close bare
    /// (e.g. "}" for a function declaration).
    /// </summary>
    public string? CloseSuffix { get; init; }
}
