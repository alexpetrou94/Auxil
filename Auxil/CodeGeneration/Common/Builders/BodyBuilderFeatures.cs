using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Builders;

/// <summary>
/// Trait-gated statement members. Each block is constrained on the
/// feature it implements, so a language without the statement does not
/// offer it — using it is a compile-time error.
/// </summary>
[PublicAPI]
public static class BodyBuilderFeatures
{
    extension<TLanguage>(BodyBuilder<TLanguage> builder) where TLanguage : struct, ILanguage, IHasDoWhile
    {
        /// <summary>Adds a do/while loop.</summary>
        /// <param name="condition">The condition, as a raw expression.</param>
        /// <param name="body">Builds the loop body.</param>
        /// <param name="id">An optional id for surgical editing.</param>
        public BodyBuilder<TLanguage> DoWhile(string condition, Action<BodyBuilder<TLanguage>> body, string? id = null)
        {
            CodeDoWhile statement = new() { Condition = condition, Id = id };
            ConfigureBlock(builder, statement.Body, body);
            builder.Statements.Add(statement);
            return builder;
        }
    }

    extension<TLanguage>(BodyBuilder<TLanguage> builder) where TLanguage : struct, ILanguage, IHasCStyleFor
    {
        /// <summary>Adds a C-style for loop.</summary>
        /// <param name="initializer">The initializer clause, as a raw expression.</param>
        /// <param name="condition">The condition clause, as a raw expression.</param>
        /// <param name="increment">The increment clause, as a raw expression.</param>
        /// <param name="body">Builds the loop body.</param>
        /// <param name="id">An optional id for surgical editing.</param>
        public BodyBuilder<TLanguage> For(string initializer, string condition, string increment, Action<BodyBuilder<TLanguage>> body, string? id = null)
        {
            CodeFor statement = new() { Initializer = initializer, Condition = condition, Increment = increment, Id = id };
            ConfigureBlock(builder, statement.Body, body);
            builder.Statements.Add(statement);
            return builder;
        }
    }

    extension<TLanguage>(BodyBuilder<TLanguage> builder) where TLanguage : struct, ILanguage, IHasThrow
    {
        /// <summary>Adds a throw statement.</summary>
        /// <param name="value">The thrown value, as a raw expression.</param>
        /// <param name="id">An optional id for surgical editing.</param>
        public BodyBuilder<TLanguage> Throw(string value, string? id = null)
        {
            builder.Statements.Add(new CodeThrow { Value = value, Id = id });
            return builder;
        }
    }

    extension<TLanguage>(BodyBuilder<TLanguage> builder) where TLanguage : struct, ILanguage, IHasMatch
    {
        /// <summary>Adds a match (GDScript) / switch (TypeScript, C#) statement.</summary>
        /// <param name="expression">The matched expression, as a raw expression.</param>
        /// <param name="configure">Configures the arms.</param>
        /// <param name="id">An optional id for surgical editing.</param>
        public BodyBuilder<TLanguage> Match(string expression, Action<MatchBuilder<TLanguage>> configure, string? id = null)
        {
            CodeMatch statement = new() { Expression = expression, Id = id };
            MatchBuilder<TLanguage> matchBuilder = new(statement);
            configure(matchBuilder);
            builder.Statements.Add(statement);
            return builder;
        }
    }

    extension<TLanguage>(BodyBuilder<TLanguage> builder) where TLanguage : struct, ILanguage, IHasExceptionHandling
    {
        /// <summary>Adds a try block.</summary>
        /// <param name="body">Builds the try block.</param>
        /// <param name="id">An optional id for surgical editing.</param>
        public BodyBuilder<TLanguage> Try(Action<BodyBuilder<TLanguage>> body, string? id = null)
        {
            CodeTry statement = new() { Id = id };
            ConfigureBlock(builder, statement.Body, body);
            builder.Statements.Add(statement);
            return builder;
        }

        /// <summary>Adds a bare catch block to the most recent Try statement in the current body.</summary>
        /// <param name="body">Builds the catch block.</param>
        public BodyBuilder<TLanguage> Catch(Action<BodyBuilder<TLanguage>> body)
        {
            return builder.AddCatch(null, null, body);
        }

