using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// Base for typed scope handles. Holds the builder, the model node the
/// scope manages, and the typed parent. End() returns the parent.
/// </summary>
[PublicAPI]
public abstract class ScopeBase<TLanguage, TParent> : IScope
    where TLanguage : struct, ILanguage
    where TParent : IScope
{
    private readonly TParent _parent;

    internal ScopeBase(CodeBuilder<TLanguage> builder, CodeNode node, TParent parent)
    {
        Builder = builder;
        Node = node;
        _parent = parent;
    }

    /// <summary>The builder that created this scope.</summary>
    internal CodeBuilder<TLanguage> Builder { get; }

    /// <summary>The model node this scope manages.</summary>
    protected CodeNode Node { get; }

    /// <summary>Returns to the parent scope (alias of the explicit EndX() methods).</summary>
    public TParent End()
    {
        return _parent;
    }
}
