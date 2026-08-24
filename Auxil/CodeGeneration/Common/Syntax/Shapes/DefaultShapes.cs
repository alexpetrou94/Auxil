namespace Auxil.CodeGeneration.Common.Syntax.Shapes;

/// <summary>
/// The shared default shape implementations, driven entirely by the
/// language's syntax profile tokens. These are what every language gets
/// unless it overrides a hook in its Shapes bundle.
/// </summary>
[PublicAPI]
public static class DefaultShapes
{
    // ---- Field ----

    /// <summary>Shapes a field declaration (the shared default).</summary>
    public static string Field(ShapeContext ctx, string name, CodeType? type, bool nullable, string? defaultValue, bool isConst, string keyword, bool isInterfaceMember)
    {
        if (isInterfaceMember)
        {
            if (ctx.Syntax.PropertyStyle == PropertyStyle.AsField)
            {
                if (defaultValue is not null)
                {
                    ctx.Warnings.Add($"Default value on interface property '{name}' is not supported in {ctx.LanguageName}; skipped.");
                }

                return BuildFieldDeclaration(ctx, name, type, nullable, null, isConst: false, keyword: string.Empty);
            }

            return BuildAutoAccessors(ctx, name, type, nullable, hasGetter: true, hasSetter: true);
        }

        return BuildFieldDeclaration(ctx, name, type, nullable, defaultValue, isConst, keyword);
    }

    private static string BuildFieldDeclaration(ShapeContext ctx, string name, CodeType? type, bool nullable, string? defaultValue, bool isConst, string keyword)
    {
        List<string> parts = [];

        if (ctx.Syntax.ParameterStyle == ParameterStyle.TypeFirst)
        {
            if (isConst)
            {
                parts.Add(ctx.Syntax.ConstKeyword);
            }

            string line = type is null
                ? name
                : $"{ctx.Syntax.MapType(type)}{(nullable ? "?" : "")} {name}";
            if (defaultValue is not null)
            {
                line += $" = {defaultValue}";
            }

            parts.Add(line);
            return string.Join(" ", parts);
        }

        if (isConst && keyword.Length > 0)
        {
            keyword = ctx.Syntax.ConstKeyword;
        }

        if (keyword.Length > 0)
        {
            parts.Add(keyword);
        }

        string declaration = name;
        if (nullable && ctx.Syntax.PropertyStyle == PropertyStyle.AsField)
        {
            declaration += "?";
        }

        if (type is not null)
        {
            declaration += $": {ctx.Syntax.MapType(type)}";
        }

        if (defaultValue is not null)
        {
            declaration += $" = {defaultValue}";
        }

        parts.Add(declaration);
        return string.Join(" ", parts);
    }

    private static string BuildAutoAccessors(ShapeContext ctx, string name, CodeType? type, bool nullable, bool hasGetter, bool hasSetter)
    {
        string line = name;
        if (type is not null)
        {
            line = $"{ctx.Syntax.MapType(type)}{(nullable ? "?" : "")} {line}";
        }

        List<string> accessors = [];
        if (hasGetter)
        {
            accessors.Add("get");
        }

        if (hasSetter)
        {
            accessors.Add("set");
        }

        return $"{line} {{ {string.Join("; ", accessors)}; }}";
    }

    // ---- Signature ----

    /// <summary>Shapes a method/constructor/function signature (the shared default).</summary>
    public static string Signature(ShapeContext ctx, SignatureKind kind, string name, List<CodeParameter> parameters, List<CodeGenericParameter> generics, CodeType? returnType, string className, bool interfaceMode)
    {
        string actualName = kind == SignatureKind.Constructor
            ? (ctx.Syntax.KeywordConstructor.Length > 0 ? ctx.Syntax.KeywordConstructor : className)
            : name;

        bool includeReturnType = kind != SignatureKind.Constructor;

        List<string> parts = [];

        string? mappedReturn = returnType is null ? null : ctx.Syntax.MapType(returnType);
        if (includeReturnType && ctx.Syntax.ReturnTypeStyle == ReturnTypeStyle.BeforeName)
        {
            parts.Add(mappedReturn ?? "void");
        }

        string keyword = kind == SignatureKind.Function ? ctx.Syntax.KeywordFunction : ctx.Syntax.KeywordMethod;
        if (keyword.Length > 0 && ctx.Syntax.ReturnTypeStyle != ReturnTypeStyle.BeforeName)
        {
            parts.Add(keyword);
        }

        string signature = BuildNameWithGenerics(ctx, actualName, generics) + $"({BuildParamList(ctx, parameters, interfaceMode)})";
        if (includeReturnType && mappedReturn is not null && ctx.Syntax.ReturnTypeStyle != ReturnTypeStyle.BeforeName)
        {
            signature += ctx.Syntax.ReturnTypeStyle == ReturnTypeStyle.ColonAfterParens ? $": {mappedReturn}" : $" -> {mappedReturn}";
        }

        parts.Add(signature);
        AppendWhereClauses(parts, ctx, generics);
        return string.Join(" ", parts);
    }

