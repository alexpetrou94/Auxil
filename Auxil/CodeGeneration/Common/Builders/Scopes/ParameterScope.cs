using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// Base for scopes that accept parameters (method, constructor,
/// function, signal).
/// </summary>
[PublicAPI]
public abstract class ParameterScope<TLanguage, TScope, TParent> : DeclarationScope<TLanguage, TScope, TParent>
    where TLanguage : struct, ILanguage
    where TScope : ParameterScope<TLanguage, TScope, TParent>
    where TParent : IScope
{
    private CodeParameter? _lastParameter;

    internal ParameterScope(CodeBuilder<TLanguage> builder, CodeNode node, TParent parent) : base(builder, node, parent)
    {
    }

    /// <summary>Stores the parameter on the current declaration.</summary>
    protected abstract void AddParameter(CodeParameter parameter);

    /// <summary>Adds a parameter to the current declaration.</summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="type">The parameter type.</param>
    /// <param name="defaultValue">An optional default value, as a raw expression.</param>
    public TScope Parameter(string name, CodeType? type = null, string? defaultValue = null)
    {
        CodeParameter parameter = new() { Name = name, Type = type, DefaultValue = defaultValue };
        _lastParameter = parameter;
        AddParameter(parameter);
        return (TScope)this;
    }

    /// <summary>Adds a parameter using a type name string.</summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="typeName">The parameter type name.</param>
    /// <param name="defaultValue">An optional default value, as a raw expression.</param>
    public TScope Parameter(string name, string typeName, string? defaultValue = null)
    {
        return Parameter(name, CodeType.Of(typeName), defaultValue);
    }

    /// <summary>
    /// Marks the most recently added parameter as nullable (may be absent).
    /// TypeScript renders it as p?: T; C# renders the type as nullable (T?).
    /// </summary>
    public TScope Nullable()
    {
        _lastParameter?.Nullable = true;
        return (TScope)this;
    }
}
