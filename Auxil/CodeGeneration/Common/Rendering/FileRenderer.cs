using Auxil.CodeGeneration.Common.Syntax;

namespace Auxil.CodeGeneration.Common.Rendering;

/// <summary>
/// Shared file renderer: script-class layout (GDScript class_name/
/// extends) or module layout (imports + top-level declarations).
/// </summary>
internal static class FileRenderer
{
    public static void RenderFile(RenderContext ctx, CodeFile file)
    {
        if (ctx.Syntax.FileLayout == FileLayoutStyle.ScriptClass)
        {
            RenderScriptClass(ctx, file);
            return;
        }

        RenderModule(ctx, file);
    }

    private static void RenderScriptClass(RenderContext ctx, CodeFile file)
    {
        DeclarationRenderer.WriteDocComment(ctx, file.DocComment);
        DeclarationRenderer.WriteAnnotations(ctx, file.Annotations);

        bool hasHeader = false;
        if (file.ClassName is not null)
        {
            ctx.Writer.WriteLine($"class_name {file.ClassName}");
            hasHeader = true;
        }

        if (file.ExtendsType is not null)
        {
            ctx.Writer.WriteLine($"extends {file.ExtendsType}");
            hasHeader = true;
        }

        if (hasHeader && file.Children.Count > 0)
        {
            ctx.Writer.BlankLine();
        }

        RenderNodes(ctx, file.Children, file.ClassName);
    }

    private static void RenderModule(RenderContext ctx, CodeFile file)
    {
        DeclarationRenderer.WriteDocComment(ctx, file.DocComment);
        DeclarationRenderer.WriteAnnotations(ctx, file.Annotations);

        if (file.ClassName is not null || file.ExtendsType is not null)
        {
            ctx.Warnings.Add($"File-as-class (ClassName/Extends) is not supported in {ctx.LanguageName}; skipped.");
        }

        List<CodeNode> children = file.Children.Where(c => c is not CodeImport).ToList();

        bool wroteAnything = false;
        foreach (CodeImport import in file.Children.OfType<CodeImport>())
        {
            NodeFeatures.Render(ctx, import, NodeContext.FileLevel);
            wroteAnything = true;
        }

        foreach (CodeNode node in children)
        {
            if (wroteAnything)
            {
                ctx.Writer.BlankLine();
            }

            if (NodeFeatures.Render(ctx, node, NodeContext.FileLevel, file.ClassName))
            {
                wroteAnything = true;
            }
        }
    }

    private static void RenderNodes(RenderContext ctx, List<CodeNode> nodes, string? className)
    {
        CodeNode? previous = null;

        foreach (CodeNode node in nodes)
        {
            bool related = previous is not null
                && ctx.Syntax.BlockStyle == BlockStyle.Indentation
                && IsRelatedDataMember(node, previous);

            if (previous is not null && !related)
            {
                ctx.Writer.BlankLine();
            }

            if (!NodeFeatures.Render(ctx, node, NodeContext.FileLevel, className))
            {
                continue;
            }

            previous = node;
        }
    }

    private static bool IsRelatedDataMember(CodeNode node, CodeNode previous)
    {
        if (previous is CodeComment)
        {
            return true;
        }

        bool isDataMember = node is CodeField;
        bool previousIsDataMember = previous is CodeField;

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
