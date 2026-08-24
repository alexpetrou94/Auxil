namespace Auxil.CodeGeneration.Common.Syntax;

/// <summary>
/// How a language lays out a file.
/// </summary>
[PublicAPI]
public enum FileLayoutStyle
{
    /// <summary>TypeScript, C#: imports, then top-level declarations.</summary>
    Module,

    /// <summary>GDScript: the file itself is a class (class_name/extends then members).</summary>
    ScriptClass
}
