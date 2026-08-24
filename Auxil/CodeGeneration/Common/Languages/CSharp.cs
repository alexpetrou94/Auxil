using Auxil.CodeGeneration.Common.Syntax;

namespace Auxil.CodeGeneration.Common.Languages;

/// <summary>
/// C#: brace blocks, // comments, /// XML doc comments, semicolons,
/// namespaces, usings, interfaces, generics, full modifier set,
/// where-clause constraints.
/// </summary>
[PublicAPI]
public struct CSharp : ILanguage,
    IHasImports, IHasNamespaces, IHasInterfaces, IHasGenerics, IHasConstructors,
    IHasImplements, IHasExceptionHandling, IHasDoWhile, IHasCStyleFor, IHasThrow,
    IHasMatch, IHasInnerClasses, IHasVisibilityModifiers, IHasSealed, IHasVirtual,
    IHasOverride, IHasReadonly, IHasConst, IHasAsync, IHasNestedFunctions
{
    /// <inheritdoc/>
    public static string Name => "C#";

    /// <inheritdoc/>
    public static LanguageSyntax Syntax => CSharpSyntax.Create();
}
