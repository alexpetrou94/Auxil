using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// A signal scope. Parameters are its only members.
/// </summary>
[PublicAPI]
public sealed class SignalScope<TLanguage, TParent> : ParameterScope<TLanguage, SignalScope<TLanguage, TParent>, TParent>
    where TLanguage : struct, ILanguage
    where TParent : IScope
{
    private readonly CodeSignal _signal;

    internal SignalScope(CodeBuilder<TLanguage> builder, CodeSignal signal, TParent parent) : base(builder, signal, parent)
    {
        _signal = signal;
    }

    /// <summary>Returns to the parent scope.</summary>
    public TParent EndSignal()
    {
        return End();
    }

    /// <inheritdoc/>
    protected override void AddParameter(CodeParameter parameter)
    {
        _signal.Parameters.Add(parameter);
    }
}
