namespace Auxil.CodeGeneration.Common;

/// <summary>
/// How chained blocks (else, catch, do-while) place their braces.
/// Only affects brace-based languages; indentation languages ignore it.
/// </summary>
[PublicAPI]
public enum BraceStyle
{
    /// <summary>Each block on its own line: } newline else {</summary>
    Allman,

    /// <summary>Closing brace shares the line: } else {</summary>
    KAndR
}
