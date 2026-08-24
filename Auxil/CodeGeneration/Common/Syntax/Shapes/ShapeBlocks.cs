namespace Auxil.CodeGeneration.Common.Syntax.Shapes;

/// <summary>
/// Block open/close mechanics shared by the shape defaults and the
/// renderer flow: honors the language's block style (braces vs
/// indentation) and the brace placement option where the language
/// honors it.
/// </summary>
[PublicAPI]
public static class ShapeBlocks
{
    /// <summary>Opens a block for the given header.</summary>
    public static void Open(ShapeContext ctx, string header)
    {
        if (ctx.Syntax.BlockStyle == BlockStyle.Indentation)
        {
            ctx.Writer.OpenBlock(header);
            return;
        }

        if (ctx.Syntax.BraceOnOwnLine && ctx.Writer.Options.BraceStyle == BraceStyle.Allman)
        {
            ctx.Writer.WriteLine(header);
            ctx.Writer.WriteLine("{");
            ctx.Writer.Indent();
            return;
        }

        ctx.Writer.OpenBlock(header);
    }

    /// <summary>Closes the current block.</summary>
    public static void Close(ShapeContext ctx)
    {
        Close(ctx, null);
    }

    /// <summary>
    /// Closes the current block, appending a suffix to the closing token
    /// (e.g. "," for an object-literal member close).
    /// </summary>
    public static void Close(ShapeContext ctx, string? suffix)
    {
        if (ctx.Syntax.BlockStyle == BlockStyle.Indentation)
        {
            ctx.Writer.CloseBlock(suffix);
            return;
        }

        if (ctx.Syntax.BraceOnOwnLine && ctx.Writer.Options.BraceStyle == BraceStyle.Allman)
        {
            ctx.Writer.Dedent();
            ctx.Writer.Write("}");
            if (suffix is not null)
            {
                ctx.Writer.Write(suffix);
            }

            ctx.Writer.WriteLine();
            return;
        }

        ctx.Writer.CloseBlock(suffix);
    }
}
