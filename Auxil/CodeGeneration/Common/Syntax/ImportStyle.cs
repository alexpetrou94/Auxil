namespace Auxil.CodeGeneration.Common.Syntax;

/// <summary>
/// How import/using directives are written.
/// </summary>
[PublicAPI]
public enum ImportStyle
{
    /// <summary>Using directives, e.g. C# "using X" / "using A = X".</summary>
    Using,

    /// <summary>Import-from directives, e.g. TypeScript "import { x } from \"y\"".</summary>
    From
}
