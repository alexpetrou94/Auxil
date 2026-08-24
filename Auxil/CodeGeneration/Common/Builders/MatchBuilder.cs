using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders;

/// <summary>
/// Fluent helper for configuring the arms of a Match statement.
/// </summary>
[PublicAPI]
public class MatchBuilder<TLanguage>
    where TLanguage : struct, ILanguage
{
    private readonly CodeMatch _match;

    internal MatchBuilder(CodeMatch match)
    {
        _match = match;
    }

    /// <summary>Adds an arm to the match statement.</summary>
    /// <param name="pattern">The pattern, as a raw expression; "_" maps to the default arm.</param>
    /// <param name="body">Builds the arm body.</param>
    public MatchBuilder<TLanguage> Arm(string pattern, Action<BodyBuilder<TLanguage>> body)
    {
        CodeMatchArm arm = new() { Pattern = pattern };
        BodyBuilder<TLanguage> bodyBuilder = new(arm.Body);
        body(bodyBuilder);
        _match.Arms.Add(arm);
        return this;
    }
}
