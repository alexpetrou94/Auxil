using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// A method scope. Parameters, return type, generics, body.
/// </summary>
[PublicAPI]
public sealed class MethodScope<TLanguage, TParent> : BodyScope<TLanguage, MethodScope<TLanguage, TParent>, TParent>
    where TLanguage : struct, ILanguage
    where TParent : IScope
{
    internal MethodScope(CodeBuilder<TLanguage> builder, CodeMethod method, TParent parent) : base(builder, method, parent)
    {
        MethodNode = method;
    }

    internal CodeMethod MethodNode { get; }

    /// <summary>Returns to the parent scope.</summary>
    public TParent EndMethod()
    {
        return End();
    }

    /// <summary>Sets the method's return type.</summary>
    /// <param name="type">The return type; null renders no return type (or void where the language requires it).</param>
    public MethodScope<TLanguage, TParent> Returns(CodeType? type)
    {
        MethodNode.ReturnType = type;
        return this;
    }

    /// <summary>Sets the method's return type using a type name string.</summary>
    /// <param name="typeName">The return type name.</param>
    public MethodScope<TLanguage, TParent> Returns(string typeName)
    {
        return Returns(CodeType.Of(typeName));
    }

    /// <inheritdoc/>
    protected override void AddParameter(CodeParameter parameter)
    {
        MethodNode.Parameters.Add(parameter);
    }

    /// <inheritdoc/>
    protected override void SetBody(CodeBody body)
    {
        MethodNode.Body = body;
    }
}
