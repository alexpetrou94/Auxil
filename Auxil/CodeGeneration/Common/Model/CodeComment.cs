namespace Auxil.CodeGeneration.Common.Model;

/// <summary>
/// A comment node. Documentation comments render as ## (GDScript),
/// /** */ (TypeScript), /// (C#) depending on the language.
/// </summary>
[PublicAPI]
public sealed class CodeComment : CodeNode
{
    /// <summary>The comment text.</summary>
    public required string Text { get; init; }

    /// <summary>Whether this is a documentation comment (rendered with doc tokens).</summary>
    public bool IsDocumentation { get; init; }
}
