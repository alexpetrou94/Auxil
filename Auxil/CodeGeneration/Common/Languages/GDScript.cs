using Auxil.CodeGeneration.Common.Syntax;

namespace Auxil.CodeGeneration.Common.Languages;

/// <summary>
/// GDScript: indentation blocks, # comments, ## doc comments, signals,
/// file-as-class identity, inner classes, match, constructors, const.
/// </summary>
[PublicAPI]
public struct GDScript : ILanguage,
    IHasScriptClass, IHasSignals, IHasInnerClasses, IHasMatch, IHasConstructors, IHasConst
{
    /// <inheritdoc/>
    public static string Name => "GDScript";

    /// <inheritdoc/>
    public static LanguageSyntax Syntax => GDScriptSyntax.Create();
}
