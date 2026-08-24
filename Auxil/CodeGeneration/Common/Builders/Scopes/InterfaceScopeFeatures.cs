using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// Trait-gated interface scope members. Each block is constrained on the
/// feature it implements, so a language without the feature does not
/// offer the member — using it is a compile-time error.
/// </summary>
[PublicAPI]
public static class InterfaceScopeFeatures
{
    extension<TLanguage, TParent>(InterfaceScope<TLanguage, TParent> scope)
        where TLanguage : struct, ILanguage, IHasGenerics
        where TParent : IScope
    {
        /// <summary>Adds a generic type parameter to the interface.</summary>
        /// <param name="name">The type parameter name, e.g. "T".</param>
        /// <param name="constraint">An optional constraint (rendered per language).</param>
        /// <param name="defaultValue">An optional default type argument (TypeScript only).</param>
        public InterfaceScope<TLanguage, TParent> Generic(string name, string? constraint = null, string? defaultValue = null)
        {
            scope.InterfaceNode.Generics.Add(new CodeGenericParameter { Name = name, Constraint = constraint, DefaultValue = defaultValue });
            return scope;
        }
    }
}
