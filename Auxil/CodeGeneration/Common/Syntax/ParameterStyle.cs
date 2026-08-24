namespace Auxil.CodeGeneration.Common.Syntax;

/// <summary>
/// How a parameter declaration reads: name first (TypeScript, GDScript)
/// or type first (C#, Java).
/// </summary>
[PublicAPI]
public enum ParameterStyle
{
    /// <summary>Name first: name: Type (TypeScript, GDScript).</summary>
    NameFirst,

    /// <summary>Type first: Type name (C#, Java).</summary>
    TypeFirst
}