    // ---- Property ----

    /// <summary>Shapes a property declaration (the shared default).</summary>
    public static void Property(ShapeContext ctx, CodeProperty property, string prefix, bool isTopLevel, bool isInterfaceMember)
    {
        if (ctx.Syntax.PropertyStyle == PropertyStyle.AsField)
        {
            string keyword = isInterfaceMember ? string.Empty : (isTopLevel ? ctx.Syntax.TopLevelFieldKeyword : ctx.Syntax.FieldKeyword);
            string declaration = BuildFieldDeclaration(ctx, property.Name, property.Type, property.Nullable, null, isConst: false, keyword);
            ctx.Writer.WriteStatement(string.Join(" ", new[] { prefix, declaration }.Where(s => s.Length > 0)));
            return;
        }

        if (ctx.Syntax.PropertyStyle == PropertyStyle.AutoAccessors)
        {
            if (isInterfaceMember)
            {
                ctx.Writer.WriteLine(BuildAutoAccessors(ctx, property.Name, property.Type, property.Nullable, property.HasGetter, property.HasSetter));
                return;
            }

            RenderAutoAccessors(ctx, property, prefix);
            return;
        }

        RenderBlockFormProperty(ctx, property, prefix);
    }

    private static void RenderAutoAccessors(ShapeContext ctx, CodeProperty property, string prefix)
    {
        string name = property.Name;
        if (property.Type is not null)
        {
            name = $"{ctx.Syntax.MapType(property.Type)}{(property.Nullable ? "?" : "")} {name}";
        }

        string declaration = string.Join(" ", new[] { prefix, name }.Where(s => s.Length > 0));

        bool hasGetterBody = property.GetterBody is not null;
        bool hasSetterBody = property.SetterBody is not null;

        if (!hasGetterBody && !hasSetterBody)
        {
            List<string> accessors = [];
            if (property.HasGetter)
            {
                accessors.Add("get");
            }

            if (property.HasSetter)
            {
                accessors.Add("set");
            }

            declaration += $" {{ {string.Join("; ", accessors)}; }}";
            ctx.Writer.WriteLine(declaration);
            return;
        }

        ShapeBlocks.Open(ctx, declaration);

        if (property.HasGetter)
        {
            RenderAccessor(ctx, "get", property.GetterBody);
        }

        if (property.HasSetter)
        {
            RenderAccessor(ctx, "set", property.SetterBody);
        }

        ShapeBlocks.Close(ctx);
    }

    private static void RenderAccessor(ShapeContext ctx, string keyword, CodeBody? body)
    {
        ShapeBlocks.Open(ctx, keyword);
        ctx.RenderStatements(body?.Statements ?? []);
        ShapeBlocks.Close(ctx);
    }

    private static void RenderBlockFormProperty(ShapeContext ctx, CodeProperty property, string prefix)
    {
        string header = string.Join(" ", new[] { prefix, "var", property.Name }.Where(s => s.Length > 0));
        if (property.Type is not null)
        {
            header += $": {ctx.Syntax.MapType(property.Type)}";
        }

        bool hasGetterBody = property.GetterBody is not null;
        bool hasSetterBody = property.SetterBody is not null;

        if (!hasGetterBody && !hasSetterBody)
        {
            ctx.Writer.WriteLine(header);
            return;
        }

        ctx.Writer.OpenBlock(header);

        if (property.HasGetter)
        {
            ctx.Writer.WriteLine("get:");
            ctx.Writer.Indent();
            ctx.RenderStatements(property.GetterBody?.Statements ?? []);
            ctx.Writer.Dedent();
        }

        if (property.HasSetter)
        {
            ctx.Writer.WriteLine("set(value):");
            ctx.Writer.Indent();
            ctx.RenderStatements(property.SetterBody?.Statements ?? []);
            ctx.Writer.Dedent();
        }

        ctx.Writer.CloseBlock();
    }

