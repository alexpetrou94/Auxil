using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// A constructor scope. Parameters and body.
/// </summary>
[PublicAPI]
public sealed class ConstructorScope<TLanguage, TParent> : BodyScope<TLanguage, ConstructorScope<TLanguage, TParent>, TParent>
    where TLanguage : struct, ILanguage
    where TParent : IScope
{
    private readonly CodeConstructor _ctor;

    internal ConstructorScope(CodeBuilder<TLanguage> builder, CodeConstructor ctor, TParent parent) : base(builder, ctor, parent)
    {
        _ctor = ctor;
    }

    /// <summary>Returns to the parent scope.</summary>
    public TParent EndConstructor()
    {
        return End();
    }

    /// <inheritdoc/>
    protected override void AddParameter(CodeParameter parameter)
    {
        _ctor.Parameters.Add(parameter);
    }

    /// <inheritdoc/>
    protected override void SetBody(CodeBody body)
    {
        _ctor.Body = body;
    }
}
