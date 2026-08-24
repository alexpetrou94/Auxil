using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// A top-level function scope. Parameters, return type, generics, body.
/// </summary>
[PublicAPI]
public sealed class FunctionScope<TLanguage, TParent> : BodyScope<TLanguage, FunctionScope<TLanguage, TParent>, TParent>
    where TLanguage : struct, ILanguage
    where TParent : IScope
{
    internal FunctionScope(CodeBuilder<TLanguage> builder, CodeFunction function, TParent parent) : base(builder, function, parent)
    {
        FunctionNode = function;
    }

    internal CodeFunction FunctionNode { get; }

    /// <summary>Returns to the parent scope.</summary>
    public TParent EndFunction()
    {
        return End();
    }

    /// <summary>Sets the function's return type.</summary>
    /// <param name="type">The return type; null renders no return type (or void where the language requires it).</param>
    public FunctionScope<TLanguage, TParent> Returns(CodeType? type)
    {
        FunctionNode.ReturnType = type;
        return this;
    }

    /// <summary>Sets the function's return type using a type name string.</summary>
    /// <param name="typeName">The return type name.</param>
    public FunctionScope<TLanguage, TParent> Returns(string typeName)
    {
        return Returns(CodeType.Of(typeName));
    }

    /// <inheritdoc/>
    protected override void AddParameter(CodeParameter parameter)
    {
        FunctionNode.Parameters.Add(parameter);
    }

    /// <inheritdoc/>
    protected override void SetBody(CodeBody body)
    {
        FunctionNode.Body = body;
    }
}
