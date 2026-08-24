using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// A property scope. Defaults to an auto get/set property; GetOnly()
/// makes it get-only, and Getter()/Setter() add accessor bodies.
/// </summary>
[PublicAPI]
public sealed class PropertyScope<TLanguage, TParent> : DeclarationScope<TLanguage, PropertyScope<TLanguage, TParent>, TParent>
    where TLanguage : struct, ILanguage
    where TParent : IScope
{
    private readonly CodeProperty _property;

    internal PropertyScope(CodeBuilder<TLanguage> builder, CodeProperty property, TParent parent) : base(builder, property, parent)
    {
        _property = property;
    }

    /// <summary>Returns to the parent scope.</summary>
    public TParent EndProperty()
    {
        return End();
    }

    /// <summary>Makes the property get-only (removes the setter).</summary>
    public PropertyScope<TLanguage, TParent> GetOnly()
    {
        _property.HasSetter = false;
        return this;
    }

    /// <summary>
    /// Marks the property as nullable (may be absent). TypeScript renders
    /// it as name?: T; C# renders the type as nullable (T?).
    /// </summary>
    public PropertyScope<TLanguage, TParent> Nullable()
    {
        _property.Nullable = true;
        return this;
    }

    /// <summary>Sets the getter body (turns an auto-property into a block accessor).</summary>
    /// <param name="configure">Builds the getter body statements.</param>
    public PropertyScope<TLanguage, TParent> Getter(Action<BodyBuilder<TLanguage>> configure)
    {
        CodeBody body = new();
        BodyBuilder<TLanguage> bodyBuilder = new(body.Statements);
        configure(bodyBuilder);
        _property.HasGetter = true;
        _property.GetterBody = body;
        return this;
    }

    /// <summary>Sets the setter body (turns an auto-property into a block accessor).</summary>
    /// <param name="configure">Builds the setter body statements.</param>
    public PropertyScope<TLanguage, TParent> Setter(Action<BodyBuilder<TLanguage>> configure)
    {
        CodeBody body = new();
        BodyBuilder<TLanguage> bodyBuilder = new(body.Statements);
        configure(bodyBuilder);
        _property.HasSetter = true;
        _property.SetterBody = body;
        return this;
    }
}
