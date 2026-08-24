namespace Auxil.CodeGeneration.Common.Model.Statements;

/// <summary>
/// A throw statement. The value is a raw expression. Languages without
/// exceptions (GDScript) skip it with a warning.
/// </summary>
[PublicAPI]
public sealed class CodeThrow : CodeStatement
{
    /// <summary>The thrown value, as a raw expression.</summary>
    public required string Value { get; init; }
}
