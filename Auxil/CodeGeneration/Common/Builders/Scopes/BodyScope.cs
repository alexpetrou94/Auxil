using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// Base for scopes that have a body (method, constructor, function).
/// Provides the Body overloads.
/// </summary>
[PublicAPI]
public abstract class BodyScope<TLanguage, TScope, TParent> : ParameterScope<TLanguage, TScope, TParent>
    where TLanguage : struct, ILanguage
    where TScope : BodyScope<TLanguage, TScope, TParent>
    where TParent : IScope
{
    internal BodyScope(CodeBuilder<TLanguage> builder, CodeNode node, TParent parent) : base(builder, node, parent)
    {
    }

    /// <summary>Stores the body.</summary>
    protected abstract void SetBody(CodeBody body);

    /// <summary>Sets the body.</summary>
    /// <param name="configure">Builds the body statements.</param>
    /// <param name="id">An optional id marking the body as a customization point.</param>
    public TScope Body(Action<BodyBuilder<TLanguage>> configure, string? id = null)
    {
        CodeBody body = new() { Id = id };
        SetBody(body);
        BodyBuilder<TLanguage> bodyBuilder = new(body.Statements);
        configure(bodyBuilder);
        return (TScope)this;
    }

    /// <summary>Sets the body as a single raw line.</summary>
    /// <param name="code">The verbatim line of code.</param>
    /// <param name="id">An optional id marking the body as a customization point.</param>
    public TScope Body(string code, string? id = null)
    {
        CodeBody body = new() { Id = id };
        body.Statements.Add(new CodeExpressionStatement { Line = code });
        SetBody(body);
        return (TScope)this;
    }
}
