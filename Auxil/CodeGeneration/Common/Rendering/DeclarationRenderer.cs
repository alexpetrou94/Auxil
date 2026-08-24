using Auxil.CodeGeneration.Common.Syntax;
using Auxil.CodeGeneration.Common.Syntax.Shapes;

namespace Auxil.CodeGeneration.Common.Rendering;

/// <summary>
/// Shared declaration renderer flow: classes, interfaces, enums,
/// namespaces, signals, imports, fields, properties, methods,
/// constructors and functions. The flow handles doc comments,
/// annotations, modifier prefixes, warnings, blank lines and blocks;
/// each construct's look is delegated to the language's shape hooks.
/// Capability and context checks come from the feature table.
/// </summary>
internal static class DeclarationRenderer
{
    // ---- Class ----

    public static void RenderClass(RenderContext ctx, CodeClass cls)
    {
        WriteDocComment(ctx, cls);
        WriteAnnotations(ctx, cls.Annotations);
        WarnUnsupportedModifiers(ctx, cls);
        WarnGenerics(ctx, cls.Generics, $"class {cls.Name}");

        string header = string.Join(" ", [.. ctx.Syntax.BuildModifierPrefix(cls.Modifiers),
            ctx.Shapes.ClassHeader(ctx.Shape, cls.Name, cls.Generics, cls.ExtendsType, cls.ImplementsTypes)]);

        ShapeBlocks.Open(ctx.Shape, header);

        RenderMembers(ctx, cls.Members, cls.Name);

        ShapeBlocks.Close(ctx.Shape);
    }

    // ---- Interface ----

    public static void RenderInterface(RenderContext ctx, CodeInterface iface)
    {
        WriteDocComment(ctx, iface);
        WriteAnnotations(ctx, iface.Annotations);
        WarnUnsupportedModifiers(ctx, iface);
        WarnGenerics(ctx, iface.Generics, $"interface {iface.Name}");

        string header = string.Join(" ", [.. ctx.Syntax.BuildModifierPrefix(iface.Modifiers),
            ctx.Shapes.InterfaceHeader(ctx.Shape, iface.Name, iface.Generics, iface.ExtendsTypes)]);

        ShapeBlocks.Open(ctx.Shape, header);

        CodeNode? previous = null;
        foreach (CodeNode member in iface.Members)
        {
            bool renders = NodeFeatures.CanRenderInContext(ctx, member, NodeContext.InterfaceMember);
            if (renders && previous is not null && !IsRelatedDataMember(member, previous, ctx))
            {
                ctx.Writer.BlankLine();
            }

            if (NodeFeatures.Render(ctx, member, NodeContext.InterfaceMember))
            {
                previous = member;
            }
        }

        ShapeBlocks.Close(ctx.Shape);
    }

    internal static void RenderInterfaceField(RenderContext ctx, CodeField field)
    {
        WriteDocComment(ctx, field);
        WriteAnnotations(ctx, field.Annotations);

        string text = ctx.Shapes.Field(ctx.Shape, field.Name, field.Type, field.Nullable, field.DefaultValue, isConst: false, keyword: string.Empty, isInterfaceMember: true);
        if (ctx.Syntax.PropertyStyle == PropertyStyle.AsField)
        {
            ctx.Writer.WriteStatement(text);
        }
        else
        {
            ctx.Writer.WriteLine(text);
        }
    }

    internal static void RenderInterfaceProperty(RenderContext ctx, CodeProperty property)
    {
        WriteDocComment(ctx, property);
        WriteAnnotations(ctx, property.Annotations);

        ctx.Shapes.Property(ctx.Shape, property, prefix: string.Empty, isTopLevel: false, isInterfaceMember: true);
    }

    internal static void RenderInterfaceMethod(RenderContext ctx, CodeMethod method)
    {
        WriteDocComment(ctx, method);
        WriteAnnotations(ctx, method.Annotations);
        WarnUnsupportedModifiers(ctx, method);
        WarnGenerics(ctx, method.Generics, $"method {method.Name}");
        bool interfaceMode = ctx.Syntax.ParameterStyle == ParameterStyle.NameFirst;
        ctx.Writer.WriteStatement(ctx.Shapes.Signature(ctx.Shape, SignatureKind.Method, method.Name, method.Parameters, method.Generics, method.ReturnType, className: string.Empty, interfaceMode));
    }

    // ---- Enum ----

    public static void RenderEnum(RenderContext ctx, CodeEnum enm)
    {
        WriteDocComment(ctx, enm);
        WriteAnnotations(ctx, enm.Annotations);
        WarnUnsupportedModifiers(ctx, enm);

        string prefix = string.Join(" ", ctx.Syntax.BuildModifierPrefix(enm.Modifiers));
        ctx.Shapes.Enum(ctx.Shape, enm, prefix);
    }

    // ---- Namespace ----

