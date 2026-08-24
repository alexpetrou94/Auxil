namespace Auxil.CodeGeneration.Common.Model;

/// <summary>
/// An import/using directive: TS import { a } from "x",
/// C# using X.Y.Z. Renders nothing for languages without imports.
/// </summary>
[PublicAPI]
public sealed class CodeImport : CodeNode
{
    /// <summary>The imported module, namespace, or path.</summary>
    public required string From { get; init; }

    /// <summary>An optional default import name or alias.</summary>
    public string? DefaultName { get; set; }

    /// <summary>Named members to import (TypeScript).</summary>
    public List<string> NamedNames { get; } = [];
}
