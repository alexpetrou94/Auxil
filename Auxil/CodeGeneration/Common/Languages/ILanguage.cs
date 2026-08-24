using Auxil.CodeGeneration.Common.Syntax;
using Auxil.CodeGeneration.Common.Syntax.Shapes;

namespace Auxil.CodeGeneration.Common.Languages;

/// <summary>
/// Marker for a target language. A language type lists its features as
/// trait interfaces, so the builder API and the renderer both know
/// exactly what the language can express — unsupported features are
/// compile-time errors instead of runtime warnings.
/// </summary>
[PublicAPI]
public interface ILanguage
{
    /// <summary>The language name, used in warnings and diagnostics.</summary>
    static abstract string Name { get; }

    /// <summary>The language's syntax profile (tokens, styles, type map).</summary>
    static abstract LanguageSyntax Syntax { get; }

    /// <summary>
    /// The language's shape bundle: how each construct looks. Defaults
    /// to the shared implementations driven by the syntax profile; a
    /// language overrides the hooks by declaring its own Shapes member
    /// (implicit interface implementation, no override keyword).
    /// </summary>
    static virtual Shapes Shapes => Shapes.Default;
}