    public static void RenderNamespace(RenderContext ctx, CodeNamespace namespaceNode)
    {
        ctx.Writer.WriteStatement($"namespace {namespaceNode.Name}");
        ctx.Writer.BlankLine();

        bool wroteAnything = false;
        foreach (CodeNode child in namespaceNode.Children)
        {
            if (wroteAnything)
            {
                ctx.Writer.BlankLine();
            }

            if (NodeFeatures.Render(ctx, child, NodeContext.FileLevel))
            {
                wroteAnything = true;
            }
        }
    }

    // ---- Signal ----

    public static void RenderSignal(RenderContext ctx, CodeSignal signal)
    {
        WriteDocComment(ctx, signal);
        ctx.Writer.WriteLine($"{ctx.Syntax.KeywordSignal} {signal.Name}({BuildParamList(ctx, signal.Parameters)})");
    }

    // ---- Import ----

    public static void RenderImport(RenderContext ctx, CodeImport import)
    {
        if (ctx.Syntax.ImportStyle == ImportStyle.Using)
        {
            ctx.Writer.WriteStatement(import.DefaultName is not null
                ? $"using {import.DefaultName} = {import.From}"
                : $"using {import.From}");
            return;
        }

        List<string> parts = [];
        if (import.DefaultName is not null)
        {
            parts.Add(import.DefaultName);
        }

        if (import.NamedNames.Count > 0)
        {
            string namesList = string.Join(", ", import.NamedNames);
            parts.Add(import.DefaultName is not null ? $", {{ {namesList} }}" : $"{{ {namesList} }}");
        }

        string body = parts.Count > 0 ? string.Join(string.Empty, parts) + " from " : string.Empty;
        ctx.Writer.WriteStatement($"import {body}\"{import.From}\"");
    }

    // ---- Type alias ----

    public static void RenderTypeAlias(RenderContext ctx, CodeTypeAlias alias)
    {
        WriteDocComment(ctx, alias);
        WriteAnnotations(ctx, alias.Annotations);
        WarnUnsupportedModifiers(ctx, alias);

        string line = string.Join(" ", [.. ctx.Syntax.BuildModifierPrefix(alias.Modifiers),
            $"{ctx.Syntax.KeywordTypeAlias} {alias.Name} = {alias.Definition}"]);
        ctx.Writer.WriteStatement(line);
    }

    // ---- Field ----

    public static void RenderField(RenderContext ctx, CodeField field, bool isTopLevel = false)
    {
        WriteDocComment(ctx, field);
        WriteAnnotations(ctx, field.Annotations);
        WarnUnsupportedModifiers(ctx, field);

        string keyword = isTopLevel ? ctx.Syntax.TopLevelFieldKeyword : ctx.Syntax.FieldKeyword;
        string declaration = ctx.Shapes.Field(ctx.Shape, field.Name, field.Type, field.Nullable, field.DefaultValue, field.Modifiers.HasFlag(CodeModifier.Const), keyword, isInterfaceMember: false);
        ctx.Writer.WriteStatement(string.Join(" ", [.. ctx.Syntax.BuildModifierPrefix(field.Modifiers), declaration]));
    }

    // ---- Property ----

    public static void RenderProperty(RenderContext ctx, CodeProperty property, bool isTopLevel = false)
    {
        WriteDocComment(ctx, property);
        WriteAnnotations(ctx, property.Annotations);
        WarnUnsupportedModifiers(ctx, property);

        string prefix = string.Join(" ", ctx.Syntax.BuildModifierPrefix(property.Modifiers));
        ctx.Shapes.Property(ctx.Shape, property, prefix, isTopLevel, isInterfaceMember: false);
    }

    // ---- Method / Constructor / Function ----

    public static void RenderMethod(RenderContext ctx, CodeMethod method)
    {
        WriteDocComment(ctx, method);
        WriteAnnotations(ctx, method.Annotations);
        WarnUnsupportedModifiers(ctx, method);
        WarnGenerics(ctx, method.Generics, $"method {method.Name}");

        string header = string.Join(" ", [.. ctx.Syntax.BuildModifierPrefix(method.Modifiers),
            ctx.Shapes.Signature(ctx.Shape, SignatureKind.Method, method.Name, method.Parameters, method.Generics, method.ReturnType, className: string.Empty, interfaceMode: false)]);

        if (method.Modifiers.HasFlag(CodeModifier.Abstract))
        {
            ctx.Writer.WriteStatement(header);
            return;
        }

        ShapeBlocks.Open(ctx.Shape, header);
        StatementRenderer.RenderBody(ctx, method.Body);
        ShapeBlocks.Close(ctx.Shape);
    }

    public static void RenderConstructor(RenderContext ctx, CodeConstructor ctor, string className)
    {
        WriteDocComment(ctx, ctor);
        WriteAnnotations(ctx, ctor.Annotations);
        WarnUnsupportedModifiers(ctx, ctor);

        string header = string.Join(" ", [.. ctx.Syntax.BuildModifierPrefix(ctor.Modifiers),
            ctx.Shapes.Signature(ctx.Shape, SignatureKind.Constructor, string.Empty, ctor.Parameters, [], null, className, interfaceMode: false)]);

        ShapeBlocks.Open(ctx.Shape, header);
        StatementRenderer.RenderBody(ctx, ctor.Body);
        ShapeBlocks.Close(ctx.Shape);
    }

