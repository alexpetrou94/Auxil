using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Rendering;

/// <summary>
/// Renders a code model through the shared renderer pipeline. The
/// language's trait list decides which nodes it can express; everything
/// else is skipped with a warning.
/// </summary>
[PublicAPI]
public static class CodeRenderer
{
    /// <summary>
    /// Renders a code model in the given language, collecting warnings
    /// for features the language cannot express.
    /// </summary>
    /// <param name="file">The model to render.</param>
    /// <param name="options">The formatting options.</param>
    /// <param name="warnings">Receives warnings for skipped features.</param>
    /// <returns>The rendered source text.</returns>
    public static string Render<TLanguage>(CodeFile file, CodeBuilderOptions options, List<string> warnings)
        where TLanguage : struct, ILanguage
    {
        CodeModelValidator.Validate(file);
        LanguageProfile profile = LanguageProfile.Get<TLanguage>();
        CodeWriter writer = new(profile.Syntax, options);
        RenderContext ctx = new(writer, profile, warnings);
        FileRenderer.RenderFile(ctx, file);
        return writer.ToString();
    }
}
