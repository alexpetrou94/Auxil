using Auxil.CodeGeneration.Common.Syntax;

namespace Auxil.CodeGeneration.Common.Languages;

/// <summary>
/// TypeScript: brace blocks, // comments, /** */ doc comments, semicolons,
/// imports, interfaces, top-level functions, generics, full modifiers.
/// </summary>
[PublicAPI]
public struct TypeScript : ILanguage,
    IHasImports, IHasInterfaces, IHasTopLevelFunctions, IHasGenerics, IHasConstructors,
    IHasImplements, IHasExceptionHandling, IHasDoWhile, IHasCStyleFor, IHasThrow,
    IHasMatch, IHasVisibilityModifiers, IHasOverride, IHasReadonly, IHasConst,
    IHasAsync, IHasExport, IHasTypeAliases, IHasNestedFunctions, IHasObjectLiterals
{
    /// <inheritdoc/>
    public static string Name => "TypeScript";

    /// <inheritdoc/>
    public static LanguageSyntax Syntax => TypeScriptSyntax.Create();
}
