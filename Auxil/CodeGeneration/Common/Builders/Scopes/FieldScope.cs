using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// A field/variable scope. Configuration only: modifiers, doc, annotations.
/// </summary>
[PublicAPI]
public sealed class FieldScope<TLanguage, TParent> : DeclarationScope<TLanguage, FieldScope<TLanguage, TParent>, TParent>
    where TLanguage : struct, ILanguage
    where TParent : IScope
{
    private readonly CodeField _field;

    internal FieldScope(CodeBuilder<TLanguage> builder, CodeField field, TParent parent) : base(builder, field, parent)
    {
        _field = field;
    }

    /// <summary>Returns to the parent scope.</summary>
    public TParent EndField()
    {
        return End();
    }

    /// <summary>
    /// Marks the field as nullable (may be absent). TypeScript renders it
    /// as name?: T; C# renders the type as nullable (T?).
    /// </summary>
    public FieldScope<TLanguage, TParent> Nullable()
    {
        _field.Nullable = true;
        return this;
    }
}
