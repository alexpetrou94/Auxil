using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// An enum scope. Member() is the only member-creating operation.
/// </summary>
[PublicAPI]
public sealed class EnumScope<TLanguage, TParent> : DeclarationScope<TLanguage, EnumScope<TLanguage, TParent>, TParent>
    where TLanguage : struct, ILanguage
    where TParent : IScope
{
    private readonly CodeEnum _enm;

    internal EnumScope(CodeBuilder<TLanguage> builder, CodeEnum enm, TParent parent) : base(builder, enm, parent)
    {
        _enm = enm;
    }

    /// <summary>Returns to the parent scope.</summary>
    public TParent EndEnum()
    {
        return End();
    }

    /// <summary>Adds a member to the enum.</summary>
    /// <param name="name">The member name.</param>
    /// <param name="value">An optional explicit value, as a raw expression.</param>
    public EnumScope<TLanguage, TParent> Member(string name, string? value = null)
    {
        _enm.Members.Add(new CodeEnumMember { Name = name, Value = value });
        return this;
    }
}
