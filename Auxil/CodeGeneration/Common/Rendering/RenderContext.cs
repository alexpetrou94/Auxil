using Auxil.CodeGeneration.Common.Syntax;
using Auxil.CodeGeneration.Common.Syntax.Shapes;

namespace Auxil.CodeGeneration.Common.Rendering;

/// <summary>
/// Carries everything a renderer needs: the writer, the composed
/// language profile, and the warning sink.
/// </summary>
[PublicAPI]
public sealed class RenderContext
{
    /// <summary>Creates a render context for the given language.</summary>
    public RenderContext(CodeWriter writer, LanguageProfile profile, List<string> warnings)
    {
        Writer = writer;
        Profile = profile;
        Warnings = warnings;
    }

    /// <summary>The writer receiving the rendered source.</summary>
    public CodeWriter Writer { get; }

    /// <summary>The composed language profile.</summary>
    public LanguageProfile Profile { get; }

    /// <summary>The syntax profile in effect.</summary>
    public LanguageSyntax Syntax => Profile.Syntax;

    /// <summary>The language name, used in warnings.</summary>
    public string LanguageName => Profile.Name;

    /// <summary>The language's shape bundle.</summary>
    public Shapes Shapes => Profile.Shapes;

    /// <summary>Warnings collected during rendering.</summary>
    public List<string> Warnings { get; }

    /// <summary>
    /// The shape context for this render, built once and reused: the
    /// writer, syntax, warning sink, and the statement-renderer flow.
    /// </summary>
    public ShapeContext Shape => field ??= new ShapeContext
    {
        Writer = Writer,
        Syntax = Syntax,
        Shapes = Shapes,
        LanguageName = LanguageName,
        Warnings = Warnings,
        RenderStatements = statements => StatementRenderer.RenderStatements(this, statements)
    };
}
