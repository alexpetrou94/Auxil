namespace Auxil.CodeGeneration.Common;

/// <summary>
/// Formatting options shared by all languages.
/// </summary>
[PublicAPI]
public class CodeBuilderOptions
{
    /// <summary>The string used for one indentation level.</summary>
    public string Indent { get; set; } = "    ";

    /// <summary>The line ending written after every line.</summary>
    public string LineEnding { get; set; } = "\n";

    /// <summary>
    /// Statement terminator override: true = always ";", false = never.
    /// null (default) = use the language profile's default.
    /// </summary>
    public bool? Semicolons { get; set; }

    /// <summary>
    /// How chained blocks place their braces (brace-based languages only).
    /// </summary>
    public BraceStyle BraceStyle { get; set; } = BraceStyle.Allman;
}
