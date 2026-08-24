using Auxil.CodeGeneration.Common.Syntax;
using Auxil.CodeGeneration.Common.Syntax.Shapes;

namespace Auxil.CodeGeneration.Common.Rendering;

/// <summary>
/// Shared statement renderer flow: body rendering, the pass placeholder
/// for indentation languages, capability checks, and dispatch to the
/// language's statement shape hooks. The shape hooks (see
/// <see cref="Syntax.Shapes.Shapes"/>) decide how each statement looks.
/// </summary>
internal static class StatementRenderer
{
    public static void RenderBody(RenderContext ctx, CodeBody? body)
    {
        if (body is null)
        {
            if (ctx.Syntax.BlockStyle == BlockStyle.Indentation)
            {
                ctx.Writer.WriteLine(ctx.Syntax.KeywordPass);
            }

            return;
        }

        RenderStatements(ctx, body.Statements);
    }

    public static void RenderStatements(RenderContext ctx, List<CodeStatement> statements)
    {
        if (ctx.Syntax.BlockStyle == BlockStyle.Indentation
            && (statements.Count == 0 || !statements.Any(s => NodeFeatures.CanRender(ctx, s))))
        {
            ctx.Writer.WriteLine(ctx.Syntax.KeywordPass);
        }

        foreach (CodeStatement statement in statements)
        {
            NodeFeatures.Render(ctx, statement);
        }
    }

    private static void RenderNestedFunction(RenderContext ctx, CodeNestedFunction function)
    {
        if (function.DocComment is not null)
        {
            ctx.Writer.WriteDocComment(function.DocComment.Text);
        }

        string header = string.Join(" ", [.. ctx.Syntax.BuildModifierPrefix(function.Modifiers),
            ctx.Shapes.Signature(ctx.Shape, SignatureKind.Function, function.Name, function.Parameters, function.Generics, function.ReturnType, className: string.Empty, interfaceMode: false)]);

        ShapeBlocks.Open(ctx.Shape, header);
        RenderBody(ctx, function.Body);
        ShapeBlocks.Close(ctx.Shape);
    }

    internal static void RenderStatement(RenderContext ctx, CodeStatement statement)
    {
        ShapeContext shape = ctx.Shape;

        switch (statement)
        {
            case CodeExpressionStatement expr:
                ctx.Writer.WriteMultiLine(expr.Line);
                break;
            case CodeBlankLine:
                ctx.Writer.BlankLine();
                break;
            case CodeCommentStatement comment:
                ctx.Writer.WriteComment(comment.Text);
                break;
            case CodeIf ifStatement:
                ctx.Shapes.If(shape, ifStatement);
                break;
            case CodeForEach forEach:
                ctx.Shapes.ForEach(shape, forEach.Variable, forEach.Iterable, forEach.Body);
                break;
            case CodeWhile whileStatement:
                ctx.Shapes.While(shape, whileStatement.Condition, whileStatement.Body);
                break;
            case CodeDoWhile doWhile:
                ctx.Shapes.DoWhile(shape, doWhile.Condition, doWhile.Body);
                break;
            case CodeFor forStatement:
                ctx.Shapes.For(shape, forStatement.Initializer, forStatement.Condition, forStatement.Increment, forStatement.Body);
                break;
            case CodeMatch match:
                ctx.Shapes.Match(shape, match);
                break;
            case CodeRawBlock rawBlock:
                ctx.Shapes.RawBlock(shape, rawBlock.Header, rawBlock.Body, rawBlock.CloseSuffix);
                break;
            case CodeTry tryStatement:
                ctx.Shapes.Try(shape, tryStatement);
                break;
            case CodeLocalVariable variable:
                if (ctx.Shapes.LocalVariable(shape, variable) is { } declaration)
                {
                    ctx.Writer.WriteStatement(declaration);
                }

                break;
            case CodeNestedFunction nestedFunction:
                RenderNestedFunction(ctx, nestedFunction);
                break;
            case CodeObjectLiteral literal:
                ctx.Shapes.ObjectLiteral(shape, literal);
                break;
            case CodeThrow throwStatement:
                ctx.Writer.WriteStatement($"throw {throwStatement.Value}");
                break;
            case CodeReturn ret:
                ctx.Writer.WriteStatement(ret.Value is null ? "return" : $"return {ret.Value}");
                break;
            case CodeBreak:
                ctx.Writer.WriteStatement("break");
                break;
            case CodeContinue:
                ctx.Writer.WriteStatement("continue");
                break;
        }
    }
}
