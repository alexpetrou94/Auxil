using Auxil.CodeGeneration.Common.Languages;

namespace Auxil.CodeGeneration.Common.Rendering;

/// <summary>
/// The node dispatch table: for every model node kind it knows the
/// gating feature trait, the unsupported-feature warning, and the shared
/// renderer. A language renders a node only if it implements the node's
/// trait; anything else is warned and skipped — the render-time safety
/// net behind the compile-time gating of the builder API.
/// </summary>
internal static class NodeFeatures
{
    private static readonly Type Signals = typeof(IHasSignals);
    private static readonly Type Imports = typeof(IHasImports);
    private static readonly Type Namespaces = typeof(IHasNamespaces);
    private static readonly Type Interfaces = typeof(IHasInterfaces);
    private static readonly Type TopLevelFunctions = typeof(IHasTopLevelFunctions);
    private static readonly Type InnerClasses = typeof(IHasInnerClasses);
    private static readonly Type ExceptionHandling = typeof(IHasExceptionHandling);
    private static readonly Type DoWhile = typeof(IHasDoWhile);
    private static readonly Type CStyleFor = typeof(IHasCStyleFor);
    private static readonly Type Throw = typeof(IHasThrow);
    private static readonly Type Match = typeof(IHasMatch);
    private static readonly Type Constructors = typeof(IHasConstructors);
    private static readonly Type TypeAliases = typeof(IHasTypeAliases);
    private static readonly Type NestedFunctions = typeof(IHasNestedFunctions);
    private static readonly Type ObjectLiterals = typeof(IHasObjectLiterals);

    /// <summary>Whether the node can render at all in this language (capability check only).</summary>
    public static bool CanRender(RenderContext ctx, CodeNode node, NodeContext context)
    {
        Type? trait = GatingTrait(node.GetType(), context);
        return trait is null || ctx.Profile.Supports(trait);
    }

    /// <summary>
    /// Whether the node will render in this context: capability, context
    /// policy, and interface-member rules all pass. No warnings are
    /// emitted — use this for layout decisions ahead of rendering.
    /// </summary>
    public static bool CanRenderInContext(RenderContext ctx, CodeNode node, NodeContext context)
    {
        Type nodeType = node.GetType();

        Type? trait = GatingTrait(nodeType, context);
        if (trait is not null && !ctx.Profile.Supports(trait))
        {
            return false;
        }

        if (context == NodeContext.InterfaceMember && !IsInterfaceMember(nodeType))
        {
            return false;
        }

        return ContextPolicy.ForbiddenIn(ctx, node, context) is null;
    }

    /// <summary>Whether the statement can render at all in this language (capability check only).</summary>
    public static bool CanRender(RenderContext ctx, CodeStatement statement)
    {
        Type? trait = GatingTrait(statement.GetType(), NodeContext.Statement);
        return trait is null || ctx.Profile.Supports(trait);
    }

    /// <summary>
    /// Checks capability and context, then renders the node through the
    /// shared renderer. Returns true when the node was rendered.
    /// </summary>
    public static bool Render(RenderContext ctx, CodeNode node, NodeContext context, string? className = null)
    {
        Type nodeType = node.GetType();

        Type? trait = GatingTrait(nodeType, context);
        if (trait is not null && !ctx.Profile.Supports(trait))
        {
            ctx.Warnings.Add(UnsupportedWarning(ctx, nodeType, node));
            return false;
        }

        if (context == NodeContext.InterfaceMember && !IsInterfaceMember(nodeType))
        {
            ctx.Warnings.Add($"{nodeType.Name} is not supported in a {ctx.LanguageName} interface; skipped.");
            return false;
        }

        if (ContextPolicy.ForbiddenIn(ctx, node, context) is { } contextWarning)
        {
            ctx.Warnings.Add(contextWarning);
            return false;
        }

        RenderNode(ctx, node, context, className);
        return true;
    }

    /// <summary>
    /// Checks the statement's capability, then renders it through the
    /// shared renderer.
    /// </summary>
    public static void Render(RenderContext ctx, CodeStatement statement)
    {
        Type nodeType = statement.GetType();

        Type? trait = GatingTrait(nodeType, NodeContext.Statement);
        if (trait is not null && !ctx.Profile.Supports(trait))
        {
            ctx.Warnings.Add(UnsupportedWarning(ctx, nodeType, statement));
            return;
        }

        StatementRenderer.RenderStatement(ctx, statement);
    }

