using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// A type alias scope. Configuration only: modifiers, doc, annotations.
/// </summary>
[PublicAPI]
public sealed class TypeAliasScope<TLanguage, TParent> : DeclarationScope<TLanguage, TypeAliasScope<TLanguage, TParent>, TParent>
    where TLanguage : struct, ILanguage
    where TParent : IScope
{
    internal TypeAliasScope(CodeBuilder<TLanguage> builder, CodeTypeAlias alias, TParent parent) : base(builder, alias, parent)
    {
    }

    /// <summary>Returns to the parent scope.</summary>
    public TParent EndTypeAlias()
    {
        return End();
    }
}
