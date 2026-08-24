using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// Trait-gated modifier members. Each block is constrained on the
/// feature it implements, so a language without the modifier does not
/// offer it — using it is a compile-time error.
/// </summary>
[PublicAPI]
public static class DeclarationScopeFeatures
{
    extension<TLanguage, TScope, TParent>(DeclarationScope<TLanguage, TScope, TParent> scope)
        where TLanguage : struct, ILanguage, IHasVisibilityModifiers
        where TScope : DeclarationScope<TLanguage, TScope, TParent>
        where TParent : IScope
    {
        /// <summary>Adds the public modifier.</summary>
        public TScope Public() => scope.AddModifier(CodeModifier.Public);

        /// <summary>Adds the private modifier.</summary>
        public TScope Private() => scope.AddModifier(CodeModifier.Private);

        /// <summary>Adds the protected modifier.</summary>
        public TScope Protected() => scope.AddModifier(CodeModifier.Protected);

        /// <summary>Adds the internal modifier.</summary>
        public TScope Internal() => scope.AddModifier(CodeModifier.Internal);
    }

    extension<TLanguage, TScope, TParent>(DeclarationScope<TLanguage, TScope, TParent> scope)
        where TLanguage : struct, ILanguage, IHasSealed
        where TScope : DeclarationScope<TLanguage, TScope, TParent>
        where TParent : IScope
    {
        /// <summary>Adds the sealed modifier.</summary>
        public TScope Sealed() => scope.AddModifier(CodeModifier.Sealed);
    }

    extension<TLanguage, TScope, TParent>(DeclarationScope<TLanguage, TScope, TParent> scope)
        where TLanguage : struct, ILanguage, IHasVirtual
        where TScope : DeclarationScope<TLanguage, TScope, TParent>
        where TParent : IScope
    {
        /// <summary>Adds the virtual modifier.</summary>
        public TScope Virtual() => scope.AddModifier(CodeModifier.Virtual);
    }

    extension<TLanguage, TScope, TParent>(DeclarationScope<TLanguage, TScope, TParent> scope)
        where TLanguage : struct, ILanguage, IHasOverride
        where TScope : DeclarationScope<TLanguage, TScope, TParent>
        where TParent : IScope
    {
        /// <summary>Adds the override modifier.</summary>
        public TScope Override() => scope.AddModifier(CodeModifier.Override);
    }

    extension<TLanguage, TScope, TParent>(DeclarationScope<TLanguage, TScope, TParent> scope)
        where TLanguage : struct, ILanguage, IHasReadonly
        where TScope : DeclarationScope<TLanguage, TScope, TParent>
        where TParent : IScope
    {
        /// <summary>Adds the readonly modifier.</summary>
        public TScope Readonly() => scope.AddModifier(CodeModifier.Readonly);
    }

    extension<TLanguage, TScope, TParent>(DeclarationScope<TLanguage, TScope, TParent> scope)
        where TLanguage : struct, ILanguage, IHasConst
        where TScope : DeclarationScope<TLanguage, TScope, TParent>
        where TParent : IScope
    {
        /// <summary>Adds the const modifier.</summary>
        public TScope Const() => scope.AddModifier(CodeModifier.Const);
    }

    extension<TLanguage, TScope, TParent>(DeclarationScope<TLanguage, TScope, TParent> scope)
        where TLanguage : struct, ILanguage, IHasAsync
        where TScope : DeclarationScope<TLanguage, TScope, TParent>
        where TParent : IScope
    {
        /// <summary>Adds the async modifier.</summary>
        public TScope Async() => scope.AddModifier(CodeModifier.Async);
    }

    extension<TLanguage, TScope, TParent>(DeclarationScope<TLanguage, TScope, TParent> scope)
        where TLanguage : struct, ILanguage, IHasExport
        where TScope : DeclarationScope<TLanguage, TScope, TParent>
        where TParent : IScope
    {
        /// <summary>Adds the export modifier.</summary>
        public TScope Export() => scope.AddModifier(CodeModifier.Export);
    }
}
