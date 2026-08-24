namespace Auxil.CodeGeneration.Common.Model;

/// <summary>
/// A declaration annotation: renders as @export (GDScript),
/// @Injectable (TypeScript decorator), [Serializable] (C#), etc.
/// </summary>
[PublicAPI]
public sealed class CodeAnnotation
{
    /// <summary>The annotation name, without the leading symbol.</summary>
    public required string Name { get; init; }

    /// <summary>Arguments rendered inside the annotation's parentheses.</summary>
    public List<string> Arguments { get; init; } = [];
}