        /// <summary>Adds an untyped catch block to the most recent Try statement.</summary>
        /// <param name="variable">The exception variable name, or null for a bare catch.</param>
        /// <param name="body">Builds the catch block.</param>
        public BodyBuilder<TLanguage> Catch(string? variable, Action<BodyBuilder<TLanguage>> body)
        {
            return builder.AddCatch(null, variable, body);
        }

        /// <summary>Adds a typed catch block to the most recent Try statement.</summary>
        /// <param name="type">The exception type.</param>
        /// <param name="variable">The exception variable name, or null when the type is not bound.</param>
        /// <param name="body">Builds the catch block.</param>
        public BodyBuilder<TLanguage> Catch(string? type, string? variable, Action<BodyBuilder<TLanguage>> body)
            => builder.AddCatch(type, variable, body);

        private BodyBuilder<TLanguage> AddCatch(string? type, string? variable, Action<BodyBuilder<TLanguage>> body)
        {
            if (builder.Statements.OfType<CodeTry>().LastOrDefault() is not { } last)
            {
                throw new CodeBuilderException("Catch requires a preceding Try statement.");
            }

            CodeCatch catchBlock = new()
            {
                Type = type is null ? null : CodeType.Of(type),
                Variable = variable
            };
            ConfigureBlock(builder, catchBlock.Body, body);
            last.Catches.Add(catchBlock);
            return builder;
        }

        /// <summary>Adds a finally block to the most recent Try statement in the current body.</summary>
        /// <param name="body">Builds the finally block.</param>
        public BodyBuilder<TLanguage> Finally(Action<BodyBuilder<TLanguage>> body)
        {
            if (builder.Statements.OfType<CodeTry>().LastOrDefault() is not { } last)
            {
                throw new CodeBuilderException("Finally requires a preceding Try statement.");
            }

            if (last.Finally.Count > 0)
            {
                throw new CodeBuilderException("Finally can only be added once per Try statement.");
            }

            ConfigureBlock(builder, last.Finally, body);
            return builder;
        }
    }

    extension<TLanguage>(BodyBuilder<TLanguage> builder) where TLanguage : struct, ILanguage, IHasNestedFunctions
    {
        /// <summary>Adds a nested function declaration (TypeScript nested function, C# local function).</summary>
        /// <param name="name">The function name.</param>
        /// <param name="configure">Builds the function's signature and body.</param>
        /// <param name="id">An optional id for surgical editing.</param>
        public BodyBuilder<TLanguage> Function(string name, Action<FunctionStatementBuilder<TLanguage>> configure, string? id = null)
        {
            CodeNestedFunction function = new() { Name = name, Id = id };
            FunctionStatementBuilder<TLanguage> functionBuilder = new(function);
            configure(functionBuilder);
            builder.Statements.Add(function);
            return builder;
        }
    }

    extension<TLanguage>(BodyBuilder<TLanguage> builder) where TLanguage : struct, ILanguage, IHasObjectLiterals
    {
        /// <summary>Adds an object literal statement.</summary>
        /// <param name="prefix">Leading text before the opening brace (e.g. "return"), or null for a bare literal.</param>
        /// <param name="configure">Builds the literal's members.</param>
        /// <param name="id">An optional id for surgical editing.</param>
        public BodyBuilder<TLanguage> ObjectLiteral(string? prefix, Action<ObjectLiteralBuilder<TLanguage>> configure, string? id = null)
        {
            CodeObjectLiteral literal = new() { Prefix = prefix, Id = id };
            ObjectLiteralBuilder<TLanguage> literalBuilder = new(literal);
            configure(literalBuilder);
            builder.Statements.Add(literal);
            return builder;
        }

        /// <summary>Adds an object literal statement with no prefix.</summary>
        /// <param name="configure">Builds the literal's members.</param>
        /// <param name="id">An optional id for surgical editing.</param>
        public BodyBuilder<TLanguage> ObjectLiteral(Action<ObjectLiteralBuilder<TLanguage>> configure, string? id = null)
        {
            return builder.ObjectLiteral(null, configure, id);
        }
    }

    private static void ConfigureBlock<TLanguage>(BodyBuilder<TLanguage> builder, List<CodeStatement> target, Action<BodyBuilder<TLanguage>> configure)
        where TLanguage : struct, ILanguage
    {
        BodyBuilder<TLanguage> bodyBuilder = new(target);
        configure(bodyBuilder);
    }
}