    public static void RenderFunction(RenderContext ctx, CodeFunction function)
    {
        WriteDocComment(ctx, function);
        WriteAnnotations(ctx, function.Annotations);
        WarnUnsupportedModifiers(ctx, function);

        string header = string.Join(" ", [.. ctx.Syntax.BuildModifierPrefix(function.Modifiers),
            ctx.Shapes.Signature(ctx.Shape, SignatureKind.Function, function.Name, function.Parameters, function.Generics, function.ReturnType, className: string.Empty, interfaceMode: false)]);

        ShapeBlocks.Open(ctx.Shape, header);
        StatementRenderer.RenderBody(ctx, function.Body);
        ShapeBlocks.Close(ctx.Shape);
    }

    // ---- Shared helpers ----

    private static string BuildParamList(RenderContext ctx, List<CodeParameter> parameters)
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
            if (p.Type is not null)
            {
                name += $": {ctx.Syntax.MapType(p.Type)}";
            }

            if (p.DefaultValue is not null)
            {
                name += $" = {p.DefaultValue}";
            }

            return name;
        }));
    }

    private static void WarnGenerics(RenderContext ctx, List<CodeGenericParameter> generics, string context)
    {
        if (generics.Count > 0 && ctx.Syntax.GenericStyle == GenericStyle.None)
        {
            ctx.Warnings.Add($"Generics on {context} are not supported in {ctx.LanguageName}; skipped.");
        }

        if (ctx.Syntax.GenericStyle == GenericStyle.WhereClauses)
        {
            foreach (CodeGenericParameter generic in generics.Where(g => g.DefaultValue is not null))
            {
                ctx.Warnings.Add($"Default generic type argument on '{generic.Name}' is not supported in {ctx.LanguageName}; skipped.");
            }
        }
    }

        private static void WarnUnsupportedModifiers(RenderContext ctx, CodeMember member)
    {
        foreach (CodeModifier modifier in Enum.GetValues<CodeModifier>())
        {
            if (modifier is CodeModifier.None or CodeModifier.Const)
            {
                continue;
            }

            if (member.Modifiers.HasFlag(modifier) && !ctx.Syntax.IsModifierSupported(modifier))
            {
                ctx.Warnings.Add($"{modifier} modifier is not supported in {ctx.LanguageName}; skipped.");
            }
        }
    }

    private static void WriteDocComment(RenderContext ctx, CodeMember member)
    {
        WriteDocComment(ctx, member.DocComment);
    }

    internal static void WriteDocComment(RenderContext ctx, CodeComment? comment)
    {
        if (comment is not null)
        {
            ctx.Writer.WriteDocComment(comment.Text);
        }
    }

    internal static void WriteAnnotations(RenderContext ctx, IEnumerable<CodeAnnotation> annotations)
    {
        foreach (CodeAnnotation annotation in annotations)
        {
            string body = annotation.Arguments.Count > 0
                ? $"{annotation.Name}({string.Join(", ", annotation.Arguments)})"
                : annotation.Name;

            ctx.Writer.WriteLine(ctx.Syntax.AnnotationStyle == AnnotationStyle.Square ? $"[{body}]" : $"@{body}");
        }
    }

    private static void RenderMembers(RenderContext ctx, List<CodeNode> members, string className)
    {
        if (ctx.Syntax.BlockStyle == BlockStyle.Indentation && (members.Count == 0 || !members.Any(m => NodeFeatures.CanRender(ctx, m, NodeContext.ClassMember))))
        {
            ctx.Writer.WriteLine(ctx.Syntax.KeywordPass);
        }

        CodeNode? previous = null;
        foreach (CodeNode member in members)
        {
            bool renders = NodeFeatures.CanRenderInContext(ctx, member, NodeContext.ClassMember);
            if (renders && previous is not null && !IsRelatedDataMember(member, previous, ctx))
            {
                ctx.Writer.BlankLine();
            }

            if (NodeFeatures.Render(ctx, member, NodeContext.ClassMember, className))
            {
                previous = member;
            }
        }
    }

    private static bool IsRelatedDataMember(CodeNode node, CodeNode previous, RenderContext ctx)
    {
        if (previous is CodeComment)
        {
            return true;
        }

        bool includeProperties = ctx.Syntax.BlockStyle != BlockStyle.Indentation;
        bool isDataMember = node is CodeField || (includeProperties && node is CodeProperty);
        bool previousIsDataMember = previous is CodeField || (includeProperties && previous is CodeProperty);

        if (!isDataMember || !previousIsDataMember)
        {
            return false;
        }

        if (previous is CodeMember previousMember)
        {
            return previousMember.Annotations.Count == 0 && previousMember.DocComment is null;
        }

        return true;
    }
}
