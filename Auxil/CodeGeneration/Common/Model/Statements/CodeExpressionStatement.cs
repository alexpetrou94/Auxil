namespace Auxil.CodeGeneration.Common.Model.Statements;

/// <summary>
/// A raw, verbatim line of code. Written exactly as given, with no
/// statement terminator added.
/// </summary>
[PublicAPI]
public sealed class CodeExpressionStatement : CodeStatement
{
    /// <summary>The verbatim line of code.</summary>
    public required string Line { get; init; }
}