    private static Type? GatingTrait(Type nodeType, NodeContext context)
    {
        if (nodeType == typeof(CodeSignal))
        {
            return Signals;
        }

        if (nodeType == typeof(CodeImport))
        {
            return Imports;
        }

        if (nodeType == typeof(CodeNamespace))
        {
            return Namespaces;
        }

        if (nodeType == typeof(CodeInterface))
        {
            return Interfaces;
        }

        if (nodeType == typeof(CodeFunction))
        {
            return TopLevelFunctions;
        }

        if (nodeType == typeof(CodeTry))
        {
            return ExceptionHandling;
        }

        if (nodeType == typeof(CodeDoWhile))
        {
            return DoWhile;
        }

        if (nodeType == typeof(CodeFor))
        {
            return CStyleFor;
        }

        if (nodeType == typeof(CodeThrow))
        {
            return Throw;
        }

        if (nodeType == typeof(CodeMatch))
        {
            return Match;
        }

        if (nodeType == typeof(CodeConstructor))
        {
            return Constructors;
        }

        if (nodeType == typeof(CodeTypeAlias))
        {
            return TypeAliases;
        }

        if (nodeType == typeof(CodeNestedFunction))
        {
            return NestedFunctions;
        }

        if (nodeType == typeof(CodeObjectLiteral))
        {
            return ObjectLiterals;
        }

        if (nodeType == typeof(CodeClass) && context == NodeContext.ClassMember)
        {
            return InnerClasses;
        }

        return null;
    }

    private static string UnsupportedWarning(RenderContext ctx, Type nodeType, object node)
    {
        switch (node)
        {
            case CodeSignal signal:
                return $"Signal '{signal.Name}' is not supported in {ctx.LanguageName}; skipped.";
            case CodeImport import:
                return $"Import '{import.From}' is not supported in {ctx.LanguageName}; skipped.";
            case CodeNamespace namespaceNode:
                return $"Namespace '{namespaceNode.Name}' is not supported in {ctx.LanguageName}; skipped.";
            case CodeInterface:
                return $"Interfaces are not supported in {ctx.LanguageName}; skipped.";
            case CodeFunction function:
                return $"Top-level function '{function.Name}' is not supported in {ctx.LanguageName}; skipped. Use Method() instead.";
            case CodeTry:
                return $"Try/catch statements are not supported in {ctx.LanguageName}; skipped.";
            case CodeDoWhile:
                return $"Do/while statements are not supported in {ctx.LanguageName}; skipped.";
            case CodeFor:
                return $"C-style for statements are not supported in {ctx.LanguageName}; skipped. Use ForEach with range() instead.";
            case CodeThrow:
                return $"Throw statements are not supported in {ctx.LanguageName}; skipped.";
            case CodeMatch:
                return $"Match statements are not supported in {ctx.LanguageName}; skipped.";
            case CodeClass cls:
                return $"Nested class '{cls.Name}' is not supported in {ctx.LanguageName}; skipped.";
            case CodeConstructor:
                return $"Constructors are not supported in {ctx.LanguageName}; skipped.";
            case CodeTypeAlias:
                return $"Type aliases are not supported in {ctx.LanguageName}; skipped.";
            case CodeNestedFunction:
                return $"Nested functions are not supported in {ctx.LanguageName}; skipped.";
            case CodeObjectLiteral:
                return $"Object literals are not supported in {ctx.LanguageName}; skipped.";
            default:
                return $"{nodeType.Name} is not supported in {ctx.LanguageName}; skipped.";
        }
    }

    private static bool IsInterfaceMember(Type nodeType)
    {
        return nodeType == typeof(CodeComment)
            || nodeType == typeof(CodeField)
            || nodeType == typeof(CodeProperty)
            || nodeType == typeof(CodeMethod);
    }

    private static void RenderNode(RenderContext ctx, CodeNode node, NodeContext context, string? className)
    {
        switch (node)
        {
            case CodeComment comment:
                ctx.Writer.WriteComment(comment.Text);
                break;
            case CodeField field:
                if (context == NodeContext.InterfaceMember)
                {
                    DeclarationRenderer.RenderInterfaceField(ctx, field);
                }
                else
                {
                    DeclarationRenderer.RenderField(ctx, field, context == NodeContext.FileLevel);
                }

                break;
            case CodeProperty property:
                if (context == NodeContext.InterfaceMember)
                {
                    DeclarationRenderer.RenderInterfaceProperty(ctx, property);
                }
                else
                {
                    DeclarationRenderer.RenderProperty(ctx, property, context == NodeContext.FileLevel);
                }

                break;
            case CodeSignal signal:
                DeclarationRenderer.RenderSignal(ctx, signal);
                break;
            case CodeEnum enm:
                DeclarationRenderer.RenderEnum(ctx, enm);
                break;
            case CodeClass cls:
                DeclarationRenderer.RenderClass(ctx, cls);
                break;
            case CodeInterface iface:
                DeclarationRenderer.RenderInterface(ctx, iface);
                break;
            case CodeMethod method:
                if (context == NodeContext.InterfaceMember)
                {
                    DeclarationRenderer.RenderInterfaceMethod(ctx, method);
                }
                else
                {
                    DeclarationRenderer.RenderMethod(ctx, method);
                }

                break;
            case CodeConstructor ctor:
                DeclarationRenderer.RenderConstructor(ctx, ctor, className ?? string.Empty);
                break;
            case CodeFunction function:
                DeclarationRenderer.RenderFunction(ctx, function);
                break;
            case CodeNamespace namespaceNode:
                DeclarationRenderer.RenderNamespace(ctx, namespaceNode);
                break;
            case CodeImport import:
                DeclarationRenderer.RenderImport(ctx, import);
                break;
            case CodeTypeAlias alias:
                DeclarationRenderer.RenderTypeAlias(ctx, alias);
                break;
        }
    }
}
