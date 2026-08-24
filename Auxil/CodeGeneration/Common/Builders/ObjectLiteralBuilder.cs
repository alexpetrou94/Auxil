using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders;

/// <summary>
/// Fluent helper for configuring an object literal statement. Members
/// are comma-separated and the closing brace carries the terminator
/// automatically; a member value is a raw expression, a nested object
/// literal, or a function (rendered as a method shorthand).
/// </summary>
[PublicAPI]
public class ObjectLiteralBuilder<TLanguage>
    where TLanguage : struct, ILanguage
{
    private readonly CodeObjectLiteral _literal;

    internal ObjectLiteralBuilder(CodeObjectLiteral literal)
    {
        _literal = literal;
    }

    /// <summary>Adds a member with a raw expression value.</summary>
    /// <param name="name">The member name, written verbatim.</param>
    /// <param name="expression">The member value, as a raw expression.</param>
    public ObjectLiteralBuilder<TLanguage> Member(string name, string expression)
    {
        _literal.Members.Add(new CodeObjectMember { Name = name, Expression = expression });
        return this;
    }

    /// <summary>Adds a member whose value is a nested object literal.</summary>
    /// <param name="name">The member name, written verbatim.</param>
    /// <param name="nested">Builds the nested literal's members.</param>
    public ObjectLiteralBuilder<TLanguage> Member(string name, Action<ObjectLiteralBuilder<TLanguage>> nested)
    {
        CodeObjectLiteral nestedLiteral = new();
        ObjectLiteralBuilder<TLanguage> nestedBuilder = new(nestedLiteral);
        nested(nestedBuilder);
        _literal.Members.Add(new CodeObjectMember { Name = name, Object = nestedLiteral });
        return this;
    }

    /// <summary>Adds a member whose value is a function (method shorthand).</summary>
    /// <param name="name">The member name, written verbatim.</param>
    /// <param name="function">Builds the function's signature and body.</param>
    public ObjectLiteralBuilder<TLanguage> Member(string name, Action<FunctionStatementBuilder<TLanguage>> function)
    {
        CodeNestedFunction functionNode = new() { Name = name };
        FunctionStatementBuilder<TLanguage> functionBuilder = new(functionNode);
        function(functionBuilder);
        _literal.Members.Add(new CodeObjectMember { Name = name, Function = functionNode });
        return this;
    }
}
