namespace Auxil.CodeGeneration.Common.Model;

/// <summary>
/// The body of a method, constructor, or function: an ordered list of
/// statements. An optional Id marks this body as a customization point
/// for later replacement (scaffold-and-customize workflows).
/// </summary>
[PublicAPI]
public sealed class CodeBody : CodeNode
{
    /// <summary>An optional id for surgical editing.</summary>
    public string? Id { get; set; }

    /// <summary>The statements in execution order.</summary>
    public List<CodeStatement> Statements { get; } = [];
}
