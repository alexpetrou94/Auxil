namespace Auxil.CodeGeneration.Common.Model;

/// <summary>
/// Base class for declarative model nodes (class, method, field, signal, ...)
/// that carry annotations, modifiers, and a documentation comment.
/// </summary>
[PublicAPI]
public abstract class CodeMember : CodeNode
{
    /// <summary>Annotations (attributes/decorators) applied to the declaration.</summary>
    public List<CodeAnnotation> Annotations { get; } = [];

    /// <summary>The documentation comment rendered above the declaration, if any.</summary>
    public CodeComment? DocComment { get; set; }

    /// <summary>The declaration modifiers.</summary>
    public CodeModifier Modifiers { get; set; }
}
