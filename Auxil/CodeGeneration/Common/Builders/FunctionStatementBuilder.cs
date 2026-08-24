using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders;

/// <summary>
/// Fluent helper for configuring a nested function declaration
/// (TypeScript nested function, C# local function). The signature is
/// built from structured parts; the body renders one level deeper.
/// </summary>
[PublicAPI]
public class FunctionStatementBuilder<TLanguage>
    where TLanguage : struct, ILanguage
{
    private readonly CodeNestedFunction _function;
    private CodeParameter? _lastParameter;

    internal FunctionStatementBuilder(CodeNestedFunction function)
    {
        _function = function;
    }

    /// <summary>Sets the documentation comment rendered above the function.</summary>
    /// <param name="text">The documentation text; may span multiple lines.</param>
    public FunctionStatementBuilder<TLanguage> Doc(string text)
    {
        _function.DocComment = new CodeComment { Text = text, IsDocumentation = true };
        return this;
    }

    /// <summary>Adds the static modifier.</summary>
    public FunctionStatementBuilder<TLanguage> Static()
    {
        _function.Modifiers |= CodeModifier.Static;
        return this;
    }

    /// <summary>Adds the async modifier.</summary>
    public FunctionStatementBuilder<TLanguage> Async()
    {
        _function.Modifiers |= CodeModifier.Async;
        return this;
    }

    /// <summary>Adds a generic type parameter to the function.</summary>
    /// <param name="name">The type parameter name, e.g. "T".</param>
    /// <param name="constraint">An optional constraint (rendered per language).</param>
    /// <param name="defaultValue">An optional default type argument (TypeScript only).</param>
    public FunctionStatementBuilder<TLanguage> Generic(string name, string? constraint = null, string? defaultValue = null)
    {
        _function.Generics.Add(new CodeGenericParameter { Name = name, Constraint = constraint, DefaultValue = defaultValue });
        return this;
    }

    /// <summary>Adds a parameter to the function.</summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="type">The parameter type.</param>
    /// <param name="defaultValue">An optional default value, as a raw expression.</param>
    public FunctionStatementBuilder<TLanguage> Parameter(string name, CodeType? type = null, string? defaultValue = null)
    {
        CodeParameter parameter = new() { Name = name, Type = type, DefaultValue = defaultValue };
        _lastParameter = parameter;
        _function.Parameters.Add(parameter);
        return this;
    }

    /// <summary>Adds a parameter using a type name string.</summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="typeName">The parameter type name.</param>
    /// <param name="defaultValue">An optional default value, as a raw expression.</param>
    public FunctionStatementBuilder<TLanguage> Parameter(string name, string typeName, string? defaultValue = null)
    {
        return Parameter(name, CodeType.Of(typeName), defaultValue);
    }

    /// <summary>
    /// Marks the most recently added parameter as nullable (may be absent).
    /// TypeScript renders it as p?: T; C# renders the type as nullable (T?).
    /// </summary>
    public FunctionStatementBuilder<TLanguage> Nullable()
    {
        _lastParameter?.Nullable = true;
        return this;
    }

    /// <summary>Sets the function's return type.</summary>
    /// <param name="type">The return type; null renders no return type (or void where the language requires it).</param>
    public FunctionStatementBuilder<TLanguage> Returns(CodeType? type)
    {
        _function.ReturnType = type;
        return this;
    }

    /// <summary>Sets the function's return type using a type name string.</summary>
    /// <param name="typeName">The return type name.</param>
    public FunctionStatementBuilder<TLanguage> Returns(string typeName)
    {
        return Returns(CodeType.Of(typeName));
    }

    /// <summary>Sets the function body.</summary>
    /// <param name="configure">Builds the body statements.</param>
    public FunctionStatementBuilder<TLanguage> Body(Action<BodyBuilder<TLanguage>> configure)
    {
        CodeBody body = new();
        BodyBuilder<TLanguage> bodyBuilder = new(body.Statements);
        configure(bodyBuilder);
        _function.Body = body;
        return this;
    }

    /// <summary>Sets the function body as a single raw line.</summary>
    /// <param name="code">The verbatim line of code.</param>
    public FunctionStatementBuilder<TLanguage> Body(string code)
    {
        CodeBody body = new();
        body.Statements.Add(new CodeExpressionStatement { Line = code });
        _function.Body = body;
        return this;
    }
}
