using System.Text;
using Auxil.CodeGeneration.Common.Syntax;

namespace Auxil.CodeGeneration.Common;

/// <summary>
/// Writes text with indentation tracking. All language-specific tokens
/// (block braces, comment prefixes, statement terminators) come from the
/// LanguageSyntax profile — no tokens are hardcoded here.
/// </summary>
[PublicAPI]
public class CodeWriter
{
    private readonly StringBuilder _builder = new();
    private int _indentLevel;
    private bool _isNewLine = true;

    /// <summary>Creates a writer for the given language syntax and formatting options.</summary>
    /// <param name="syntax">The language profile supplying all tokens.</param>
    /// <param name="options">The formatting options.</param>
    public CodeWriter(LanguageSyntax syntax, CodeBuilderOptions options)
    {
        Syntax = syntax;
        Options = options;
    }

    /// <summary>The language profile supplying all tokens.</summary>
    public LanguageSyntax Syntax { get; }

    /// <summary>The formatting options in effect.</summary>
    public CodeBuilderOptions Options { get; }

    /// <summary>Increases the indentation level.</summary>
    public void Indent()
    {
        _indentLevel++;
    }

    /// <summary>Decreases the indentation level, never below zero.</summary>
    public void Dedent()
    {
        _indentLevel = Math.Max(0, _indentLevel - 1);
    }

    /// <summary>Resets the indentation level to zero.</summary>
    public void ResetIndent()
    {
        _indentLevel = 0;
    }

    /// <summary>Writes text at the current position, applying indentation on a fresh line.</summary>
    /// <param name="text">The text to write.</param>
    public void Write(string text)
    {
        if (_isNewLine)
        {
            _builder.Append(GetIndent());
            _isNewLine = false;
        }

        _builder.Append(text);
    }

    /// <summary>Writes text followed by the configured line ending.</summary>
    /// <param name="text">The text to write; null writes an empty line.</param>
    public void WriteLine(string? text = null)
    {
        if (text is not null)
        {
            Write(text);
        }

        _builder.Append(Options.LineEnding);
        _isNewLine = true;
    }

    /// <summary>Writes each line followed by the configured line ending.</summary>
    /// <param name="lines">The lines to write.</param>
    public void WriteLines(IEnumerable<string> lines)
    {
        foreach (string line in lines)
        {
            WriteLine(line);
        }
    }

    /// <summary>
    /// Writes raw text that may contain embedded line breaks. Every line
    /// is indented at the current level; interior blank lines stay blank;
    /// a trailing line break does not produce a stray line.
    /// </summary>
    /// <param name="text">The text to write.</param>
    public void WriteMultiLine(string text)
    {
        foreach (string line in text.TrimEnd('\r', '\n').ReplaceLineEndings("\n").Split('\n'))
        {
            if (line.Length == 0)
            {
                WriteLine();
            }
            else
            {
                WriteLine(line);
            }
        }
    }

    /// <summary>Writes blank lines.</summary>
    /// <param name="count">The number of blank lines.</param>
    public void BlankLine(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            WriteLine();
        }
    }

    /// <summary>
    /// Opens a block for the given header. Brace languages write
    /// "header {" and indent; indentation languages write "header:"
    /// (or the language's header terminator) and indent.
    /// </summary>
    public void OpenBlock(string header)
    {
        WriteLine(header + (Syntax.BlockStyle == BlockStyle.Braces ? Syntax.BlockOpen : Syntax.HeaderTerminator));
        Indent();
    }

    /// <summary>
    /// Closes the current block. Brace languages write the closing token;
    /// indentation languages simply dedent. Suffix is appended to the
    /// closing token (e.g. ";" after a TypeScript enum).
    /// </summary>
    public void CloseBlock(string? suffix = null)
    {
        Dedent();

        if (Syntax.BlockStyle == BlockStyle.Braces)
        {
            Write(Syntax.BlockClose);

            if (suffix is not null)
            {
                Write(suffix);
            }

            WriteLine();
        }
        else if (suffix is not null)
        {
            WriteLine(suffix);
        }
    }

    /// <summary>
    /// Closes the current block without a newline, so the next OpenBlock
    /// lands on the same line — the K&amp;R "} else {" style.
    /// </summary>
    public void CloseBlockInline(string suffix)
    {
        Dedent();
        Write(Syntax.BlockClose);
        Write(suffix);
    }

    /// <summary>
    /// Writes a line comment. Multi-line text gets the prefix on every line.
    /// </summary>
    public void WriteComment(string text)
    {
        string[] lines = SplitLines(text);
        foreach (string line in lines)
        {
            WriteLine($"{Syntax.LineCommentPrefix} {line}");
        }
    }

    /// <summary>
    /// Writes a documentation comment using the language's doc tokens.
    /// </summary>
    public void WriteDocComment(string text)
    {
        DocCommentTokens doc = Syntax.DocComment;
        string[] lines = SplitLines(text);

        if (doc.Open is not null)
        {
            WriteLine(doc.Open);
        }

        foreach (string line in lines)
        {
            Write(doc.LinePrefix);
            WriteLine(line);
        }

        if (doc.Close is not null)
        {
            WriteLine(doc.Close);
        }
    }

    /// <summary>
    /// Writes a statement line, appending the statement terminator
    /// (semicolon or empty) from the language profile unless the
    /// options override it.
    /// </summary>
    public void WriteStatement(string text)
    {
        WriteLine(text + GetStatementTerminator());
    }

    /// <summary>The statement terminator honoring the options override.</summary>
    public string GetStatementTerminator()
    {
        return Options.Semicolons switch
        {
            true => ";",
            false => string.Empty,
            null => Syntax.StatementTerminator
        };
    }

    /// <summary>The indentation string for the current level.</summary>
    public string GetIndent()
    {
        return string.Concat(Enumerable.Repeat(Options.Indent, _indentLevel));
    }

    /// <summary>The text written so far.</summary>
    public override string ToString()
    {
        return _builder.ToString();
    }

    private static string[] SplitLines(string text)
    {
        return text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
    }
}