    // ---- Headers ----

    /// <summary>Shapes a class declaration header (the shared default).</summary>
    public static string ClassHeader(ShapeContext ctx, string name, List<CodeGenericParameter> generics, string? extendsType, List<string> implementsTypes)
    {
        List<string> parts = [ctx.Syntax.KeywordClass, BuildNameWithGenerics(ctx, name, generics)];

        if (ctx.Syntax.BaseListStyle == BaseListStyle.Colon)
        {
            List<string> bases = [];
            if (extendsType is not null)
            {
                bases.Add(extendsType);
            }

            bases.AddRange(implementsTypes);
            if (bases.Count > 0)
            {
                parts.Add($": {string.Join(", ", bases)}");
            }
        }
        else
        {
            if (extendsType is not null)
            {
                parts.Add($"{ctx.Syntax.KeywordExtends} {extendsType}");
            }

            if (ctx.Syntax.KeywordImplements.Length > 0 && implementsTypes.Count > 0)
            {
                parts.Add($"{ctx.Syntax.KeywordImplements} {string.Join(", ", implementsTypes)}");
            }
        }

        AppendWhereClauses(parts, ctx, generics);
        return string.Join(" ", parts);
    }

    /// <summary>Shapes an interface declaration header (the shared default).</summary>
    public static string InterfaceHeader(ShapeContext ctx, string name, List<CodeGenericParameter> generics, List<string> extendsTypes)
    {
        List<string> parts = [ctx.Syntax.KeywordInterface, BuildNameWithGenerics(ctx, name, generics)];

        if (extendsTypes.Count > 0)
        {
            if (ctx.Syntax.BaseListStyle == BaseListStyle.Colon)
            {
                parts.Add($": {string.Join(", ", extendsTypes)}");
            }
            else
            {
                parts.Add($"{ctx.Syntax.KeywordExtends} {string.Join(", ", extendsTypes)}");
            }
        }

        AppendWhereClauses(parts, ctx, generics);
        return string.Join(" ", parts);
    }

    // ---- Enum ----

    /// <summary>Shapes an enum declaration (the shared default).</summary>
    public static void Enum(ShapeContext ctx, CodeEnum enm, string headerPrefix)
    {
        string header = string.Join(" ", new[] { headerPrefix, ctx.Syntax.KeywordEnum, enm.Name }.Where(s => s.Length > 0));
        if (ctx.Syntax.BlockStyle == BlockStyle.Indentation)
        {
            ctx.Writer.WriteLine($"{header} {{");
            ctx.Writer.Indent();
        }
        else
        {
            ShapeBlocks.Open(ctx, header);
        }

        for (int i = 0; i < enm.Members.Count; i++)
        {
            CodeEnumMember member = enm.Members[i];
            string line = member.Value is null ? member.Name : $"{member.Name} = {member.Value}";
            bool last = i == enm.Members.Count - 1;
            ctx.Writer.WriteLine(last && !ctx.Syntax.EnumTrailingComma ? line : line + ",");
        }

        if (ctx.Syntax.BlockStyle == BlockStyle.Indentation)
        {
            ctx.Writer.Dedent();
            ctx.Writer.WriteLine("}");
        }
        else
        {
            ctx.Writer.CloseBlock(ctx.Syntax.EnumCloseSuffix);
        }
    }

    // ---- Statements ----

    /// <summary>Shapes an if/else-if/else statement (the shared default).</summary>
    public static void If(ShapeContext ctx, CodeIf ifStatement)
    {
        List<(string Header, List<CodeStatement> Body)> blocks =
        [
            (FormatCondition(ctx, "if", ifStatement.Condition), ifStatement.Then),
            .. ifStatement.ElseIfs.Select(e => (FormatCondition(ctx, ctx.Syntax.KeywordElseIf, e.Condition), e.Body))
        ];
        if (ifStatement.Else.Count > 0)
        {
            blocks.Add(("else", ifStatement.Else));
        }

        if (ctx.Syntax.BlockStyle == BlockStyle.Indentation)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                RenderBlock(ctx, blocks[i].Header, blocks[i].Body);
            }

