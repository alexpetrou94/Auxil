namespace Auxil.CodeGeneration.Common.Model.Statements;

/// <summary>
/// A comment inside a body, rendered with the language's line comment
/// prefix.
/// </summary>
[PublicAPI]
public sealed class CodeCommentStatement : CodeStatement
{
    /// <summary>The comment text.</summary>
    public required string Text { get; init; }
}
