using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// Trait-gated class scope members. Each block is constrained on the
/// feature it implements, so a language without the feature does not
/// offer the member — using it is a compile-time error.
/// </summary>
[PublicAPI]
public static class ClassScopeFeatures
{
    extension<TLanguage, TParent>(ClassScope<TLanguage, TParent> scope)
        where TLanguage : struct, ILanguage, IHasImplements
        where TParent : IScope
    {
        /// <summary>Adds interfaces implemented by the current class.</summary>
        /// <param name="interfaces">The interface names.</param>
        public ClassScope<TLanguage, TParent> Implements(params string[] interfaces)
        {
            scope.ClassNode.ImplementsTypes.AddRange(interfaces);
            return scope;
        }
    }

    extension<TLanguage, TParent>(ClassScope<TLanguage, TParent> scope)
        where TLanguage : struct, ILanguage, IHasGenerics
        where TParent : IScope
    {
        /// <summary>Adds a generic type parameter to the class.</summary>
        /// <param name="name">The type parameter name, e.g. "T".</param>
        /// <param name="constraint">An optional constraint (rendered per language, e.g. "class" in C# where clauses).</param>
        /// <param name="defaultValue">An optional default type argument (TypeScript only).</param>
        public ClassScope<TLanguage, TParent> Generic(string name, string? constraint = null, string? defaultValue = null)
        {
            scope.ClassNode.Generics.Add(new CodeGenericParameter { Name = name, Constraint = constraint, DefaultValue = defaultValue });
            return scope;
        }
    }

    extension<TLanguage, TParent>(ClassScope<TLanguage, TParent> scope)
        where TLanguage : struct, ILanguage, IHasInnerClasses
        where TParent : IScope
    {
        /// <summary>Adds an inner class to the current class.</summary>
        /// <param name="name">The inner class name.</param>
        public ClassScope<TLanguage, ClassScope<TLanguage, TParent>> Class(string name)
        {
            CodeClass cls = new() { Name = name };
            scope.ClassNode.Members.Add(cls);
            return new ClassScope<TLanguage, ClassScope<TLanguage, TParent>>(scope.Builder, cls, scope);
        }
    }

    extension<TLanguage, TParent>(ClassScope<TLanguage, TParent> scope)
        where TLanguage : struct, ILanguage, IHasInterfaces
        where TParent : IScope
    {
        /// <summary>Adds an interface declaration to the current class.</summary>
        /// <param name="name">The interface name.</param>
        public InterfaceScope<TLanguage, ClassScope<TLanguage, TParent>> Interface(string name)
        {
            CodeInterface iface = new() { Name = name };
            scope.ClassNode.Members.Add(iface);
            return new InterfaceScope<TLanguage, ClassScope<TLanguage, TParent>>(scope.Builder, iface, scope);
        }
    }

    extension<TLanguage, TParent>(ClassScope<TLanguage, TParent> scope)
        where TLanguage : struct, ILanguage, IHasSignals
        where TParent : IScope
    {
        /// <summary>Adds a signal declaration to the current class.</summary>
        /// <param name="name">The signal name.</param>
        public SignalScope<TLanguage, ClassScope<TLanguage, TParent>> Signal(string name)
        {
            CodeSignal signal = new() { Name = name };
            scope.ClassNode.Members.Add(signal);
            return new SignalScope<TLanguage, ClassScope<TLanguage, TParent>>(scope.Builder, signal, scope);
        }
    }

    extension<TLanguage, TParent>(ClassScope<TLanguage, TParent> scope)
        where TLanguage : struct, ILanguage, IHasConstructors
        where TParent : IScope
    {
        /// <summary>Adds a constructor to the current class.</summary>
        public ConstructorScope<TLanguage, ClassScope<TLanguage, TParent>> Constructor()
        {
            CodeConstructor ctor = new();
            scope.ClassNode.Members.Add(ctor);
            return new ConstructorScope<TLanguage, ClassScope<TLanguage, TParent>>(scope.Builder, ctor, scope);
        }
    }
}
