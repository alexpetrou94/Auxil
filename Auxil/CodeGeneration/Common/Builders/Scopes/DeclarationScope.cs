using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// Base for scopes that manage a CodeMember declaration: adds the
/// documentation comment, annotations, and the universally supported
/// modifiers. Feature-gated modifiers (visibility, sealed, virtual,
/// override, readonly, const, async, export) are provided as trait-gated
/// extension members.
/// </summary>
[PublicAPI]
public abstract class DeclarationScope<TLanguage, TScope, TParent> : ScopeBase<TLanguage, TParent>
    where TLanguage : struct, ILanguage
    where TScope : DeclarationScope<TLanguage, TScope, TParent>
    where TParent : IScope
{
    internal DeclarationScope(CodeBuilder<TLanguage> builder, CodeNode node, TParent parent) : base(builder, node, parent)
    {
    }

    /// <summary>The declaration this scope configures.</summary>
    protected CodeMember Declaration => (CodeMember)Node;

    /// <summary>Adds a modifier flag to the declaration.</summary>
    internal TScope AddModifier(CodeModifier modifier)
    {
        Declaration.Modifiers |= modifier;
        return (TScope)this;
    }

    /// <summary>Sets the documentation comment rendered above the declaration.</summary>
    /// <param name="text">The documentation text; may span multiple lines.</param>
    public TScope Doc(string text)
    {
        Declaration.DocComment = new CodeComment { Text = text, IsDocumentation = true };
        return (TScope)this;
    }

    /// <summary>Adds an annotation (attribute/decorator) to the declaration.</summary>
    /// <param name="name">The annotation name.</param>
    /// <param name="arguments">Optional arguments rendered in the annotation's parentheses.</param>
    public TScope Annotate(string name, params string[] arguments)
    {
        Declaration.Annotations.Add(new CodeAnnotation { Name = name, Arguments = [.. arguments] });
        return (TScope)this;
    }

    /// <summary>Adds the static modifier.</summary>
    public TScope Static() => AddModifier(CodeModifier.Static);

    /// <summary>Adds the abstract modifier.</summary>
    public TScope Abstract() => AddModifier(CodeModifier.Abstract);
}
