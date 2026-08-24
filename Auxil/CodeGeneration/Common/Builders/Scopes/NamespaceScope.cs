using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// A namespace scope. Supports classes, enums, and nested namespaces.
/// Interface() is a trait-gated extension member.
/// </summary>
[PublicAPI]
public sealed class NamespaceScope<TLanguage, TParent> : ScopeBase<TLanguage, TParent>
    where TLanguage : struct, ILanguage
    where TParent : IScope
{
    internal NamespaceScope(CodeBuilder<TLanguage> builder, CodeNamespace namespaceNode, TParent parent) : base(builder, namespaceNode, parent)
    {
        NamespaceNode = namespaceNode;
    }

    internal CodeNamespace NamespaceNode { get; }

    /// <summary>Returns to the parent scope.</summary>
    public TParent EndNamespace()
    {
        return End();
    }

    /// <summary>Adds a nested namespace to the current namespace.</summary>
    /// <param name="name">The nested namespace name.</param>
    public NamespaceScope<TLanguage, NamespaceScope<TLanguage, TParent>> Namespace(string name)
    {
        CodeNamespace nested = new() { Name = name };
        NamespaceNode.Children.Add(nested);
        return new NamespaceScope<TLanguage, NamespaceScope<TLanguage, TParent>>(Builder, nested, this);
    }

    /// <summary>Adds a class declaration to the namespace.</summary>
    /// <param name="name">The class name.</param>
    public ClassScope<TLanguage, NamespaceScope<TLanguage, TParent>> Class(string name)
    {
        CodeClass cls = new() { Name = name };
        NamespaceNode.Children.Add(cls);
        return new ClassScope<TLanguage, NamespaceScope<TLanguage, TParent>>(Builder, cls, this);
    }

    /// <summary>Adds an enum declaration to the namespace.</summary>
    /// <param name="name">The enum name.</param>
    public EnumScope<TLanguage, NamespaceScope<TLanguage, TParent>> Enum(string name)
    {
        CodeEnum enm = new() { Name = name };
        NamespaceNode.Children.Add(enm);
        return new EnumScope<TLanguage, NamespaceScope<TLanguage, TParent>>(Builder, enm, this);
    }

    /// <summary>Adds a comment inside the namespace.</summary>
    /// <param name="text">The comment text.</param>
    public NamespaceScope<TLanguage, TParent> Comment(string text)
    {
        NamespaceNode.Children.Add(new CodeComment { Text = text });
        return this;
    }
}
