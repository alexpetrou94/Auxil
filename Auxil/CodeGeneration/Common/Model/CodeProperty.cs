namespace Auxil.CodeGeneration.Common.Model;

/// <summary>
/// A property declaration with optional accessor bodies.
/// C# renders auto-properties ({ get; set; }) or block accessors;
/// GDScript renders get:/set(value): blocks when bodies are present,
/// otherwise a plain field; TypeScript renders a field.
/// </summary>
[PublicAPI]
public sealed class CodeProperty : CodeMember
{
    /// <summary>The property name.</summary>
    public required string Name { get; init; }

    /// <summary>The property type, if any.</summary>
    public CodeType? Type { get; set; }

    /// <summary>
    /// Whether the property may be absent. TypeScript renders it as
    /// name?: T; C# renders the type as nullable (T?).
    /// </summary>
    public bool Nullable { get; set; }

    /// <summary>Whether the property has a getter.</summary>
    public bool HasGetter { get; set; } = true;

    /// <summary>Whether the property has a setter.</summary>
    public bool HasSetter { get; set; } = true;

    /// <summary>The getter body, if any (turns an auto-property into a block accessor).</summary>
    public CodeBody? GetterBody { get; set; }

    /// <summary>The setter body, if any (turns an auto-property into a block accessor).</summary>
    public CodeBody? SetterBody { get; set; }
}
