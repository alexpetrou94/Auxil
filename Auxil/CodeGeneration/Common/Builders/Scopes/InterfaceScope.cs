using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// An interface scope. Supports inheritance, properties and method
/// signatures. Generic() is a trait-gated extension member.
/// </summary>
[PublicAPI]
public sealed class InterfaceScope<TLanguage, TParent> : DeclarationScope<TLanguage, InterfaceScope<TLanguage, TParent>, TParent>
    where TLanguage : struct, ILanguage
    where TParent : IScope
{
    internal InterfaceScope(CodeBuilder<TLanguage> builder, CodeInterface iface, TParent parent) : base(builder, iface, parent)
    {
        InterfaceNode = iface;
    }

    internal CodeInterface InterfaceNode { get; }

    /// <summary>Returns to the parent scope.</summary>
    public TParent EndInterface()
    {
        return End();
    }

    /// <summary>Adds base interfaces the current interface extends.</summary>
    /// <param name="interfaces">The base interface names.</param>
    public InterfaceScope<TLanguage, TParent> Extends(params string[] interfaces)
    {
        InterfaceNode.ExtendsTypes.AddRange(interfaces);
        return this;
    }

    /// <summary>Adds a comment inside the interface.</summary>
    /// <param name="text">The comment text.</param>
    public InterfaceScope<TLanguage, TParent> Comment(string text)
    {
        InterfaceNode.Members.Add(new CodeComment { Text = text });
        return this;
    }

    /// <summary>Adds a property to the interface.</summary>
    /// <param name="name">The property name.</param>
    /// <param name="type">The property type.</param>
    /// <param name="defaultValue">An optional default value, as a raw expression.</param>
    public FieldScope<TLanguage, InterfaceScope<TLanguage, TParent>> Field(string name, CodeType? type = null, string? defaultValue = null)
    {
        CodeField field = new() { Name = name, Type = type, DefaultValue = defaultValue };
        InterfaceNode.Members.Add(field);
        return new FieldScope<TLanguage, InterfaceScope<TLanguage, TParent>>(Builder, field, this);
    }

    /// <summary>Adds a property to the interface using a type name string.</summary>
    /// <param name="name">The property name.</param>
    /// <param name="typeName">The property type name.</param>
    /// <param name="defaultValue">An optional default value, as a raw expression.</param>
    public FieldScope<TLanguage, InterfaceScope<TLanguage, TParent>> Field(string name, string typeName, string? defaultValue = null)
    {
        return Field(name, CodeType.Of(typeName), defaultValue);
    }

    /// <summary>Adds a property to the interface.</summary>
    /// <param name="name">The property name.</param>
    /// <param name="type">The property type.</param>
    public PropertyScope<TLanguage, InterfaceScope<TLanguage, TParent>> Property(string name, CodeType? type = null)
    {
        CodeProperty property = new() { Name = name, Type = type };
        InterfaceNode.Members.Add(property);
        return new PropertyScope<TLanguage, InterfaceScope<TLanguage, TParent>>(Builder, property, this);
    }

    /// <summary>Adds a property to the interface using a type name string.</summary>
    /// <param name="name">The property name.</param>
    /// <param name="typeName">The property type name.</param>
    public PropertyScope<TLanguage, InterfaceScope<TLanguage, TParent>> Property(string name, string typeName)
    {
        return Property(name, CodeType.Of(typeName));
    }

    /// <summary>Adds a method signature to the interface.</summary>
    /// <param name="name">The method name.</param>
    public MethodScope<TLanguage, InterfaceScope<TLanguage, TParent>> Method(string name)
    {
        CodeMethod method = new() { Name = name };
        InterfaceNode.Members.Add(method);
        return new MethodScope<TLanguage, InterfaceScope<TLanguage, TParent>>(Builder, method, this);
    }
}
