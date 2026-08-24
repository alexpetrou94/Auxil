using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders;

/// <summary>
/// Fluent helper for building a method/constructor/function body.
/// Raw lines stay verbatim; structured statements (if/for/match) are
/// rendered with each language's skeleton. Statement methods are gated
/// on feature traits: a language without a statement does not offer it.
/// </summary>
[PublicAPI]
public class BodyBuilder<TLanguage>
    where TLanguage : struct, ILanguage
{
    internal BodyBuilder(List<CodeStatement> statements)
    {
        Statements = statements;
    }

    internal List<CodeStatement> Statements { get; }

    /// <summary>Adds a raw, verbatim line of code.</summary>
    /// <param name="code">The line of code, written exactly as given.</param>
    /// <param name="id">An optional id for surgical editing.</param>
    public BodyBuilder<TLanguage> Line(string code, string? id = null)
    {
        Statements.Add(new CodeExpressionStatement { Line = code, Id = id });
        return this;
    }

    /// <summary>Adds multiple raw, verbatim lines of code.</summary>
    /// <param name="lines">The lines of code, written exactly as given.</param>
    public BodyBuilder<TLanguage> Lines(params string[] lines)
    {
        foreach (string line in lines)
        {
            Statements.Add(new CodeExpressionStatement { Line = line });
        }

        return this;
    }

    /// <summary>Adds a comment, rendered with the language's comment prefix.</summary>
    /// <param name="text">The comment text.</param>
    public BodyBuilder<TLanguage> Comment(string text)
    {
        Statements.Add(new CodeCommentStatement { Text = text });
        return this;
    }

    /// <summary>Adds blank lines to the body.</summary>
    /// <param name="count">The number of blank lines.</param>
    public BodyBuilder<TLanguage> BlankLine(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            Statements.Add(new CodeBlankLine());
        }

        return this;
    }

    /// <summary>Adds an if statement.</summary>
    /// <param name="condition">The condition, as a raw expression.</param>
    /// <param name="body">Builds the then-block.</param>
    /// <param name="id">An optional id for surgical editing.</param>
    public BodyBuilder<TLanguage> If(string condition, Action<BodyBuilder<TLanguage>> body, string? id = null)
    {
        CodeIf statement = new() { Condition = condition, Id = id };
        ConfigureBlock(statement.Then, body);
        Statements.Add(statement);
        return this;
    }

    /// <summary>Adds an elif branch to the most recent If statement in the current body.</summary>
    /// <param name="condition">The condition, as a raw expression.</param>
    /// <param name="body">Builds the branch block.</param>
    public BodyBuilder<TLanguage> ElseIf(string condition, Action<BodyBuilder<TLanguage>> body)
    {
        CodeIf last = GetLastIf();
        CodeElseIf branch = new() { Condition = condition };
        ConfigureBlock(branch.Body, body);
        last.ElseIfs.Add(branch);
        return this;
    }

    /// <summary>Adds an else block to the most recent If statement in the current body.</summary>
    /// <param name="body">Builds the else block.</param>
    public BodyBuilder<TLanguage> Else(Action<BodyBuilder<TLanguage>> body)
    {
        CodeIf last = GetLastIf();
        if (last.Else.Count > 0)
        {
            throw new CodeBuilderException("Else can only be added once per If statement.");
        }

        ConfigureBlock(last.Else, body);
        return this;
    }

    /// <summary>Adds a foreach/for-in loop over an iterable.</summary>
    /// <param name="variable">The loop variable name.</param>
    /// <param name="iterable">The iterable, as a raw expression.</param>
    /// <param name="body">Builds the loop body.</param>
    /// <param name="id">An optional id for surgical editing.</param>
    public BodyBuilder<TLanguage> ForEach(string variable, string iterable, Action<BodyBuilder<TLanguage>> body, string? id = null)
    {
        CodeForEach statement = new() { Variable = variable, Iterable = iterable, Id = id };
        ConfigureBlock(statement.Body, body);
        Statements.Add(statement);
        return this;
    }

    /// <summary>Adds a while loop.</summary>
    /// <param name="condition">The condition, as a raw expression.</param>
    /// <param name="body">Builds the loop body.</param>
    /// <param name="id">An optional id for surgical editing.</param>
    public BodyBuilder<TLanguage> While(string condition, Action<BodyBuilder<TLanguage>> body, string? id = null)
    {
        CodeWhile statement = new() { Condition = condition, Id = id };
        ConfigureBlock(statement.Body, body);
        Statements.Add(statement);
        return this;
    }

    /// <summary>Adds a local variable declaration.</summary>
    /// <param name="name">The variable name.</param>
    /// <param name="type">An optional type; null lets the language infer (var).</param>
    /// <param name="value">An optional initializer, as a raw expression.</param>
    /// <param name="id">An optional id for surgical editing.</param>
    public BodyBuilder<TLanguage> Var(string name, CodeType? type = null, string? value = null, string? id = null)
    {
        Statements.Add(new CodeLocalVariable { Name = name, Type = type, Value = value, Id = id });
        return this;
    }

    /// <summary>Adds a local variable declaration using a type name string.</summary>
    /// <param name="name">The variable name.</param>
    /// <param name="typeName">The variable type name.</param>
    /// <param name="value">An optional initializer, as a raw expression.</param>
    /// <param name="id">An optional id for surgical editing.</param>
    public BodyBuilder<TLanguage> Var(string name, string typeName, string? value = null, string? id = null)
    {
        return Var(name, CodeType.Of(typeName), value, id);
    }

    /// <summary>Adds a constant local declaration.</summary>
    /// <param name="name">The constant name.</param>
    /// <param name="type">An optional type.</param>
    /// <param name="value">An optional initializer, as a raw expression.</param>
    /// <param name="id">An optional id for surgical editing.</param>
    public BodyBuilder<TLanguage> Const(string name, CodeType? type = null, string? value = null, string? id = null)
    {
        Statements.Add(new CodeLocalVariable { Name = name, Type = type, Value = value, IsConst = true, Id = id });
        return this;
    }

    /// <summary>Adds a constant local declaration using a type name string.</summary>
    /// <param name="name">The constant name.</param>
    /// <param name="typeName">The constant type name.</param>
    /// <param name="value">An optional initializer, as a raw expression.</param>
    /// <param name="id">An optional id for surgical editing.</param>
    public BodyBuilder<TLanguage> Const(string name, string typeName, string? value = null, string? id = null)
    {
        return Const(name, CodeType.Of(typeName), value, id);
    }

    /// <summary>Adds a return statement.</summary>
    /// <param name="value">An optional return value, as a raw expression.</param>
    /// <param name="id">An optional id for surgical editing.</param>
    public BodyBuilder<TLanguage> Return(string? value = null, string? id = null)
    {
        Statements.Add(new CodeReturn { Value = value, Id = id });
        return this;
    }

    /// <summary>
    /// Adds a nested block with a verbatim header. The header is written
    /// exactly as given; the body renders one level deeper and the block
    /// closes with the language's closing token, plus the close suffix
    /// (e.g. ";" for a statement, "," for an object-literal member).
    /// </summary>
    /// <param name="header">The block header, written exactly as given.</param>
    /// <param name="body">Builds the block body.</param>
    /// <param name="close">Text appended to the closing token, or null to close bare.</param>
    /// <param name="id">An optional id for surgical editing.</param>
    public BodyBuilder<TLanguage> Block(string header, Action<BodyBuilder<TLanguage>> body, string? close = null, string? id = null)
    {
        CodeRawBlock statement = new() { Header = header, CloseSuffix = close, Id = id };
        ConfigureBlock(statement.Body, body);
        Statements.Add(statement);
        return this;
    }

    /// <summary>Adds a break statement.</summary>
    /// <param name="id">An optional id for surgical editing.</param>
    public BodyBuilder<TLanguage> Break(string? id = null)
    {
        Statements.Add(new CodeBreak { Id = id });
        return this;
    }

    /// <summary>Adds a continue statement.</summary>
    /// <param name="id">An optional id for surgical editing.</param>
    public BodyBuilder<TLanguage> Continue(string? id = null)
    {
        Statements.Add(new CodeContinue { Id = id });
        return this;
    }

    private CodeIf GetLastIf()
    {
        if (Statements.OfType<CodeIf>().LastOrDefault() is { } last)
        {
            return last;
        }

        throw new CodeBuilderException("ElseIf/Else requires a preceding If statement.");
    }

    private void ConfigureBlock(List<CodeStatement> target, Action<BodyBuilder<TLanguage>> configure)
    {
        BodyBuilder<TLanguage> bodyBuilder = new(target);
        configure(bodyBuilder);
    }
}
