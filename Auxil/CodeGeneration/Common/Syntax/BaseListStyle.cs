namespace Auxil.CodeGeneration.Common.Syntax;

/// <summary>
/// How a declaration lists its base types (class base / implemented
/// interfaces). Colon writes ": A, B"; Extends writes "extends A"
/// and "implements B".
/// </summary>
[PublicAPI]
public enum BaseListStyle
{
    /// <summary>Colon-separated base list, e.g. C# ": IRepo".</summary>
    Colon,

    /// <summary>Keyword-based, e.g. TypeScript/GDScript "extends Node".</summary>
    Extends
}
