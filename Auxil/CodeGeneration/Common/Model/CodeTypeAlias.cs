namespace Auxil.CodeGeneration.Common.Model;

/// <summary>
/// A named type alias declaration: the name (possibly with generics) and
/// the definition are written exactly as given — no parsing or
/// validation. TypeScript: "type Name = Definition".
/// </summary>
[PublicAPI]
public sealed class CodeTypeAlias : CodeMember
{
    /// <summary>The alias name, written verbatim (may include generics).</summary>
    public required string Name { get; init; }

    /// <summary>The aliased definition, written verbatim.</summary>
    public required string Definition { get; init; }
}
