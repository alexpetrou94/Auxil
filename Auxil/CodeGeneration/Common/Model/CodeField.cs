namespace Auxil.CodeGeneration.Common.Model;

/// <summary>
/// A field/variable declaration: var/const (GDScript),
/// const/let/var (TypeScript), field (C#).
/// </summary>
[PublicAPI]
public sealed class CodeField : CodeMember
{
    /// <summary>The field name.</summary>
    public required string Name { get; init; }

    /// <summary>The field type, if any.</summary>
    public CodeType? Type { get; set; }

    /// <summary>
    /// Whether the field may be absent. TypeScript renders it as
    /// name?: T; C# renders the type as nullable (T?).
    /// </summary>
    public bool Nullable { get; set; }

    /// <summary>An optional default value, as a raw expression.</summary>
    public string? DefaultValue { get; set; }
}