            return;
        }

        RenderChain(ctx, blocks);
    }

    /// <summary>Shapes a foreach statement (the shared default).</summary>
    public static void ForEach(ShapeContext ctx, string variable, string iterable, List<CodeStatement> body)
    {
        string loopVariable = ctx.Syntax.ForEachVariable.Length > 0 ? $"{ctx.Syntax.ForEachVariable} {variable}" : variable;
        string header = ctx.Syntax.ConditionParens
            ? $"{ctx.Syntax.ForEachKeyword} ({loopVariable} {ctx.Syntax.ForEachConnector} {iterable})"
            : $"{ctx.Syntax.ForEachKeyword} {loopVariable} {ctx.Syntax.ForEachConnector} {iterable}";

        RenderBlock(ctx, header, body);
    }

    /// <summary>Shapes a while statement (the shared default).</summary>
    public static void While(ShapeContext ctx, string condition, List<CodeStatement> body)
    {
        RenderBlock(ctx, FormatCondition(ctx, "while", condition), body);
    }

    /// <summary>Shapes a C-style for statement (the shared default).</summary>
    public static void For(ShapeContext ctx, string initializer, string condition, string increment, List<CodeStatement> body)
    {
        string header = ctx.Syntax.ConditionParens
            ? $"for ({initializer}; {condition}; {increment})"
            : $"for {initializer}; {condition}; {increment}";

        RenderBlock(ctx, header, body);
    }

    /// <summary>Shapes a do/while statement (the shared default).</summary>
    public static void DoWhile(ShapeContext ctx, string condition, List<CodeStatement> body)
    {
        if (ctx.Syntax.BlockStyle == BlockStyle.Indentation)
        {
            RenderBlock(ctx, "do", body);
            RenderBlock(ctx, FormatCondition(ctx, "while", condition), []);
            return;
        }

        ShapeBlocks.Open(ctx, "do");
        ctx.RenderStatements(body);

        if (ctx.Writer.Options.BraceStyle == BraceStyle.KAndR)
        {
            ctx.Writer.CloseBlockInline($" {FormatCondition(ctx, "while", condition)}");
            ctx.Writer.WriteStatement(string.Empty);
        }
        else
        {
            ShapeBlocks.Close(ctx);
            ctx.Writer.WriteStatement(FormatCondition(ctx, "while", condition));
        }
    }

    /// <summary>Shapes a match/switch statement (the shared default).</summary>
    public static void Match(ShapeContext ctx, CodeMatch match)
    {
        string header = ctx.Syntax.ConditionParens
            ? $"{ctx.Syntax.KeywordMatch} ({match.Expression})"
            : $"{ctx.Syntax.KeywordMatch} {match.Expression}";

        ShapeBlocks.Open(ctx, header);

        foreach (CodeMatchArm arm in match.Arms)
        {
            if (ctx.Syntax.MatchArmStyle == MatchArmStyle.Colon)
            {
                ctx.Writer.WriteLine($"{arm.Pattern}:");
                ctx.Writer.Indent();
                ctx.RenderStatements(arm.Body);
                ctx.Writer.Dedent();
                continue;
            }

            string armHeader = arm.Pattern == "_" ? ctx.Syntax.KeywordDefaultArm : $"case {arm.Pattern}:";
            ctx.Writer.WriteLine(armHeader);
            ctx.Writer.Indent();
            ctx.RenderStatements(arm.Body);
            ctx.Writer.WriteStatement("break");
            ctx.Writer.Dedent();
        }

        ShapeBlocks.Close(ctx);
    }

    /// <summary>Shapes a try/catch/finally statement (the shared default).</summary>
    public static void Try(ShapeContext ctx, CodeTry tryStatement)
    {
        List<(string Header, List<CodeStatement> Body)> blocks =
        [
            ("try", tryStatement.Body),
            .. tryStatement.Catches.Select(c => (CatchHeader(ctx, c), c.Body))
        ];
        if (tryStatement.Finally.Count > 0)
        {
            blocks.Add(("finally", tryStatement.Finally));
        }

        RenderChain(ctx, blocks);
    }

    private static string CatchHeader(ShapeContext ctx, CodeCatch catchBlock)
    {
        if (ctx.Syntax.ParameterStyle == ParameterStyle.TypeFirst)
        {
            if (catchBlock.Type is not null)
            {
                return catchBlock.Variable is null
                    ? $"catch ({ctx.Syntax.MapType(catchBlock.Type)})"
                    : $"catch ({ctx.Syntax.MapType(catchBlock.Type)} {catchBlock.Variable})";
            }

            return catchBlock.Variable is null
                ? "catch"
                : $"catch (Exception {catchBlock.Variable})";
        }

        return catchBlock.Variable is null ? "catch" : $"catch ({catchBlock.Variable})";
    }

    /// <summary>Shapes a raw block statement (the shared default).</summary>
    public static void RawBlock(ShapeContext ctx, string header, List<CodeStatement> body, string? closeSuffix)
    {
        if (ctx.Syntax.BlockStyle == BlockStyle.Indentation)
        {
            RenderBlock(ctx, header, body);
            return;
        }

        ShapeBlocks.Open(ctx, header);
        ctx.RenderStatements(body);
        ctx.Writer.CloseBlock(closeSuffix);
    }

    /// <summary>Shapes a local variable declaration (the shared default).</summary>
    public static string? LocalVariable(ShapeContext ctx, CodeLocalVariable variable)
    {
        if (ctx.Syntax.ParameterStyle == ParameterStyle.TypeFirst && variable.Type is null && variable.Value is null)
        {
            ctx.Warnings.Add($"Local variable '{variable.Name}' has no type or value; skipped.");
            return null;
        }

        List<string> parts = [];

        if (ctx.Syntax.ParameterStyle == ParameterStyle.TypeFirst)
        {
            if (variable.IsConst)
            {
                parts.Add(ctx.Syntax.ConstKeyword);
            }

            string declaration = variable.Type is not null
                ? $"{ctx.Syntax.MapType(variable.Type)} {variable.Name}"
                : $"{ctx.Syntax.LocalVarKeyword} {variable.Name}";

            if (variable.Value is not null)
            {
                declaration += $" = {variable.Value}";
            }

            parts.Add(declaration);
            return string.Join(" ", parts);
        }

        parts.Add(variable.IsConst ? ctx.Syntax.ConstKeyword : ctx.Syntax.LocalVarKeyword);

        string line = variable.Name;
        if (variable.Type is not null)
        {
            line += $": {ctx.Syntax.MapType(variable.Type)}";
        }

        if (variable.Value is not null)
        {
            line += $" = {variable.Value}";
        }

        parts.Add(line);
        return string.Join(" ", parts);
    }

    // ---- Object literal ----

    /// <summary>Shapes an object literal statement (the shared default).</summary>
    public static void ObjectLiteral(ShapeContext ctx, CodeObjectLiteral literal)
    {
        ctx.Writer.WriteLine(literal.Prefix is null or "" ? "{" : $"{literal.Prefix} {{");
        ctx.Writer.Indent();
        RenderObjectMembers(ctx, literal.Members);
        ctx.Writer.Dedent();
        ctx.Writer.WriteStatement("}");
    }

    private static void RenderObjectMembers(ShapeContext ctx, List<CodeObjectMember> members)
    {
        foreach (CodeObjectMember member in members)
        {
            if (member.Expression is not null)
            {
                ctx.Writer.WriteLine($"{member.Name}: {member.Expression},");
                continue;
            }

            if (member.Object is not null)
            {
                ctx.Writer.WriteLine($"{member.Name}: {{");
                ctx.Writer.Indent();
                RenderObjectMembers(ctx, member.Object.Members);
                ctx.Writer.Dedent();
                ctx.Writer.WriteLine("},");
                continue;
            }

            if (member.Function is not null)
            {
                RenderObjectMemberFunction(ctx, member.Function);
            }
        }
    }

    private static void RenderObjectMemberFunction(ShapeContext ctx, CodeNestedFunction function)
    {
        if (function.DocComment is not null)
        {
            ctx.Writer.WriteDocComment(function.DocComment.Text);
        }

        string header = string.Join(" ", [.. ctx.Syntax.BuildModifierPrefix(function.Modifiers),
            ctx.Shapes.Signature(ctx, SignatureKind.Method, function.Name, function.Parameters, function.Generics, function.ReturnType, className: string.Empty, interfaceMode: false)]);

        ShapeBlocks.Open(ctx, header);
        ctx.RenderStatements(function.Body?.Statements ?? []);
        ShapeBlocks.Close(ctx, ",");
    }

    // ---- Shared helpers ----

    private static string BuildParamList(ShapeContext ctx, List<CodeParameter> parameters, bool interfaceMode = false)
    {
        return string.Join(", ", parameters.Select(p =>
        {
            if (ctx.Syntax.ParameterStyle == ParameterStyle.TypeFirst)
            {
                string result = p.Name;
                if (p.Type is not null)
                {
                    result = $"{ctx.Syntax.MapType(p.Type)}{(ctx.Syntax.SupportsNullable && p.Nullable ? "?" : "")} {result}";
                }

                if (p.DefaultValue is not null)
                {
                    result += $" = {p.DefaultValue}";
                }

                return result;
            }

            string name = p.Name;
            if (ctx.Syntax.SupportsNullable && (p.Nullable || (interfaceMode && p.DefaultValue is not null)))
            {
                name += "?";
            }

            if (p.Type is not null)
            {
                name += $": {ctx.Syntax.MapType(p.Type)}";
            }

            if (!interfaceMode && p.DefaultValue is not null)
            {
                name += $" = {p.DefaultValue}";
            }

            return name;
        }));
    }

    private static string BuildNameWithGenerics(ShapeContext ctx, string name, List<CodeGenericParameter> generics)
    {
        if (generics.Count == 0)
        {
            return name;
        }

        if (ctx.Syntax.GenericStyle == GenericStyle.None)
        {
            return name;
        }

        if (ctx.Syntax.GenericStyle == GenericStyle.Inline)
        {
            return name + "<" + string.Join(", ", generics.Select(g =>
            {
                string result = g.Name;
                if (g.Constraint is not null)
                {
                    result += $" extends {g.Constraint}";
                }

                if (g.DefaultValue is not null)
                {
                    result += $" = {g.DefaultValue}";
                }

                return result;
            })) + ">";
        }

        return name + "<" + string.Join(", ", generics.Select(g => g.Name)) + ">";
    }

    private static void AppendWhereClauses(List<string> parts, ShapeContext ctx, List<CodeGenericParameter> generics)
    {
        if (ctx.Syntax.GenericStyle != GenericStyle.WhereClauses)
        {
            return;
        }

        string whereClauses = string.Join(" ", generics
            .Where(g => g.Constraint is not null)
            .Select(g => $"where {g.Name} : {g.Constraint}"));

        if (whereClauses.Length > 0)
        {
            parts.Add(whereClauses);
        }
    }

    private static string FormatCondition(ShapeContext ctx, string keyword, string condition)
    {
        return ctx.Syntax.ConditionParens ? $"{keyword} ({condition})" : $"{keyword} {condition}";
    }

    private static void RenderBlock(ShapeContext ctx, string header, List<CodeStatement> statements)
    {
        ShapeBlocks.Open(ctx, header);
        ctx.RenderStatements(statements);
        ShapeBlocks.Close(ctx);
    }

    private static void RenderChain(ShapeContext ctx, List<(string Header, List<CodeStatement> Body)> blocks)
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            (string header, List<CodeStatement> body) = blocks[i];

            if (i == 0)
            {
                ShapeBlocks.Open(ctx, header);
            }
            else if (ctx.Writer.Options.BraceStyle == BraceStyle.KAndR)
            {
                ctx.Writer.CloseBlockInline($" {header}");
                ShapeBlocks.Open(ctx, string.Empty);
            }
            else
            {
                ShapeBlocks.Close(ctx);
                ShapeBlocks.Open(ctx, header);
            }

            ctx.RenderStatements(body);
        }

        ShapeBlocks.Close(ctx);
    }
}
