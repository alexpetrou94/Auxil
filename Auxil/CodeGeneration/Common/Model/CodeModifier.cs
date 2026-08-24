namespace Auxil.CodeGeneration.Common.Model;

/// <summary>
/// Union of declaration modifiers across languages.
/// A renderer decides which flags are meaningful for its language
/// and renders nothing (with a warning) for the rest.
/// </summary>
[PublicAPI]
[Flags]
public enum CodeModifier
{
    /// <summary>No modifiers.</summary>
    None = 0,

    /// <summary>The public visibility modifier.</summary>
    Public = 1 << 0,

    /// <summary>The private visibility modifier.</summary>
    Private = 1 << 1,

    /// <summary>The protected visibility modifier.</summary>
    Protected = 1 << 2,

    /// <summary>The internal visibility modifier (C#).</summary>
    Internal = 1 << 3,

    /// <summary>The static modifier.</summary>
    Static = 1 << 4,

    /// <summary>The abstract modifier.</summary>
    Abstract = 1 << 5,

    /// <summary>The virtual modifier (C#).</summary>
    Virtual = 1 << 6,

    /// <summary>The override modifier.</summary>
    Override = 1 << 7,

    /// <summary>The sealed modifier (C#).</summary>
    Sealed = 1 << 8,

    /// <summary>The readonly modifier.</summary>
    Readonly = 1 << 9,

    /// <summary>The const modifier.</summary>
    Const = 1 << 10,

    /// <summary>The async modifier.</summary>
    Async = 1 << 11,

    /// <summary>The module export modifier (TypeScript).</summary>
    Export = 1 << 12
}
