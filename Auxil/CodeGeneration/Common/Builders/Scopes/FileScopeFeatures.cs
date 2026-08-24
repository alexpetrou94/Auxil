using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// Trait-gated file scope members. Each block is constrained on the
/// feature it implements, so a language without the feature does not
/// offer the member — using it is a compile-time error.
/// </summary>
[PublicAPI]
public static class FileScopeFeatures
{
    extension<TLanguage>(FileScope<TLanguage> scope) where TLanguage : struct, ILanguage, IHasScriptClass
    {
        /// <summary>Sets the file's class name (GDScript class_name).</summary>
        /// <param name="name">The class name.</param>
        public FileScope<TLanguage> ClassName(string name)
        {
            scope.FileNode.ClassName = name;
            return scope;
        }

        /// <summary>Sets the file's base class (GDScript extends).</summary>
        /// <param name="baseType">The base class name.</param>
        public FileScope<TLanguage> Extends(string baseType)
        {
            scope.FileNode.ExtendsType = baseType;
            return scope;
        }
    }

    extension<TLanguage>(FileScope<TLanguage> scope) where TLanguage : struct, ILanguage, IHasImports
    {
        /// <summary>Adds an import/using directive.</summary>
        /// <param name="from">The imported module, namespace, or path.</param>
        /// <param name="defaultName">An optional default/alias name.</param>
        /// <param name="named">Named members to import (TypeScript).</param>
        public FileScope<TLanguage> Import(string from, string? defaultName = null, params string[] named)
        {
            CodeImport import = new() { From = from, DefaultName = defaultName };
            import.NamedNames.AddRange(named);
            scope.FileNode.Children.Add(import);
            return scope;
        }
    }

    extension<TLanguage>(FileScope<TLanguage> scope) where TLanguage : struct, ILanguage, IHasNamespaces
    {
        /// <summary>Adds a top-level namespace declaration.</summary>
        /// <param name="name">The namespace name.</param>
        public NamespaceScope<TLanguage, FileScope<TLanguage>> Namespace(string name)
        {
            CodeNamespace namespaceNode = new() { Name = name };
            scope.FileNode.Children.Add(namespaceNode);
            return new NamespaceScope<TLanguage, FileScope<TLanguage>>(scope.Builder, namespaceNode, scope);
        }
    }

    extension<TLanguage>(FileScope<TLanguage> scope) where TLanguage : struct, ILanguage, IHasInterfaces
    {
        /// <summary>Adds a top-level interface declaration.</summary>
        /// <param name="name">The interface name.</param>
        public InterfaceScope<TLanguage, FileScope<TLanguage>> Interface(string name)
        {
            CodeInterface iface = new() { Name = name };
            scope.FileNode.Children.Add(iface);
            return new InterfaceScope<TLanguage, FileScope<TLanguage>>(scope.Builder, iface, scope);
        }
    }

    extension<TLanguage>(FileScope<TLanguage> scope) where TLanguage : struct, ILanguage, IHasSignals
    {
        /// <summary>Adds a top-level signal declaration.</summary>
        /// <param name="name">The signal name.</param>
        public SignalScope<TLanguage, FileScope<TLanguage>> Signal(string name)
        {
            CodeSignal signal = new() { Name = name };
            scope.FileNode.Children.Add(signal);
            return new SignalScope<TLanguage, FileScope<TLanguage>>(scope.Builder, signal, scope);
        }
    }

    extension<TLanguage>(FileScope<TLanguage> scope) where TLanguage : struct, ILanguage, IHasConstructors
    {
        /// <summary>Adds a top-level constructor.</summary>
        public ConstructorScope<TLanguage, FileScope<TLanguage>> Constructor()
        {
            CodeConstructor ctor = new();
            scope.FileNode.Children.Add(ctor);
            return new ConstructorScope<TLanguage, FileScope<TLanguage>>(scope.Builder, ctor, scope);
        }
    }

    extension<TLanguage>(FileScope<TLanguage> scope) where TLanguage : struct, ILanguage, IHasTopLevelFunctions
    {
        /// <summary>Adds a top-level function declaration.</summary>
        /// <param name="name">The function name.</param>
        public FunctionScope<TLanguage, FileScope<TLanguage>> Function(string name)
        {
            CodeFunction function = new() { Name = name };
            scope.FileNode.Children.Add(function);
            return new FunctionScope<TLanguage, FileScope<TLanguage>>(scope.Builder, function, scope);
        }
    }

    extension<TLanguage>(FileScope<TLanguage> scope) where TLanguage : struct, ILanguage, IHasTypeAliases
    {
        /// <summary>Adds a named type alias declaration.</summary>
        /// <param name="name">The alias name, written verbatim (may include generics).</param>
        /// <param name="definition">The aliased definition, written verbatim.</param>
        public TypeAliasScope<TLanguage, FileScope<TLanguage>> TypeAlias(string name, string definition)
        {
            CodeTypeAlias alias = new() { Name = name, Definition = definition };
            scope.FileNode.Children.Add(alias);
            return new TypeAliasScope<TLanguage, FileScope<TLanguage>>(scope.Builder, alias, scope);
        }
    }
}
