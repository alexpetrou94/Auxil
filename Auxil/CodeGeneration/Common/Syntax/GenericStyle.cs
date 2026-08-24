namespace Auxil.CodeGeneration.Common.Syntax;

/// <summary>
/// How generic type parameters are written.
/// </summary>
[PublicAPI]
public enum GenericStyle
{
    /// <summary>Inline constraints, e.g. TypeScript "T extends Base = unknown".</summary>
    Inline,

    /// <summary>Where-clauses after the signature, e.g. C# "where T : class".</summary>
    WhereClauses,

    /// <summary>The language has no generics.</summary>
    None
}
