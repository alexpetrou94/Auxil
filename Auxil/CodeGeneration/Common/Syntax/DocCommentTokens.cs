namespace Auxil.CodeGeneration.Common.Syntax;

/// <summary>
/// Tokens used to render a documentation comment.
/// Open/Close are optional; LinePrefix is applied to every content line.
/// </summary>
[PublicAPI]
public sealed record DocCommentTokens(string? Open, string LinePrefix, string? Close)
{
    /// <summary>The Javadoc-style tokens: /** ... */.</summary>
    public static DocCommentTokens JavadocStyle => new("/**", " * ", " */");

    /// <summary>Creates tokens that prefix every doc line (e.g. "## " or "/// ").</summary>
    /// <param name="prefix">The per-line prefix.</param>
    public static DocCommentTokens LinePrefixStyle(string prefix)
    {
        return new DocCommentTokens(null, prefix, null);
    }
}
