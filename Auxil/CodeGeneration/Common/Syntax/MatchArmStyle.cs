namespace Auxil.CodeGeneration.Common.Syntax;

/// <summary>
/// How match arms are written.
/// </summary>
[PublicAPI]
public enum MatchArmStyle
{
    /// <summary>Case-prefixed arms with a break, e.g. TypeScript/C# switch.</summary>
    CaseWithBreak,

    /// <summary>Bare pattern followed by the header terminator, e.g. GDScript match.</summary>
    Colon
}
