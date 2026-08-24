namespace Auxil.CodeGeneration.Common.Syntax;

/// <summary>
/// How a language delimits blocks: braces (C-family) or
/// significant indentation (GDScript, Python).
/// </summary>
[PublicAPI]
public enum BlockStyle
{
    /// <summary>Blocks use braces, e.g. "if (x) {".</summary>
    Braces,

    /// <summary>Blocks use significant indentation with a header terminator, e.g. "if x:".</summary>
    Indentation
}
