namespace Auxil.CodeGeneration.Common.Syntax;

/// <summary>
/// How annotations (attributes/decorators) are written above declarations.
/// </summary>
[PublicAPI]
public enum AnnotationStyle
{
    /// <summary>At-sign prefix, e.g. GDScript/TypeScript "@export".</summary>
    At,

    /// <summary>Square brackets, e.g. C# "[Obsolete]".</summary>
    Square
}
