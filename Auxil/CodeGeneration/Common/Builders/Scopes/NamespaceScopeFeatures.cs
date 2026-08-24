using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// Trait-gated namespace scope members. Each block is constrained on the
/// feature it implements, so a language without the feature does not
/// offer the member — using it is a compile-time error.
/// </summary>
[PublicAPI]
public static class NamespaceScopeFeatures
{
    extension<TLanguage, TParent>(NamespaceScope<TLanguage, TParent> scope)
        where TLanguage : struct, ILanguage, IHasInterfaces
        where TParent : IScope
    {
        /// <summary>Adds an interface declaration to the namespace.</summary>
        /// <param name="name">The interface name.</param>
        public InterfaceScope<TLanguage, NamespaceScope<TLanguage, TParent>> Interface(string name)
        {
            CodeInterface iface = new() { Name = name };
            scope.NamespaceNode.Children.Add(iface);
            return new InterfaceScope<TLanguage, NamespaceScope<TLanguage, TParent>>(scope.Builder, iface, scope);
        }
    }
}
