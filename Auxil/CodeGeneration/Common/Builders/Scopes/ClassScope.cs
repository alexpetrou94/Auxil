using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// A class scope. Supports inheritance and class members. Feature-gated
/// members (Implements, Generic, inner Class, Interface, Signal,
/// Constructor) are provided as trait-gated extension members.
/// </summary>
[PublicAPI]
public sealed class ClassScope<TLanguage, TParent> : DeclarationScope<TLanguage, ClassScope<TLanguage, TParent>, TParent>
    where TLanguage : struct, ILanguage
    where TParent : IScope
{
    internal ClassScope(CodeBuilder<TLanguage> builder, CodeClass cls, TParent parent) : base(builder, cls, parent)
    {
        ClassNode = cls;
    }

    internal CodeClass ClassNode { get; }

    /// <summary>Returns to the parent scope.</summary>
    public TParent EndClass()
    {
        return End();
    }

    /// <summary>Sets the base class of the current class.</summary>
    /// <param name="baseType">The base class name.</param>
    public ClassScope<TLanguage, TParent> Extends(string baseType)
    {
        ClassNode.ExtendsType = baseType;
        return this;
    }

    /// <summary>Adds a comment inside the class body.</summary>
    /// <param name="text">The comment text.</param>
    public ClassScope<TLanguage, TParent> Comment(string text)
    {
        ClassNode.Members.Add(new CodeComment { Text = text });
        return this;
    }

    /// <summary>Adds an enum declaration to the current class.</summary>
    /// <param name="name">The enum name.</param>
    public EnumScope<TLanguage, ClassScope<TLanguage, TParent>> Enum(string name)
    {
        CodeEnum enm = new() { Name = name };
        ClassNode.Members.Add(enm);
        return new EnumScope<TLanguage, ClassScope<TLanguage, TParent>>(Builder, enm, this);
    }

    /// <summary>Adds a field to the current class.</summary>
    /// <param name="name">The field name.</param>
    /// <param name="type">The field type.</param>
    /// <param name="defaultValue">An optional default value, as a raw expression.</param>
    public FieldScope<TLanguage, ClassScope<TLanguage, TParent>> Field(string name, CodeType? type = null, string? defaultValue = null)
    {
        CodeField field = new() { Name = name, Type = type, DefaultValue = defaultValue };
        ClassNode.Members.Add(field);
        return new FieldScope<TLanguage, ClassScope<TLanguage, TParent>>(Builder, field, this);
    }

    /// <summary>Adds a field to the current class using a type name string.</summary>
    /// <param name="name">The field name.</param>
    /// <param name="typeName">The field type name.</param>
    /// <param name="defaultValue">An optional default value, as a raw expression.</param>
    public FieldScope<TLanguage, ClassScope<TLanguage, TParent>> Field(string name, string typeName, string? defaultValue = null)
    {
        return Field(name, CodeType.Of(typeName), defaultValue);
    }

    /// <summary>Adds a property to the current class.</summary>
    /// <param name="name">The property name.</param>
    /// <param name="type">The property type.</param>
    public PropertyScope<TLanguage, ClassScope<TLanguage, TParent>> Property(string name, CodeType? type = null)
    {
        CodeProperty property = new() { Name = name, Type = type };
        ClassNode.Members.Add(property);
        return new PropertyScope<TLanguage, ClassScope<TLanguage, TParent>>(Builder, property, this);
    }

    /// <summary>Adds a property to the current class using a type name string.</summary>
    /// <param name="name">The property name.</param>
    /// <param name="typeName">The property type name.</param>
    public PropertyScope<TLanguage, ClassScope<TLanguage, TParent>> Property(string name, string typeName)
    {
        return Property(name, CodeType.Of(typeName));
    }

    /// <summary>Adds a method to the current class.</summary>
    /// <param name="name">The method name.</param>
    public MethodScope<TLanguage, ClassScope<TLanguage, TParent>> Method(string name)
    {
        CodeMethod method = new() { Name = name };
        ClassNode.Members.Add(method);
        return new MethodScope<TLanguage, ClassScope<TLanguage, TParent>>(Builder, method, this);
    }
}
