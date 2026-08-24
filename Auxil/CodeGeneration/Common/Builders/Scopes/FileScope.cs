using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// The file scope. Top-level declarations. Feature-gated members
/// (ClassName/Extends, Import, Namespace, Interface, Signal, Constructor,
/// Function) are provided as trait-gated extension members, so a language
/// without the feature does not offer them. End()/EndFile() returns the
/// builder.
/// </summary>
[PublicAPI]
public sealed class FileScope<TLanguage> : IScope
    where TLanguage : struct, ILanguage
{
    internal FileScope(CodeBuilder<TLanguage> builder, CodeFile file)
    {
        Builder = builder;
        FileNode = file;
    }

    internal CodeBuilder<TLanguage> Builder { get; }

    internal CodeFile FileNode { get; }

    /// <summary>Completes the file and returns to the builder.</summary>
    public CodeBuilder<TLanguage> EndFile()
    {
        Builder.CloseFile();
        return Builder;
    }

    /// <summary>Alias of EndFile(): completes the file and returns to the builder.</summary>
    public CodeBuilder<TLanguage> End()
    {
        return EndFile();
    }

    /// <summary>Adds a comment to the file.</summary>
    /// <param name="text">The comment text.</param>
    public FileScope<TLanguage> Comment(string text)
    {
        FileNode.Children.Add(new CodeComment { Text = text });
        return this;
    }

    /// <summary>Sets the file's documentation comment.</summary>
    /// <param name="text">The documentation text; may span multiple lines.</param>
    public FileScope<TLanguage> Doc(string text)
    {
        FileNode.DocComment = new CodeComment { Text = text, IsDocumentation = true };
        return this;
    }

    /// <summary>Adds a file-level annotation.</summary>
    /// <param name="name">The annotation name.</param>
    /// <param name="arguments">Optional arguments rendered in the annotation's parentheses.</param>
    public FileScope<TLanguage> Annotate(string name, params string[] arguments)
    {
        FileNode.Annotations.Add(new CodeAnnotation { Name = name, Arguments = [.. arguments] });
        return this;
    }

    /// <summary>Adds a top-level class declaration.</summary>
    /// <param name="name">The class name.</param>
    public ClassScope<TLanguage, FileScope<TLanguage>> Class(string name)
    {
        CodeClass cls = new() { Name = name };
        FileNode.Children.Add(cls);
        return new ClassScope<TLanguage, FileScope<TLanguage>>(Builder, cls, this);
    }

    /// <summary>Adds a top-level enum declaration.</summary>
    /// <param name="name">The enum name.</param>
    public EnumScope<TLanguage, FileScope<TLanguage>> Enum(string name)
    {
        CodeEnum enm = new() { Name = name };
        FileNode.Children.Add(enm);
        return new EnumScope<TLanguage, FileScope<TLanguage>>(Builder, enm, this);
    }

    /// <summary>Adds a top-level field/variable.</summary>
    /// <param name="name">The field name.</param>
    /// <param name="type">The field type.</param>
    /// <param name="defaultValue">An optional default value, as a raw expression.</param>
    public FieldScope<TLanguage, FileScope<TLanguage>> Field(string name, CodeType? type = null, string? defaultValue = null)
    {
        CodeField field = new() { Name = name, Type = type, DefaultValue = defaultValue };
        FileNode.Children.Add(field);
        return new FieldScope<TLanguage, FileScope<TLanguage>>(Builder, field, this);
    }

    /// <summary>Adds a top-level field/variable using a type name string.</summary>
    /// <param name="name">The field name.</param>
    /// <param name="typeName">The field type name.</param>
    /// <param name="defaultValue">An optional default value, as a raw expression.</param>
    public FieldScope<TLanguage, FileScope<TLanguage>> Field(string name, string typeName, string? defaultValue = null)
    {
        return Field(name, CodeType.Of(typeName), defaultValue);
    }

    /// <summary>Adds a top-level property.</summary>
    /// <param name="name">The property name.</param>
    /// <param name="type">The property type.</param>
    public PropertyScope<TLanguage, FileScope<TLanguage>> Property(string name, CodeType? type = null)
    {
        CodeProperty property = new() { Name = name, Type = type };
        FileNode.Children.Add(property);
        return new PropertyScope<TLanguage, FileScope<TLanguage>>(Builder, property, this);
    }

    /// <summary>Adds a top-level property using a type name string.</summary>
    /// <param name="name">The property name.</param>
    /// <param name="typeName">The property type name.</param>
    public PropertyScope<TLanguage, FileScope<TLanguage>> Property(string name, string typeName)
    {
        return Property(name, CodeType.Of(typeName));
    }

    /// <summary>Adds a top-level method (the file-as-class form in GDScript).</summary>
    /// <param name="name">The method name.</param>
    public MethodScope<TLanguage, FileScope<TLanguage>> Method(string name)
    {
        CodeMethod method = new() { Name = name };
        FileNode.Children.Add(method);
        return new MethodScope<TLanguage, FileScope<TLanguage>>(Builder, method, this);
    }
}
