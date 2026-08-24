namespace Auxil.CodeGeneration.Common.Model.Statements;

/// <summary>
/// A return statement with an optional value.
/// </summary>
[PublicAPI]
public sealed class CodeReturn : CodeStatement
{
    /// <summary>An optional return value, as a raw expression.</summary>
    public string? Value { get; init; }
}
