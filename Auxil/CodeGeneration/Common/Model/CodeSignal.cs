namespace Auxil.CodeGeneration.Common.Model;

/// <summary>
/// A GDScript-style signal declaration. Languages without signals
/// render nothing (with a warning).
/// </summary>
[PublicAPI]
public sealed class CodeSignal : CodeMember
{
    /// <summary>The signal name.</summary>
    public required string Name { get; init; }

    /// <summary>The signal parameters in declaration order.</summary>
    public List<CodeParameter> Parameters { get; } = [];
}
