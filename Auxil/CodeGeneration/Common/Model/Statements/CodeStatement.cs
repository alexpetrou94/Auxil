namespace Auxil.CodeGeneration.Common.Model.Statements;

/// <summary>
/// Base class for statements inside a CodeBody. Statements are
/// language-agnostic; renderers map each kind to the target language's
/// syntax. An optional Id marks a statement as a customization point
/// for surgical editing (ReplaceStatement/InsertBefore/Remove).
/// </summary>
[PublicAPI]
public abstract class CodeStatement
{
    /// <summary>An optional id for surgical editing.</summary>
    public string? Id { get; set; }
}
