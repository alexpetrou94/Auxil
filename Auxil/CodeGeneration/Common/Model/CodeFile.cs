namespace Auxil.CodeGeneration.Common.Model;

/// <summary>
/// The root of the code model. Children are rendered in order.
/// ClassName/ExtendsType model GDScript's "a file is a class"
/// (class_name + extends); other languages decide how to use them.
/// </summary>
[PublicAPI]
public sealed class CodeFile : CodeNode
{
    /// <summary>The file name, used for the model only.</summary>
    public required string FileName { get; init; }

    /// <summary>The file's class name (GDScript class_name), if any.</summary>
    public string? ClassName { get; set; }

    /// <summary>The file's base class (GDScript extends), if any.</summary>
    public string? ExtendsType { get; set; }

    /// <summary>The file's documentation comment, if any.</summary>
    public CodeComment? DocComment { get; set; }

    /// <summary>File-level annotations (GDScript @icon, @tool).</summary>
    public List<CodeAnnotation> Annotations { get; } = [];

    /// <summary>The top-level nodes in render order.</summary>
    public List<CodeNode> Children { get; } = [];
}
