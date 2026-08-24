namespace Auxil.Tests.CodeGeneration;

public class CodeWriterTest
{
	private static CodeWriter CreateWriter<TLanguage>(CodeBuilderOptions? options = null)
		where TLanguage : struct, ILanguage
	{
		return new CodeWriter(TLanguage.Syntax, options ?? new CodeBuilderOptions());
	}

	[Fact]
	public void ResetIndentClearsLevel()
	{
		CodeWriter writer = CreateWriter<GDScript>();
		writer.Indent();
		writer.Indent();
		writer.WriteLine("a");
		writer.ResetIndent();
		writer.WriteLine("b");

		writer.ToString().ShouldBe("        a\nb\n");
	}

	[Fact]
	public void WriteLinesWritesEachLineIndented()
	{
		CodeWriter writer = CreateWriter<GDScript>();
		writer.Indent();
		writer.WriteLines(["a", "b"]);

		writer.ToString().ShouldBe("    a\n    b\n");
	}

	[Fact]
	public void GetIndentReflectsLevelAndOption()
	{
		CodeWriter writer = CreateWriter<GDScript>(new CodeBuilderOptions { Indent = "  " });
		writer.Indent();
		writer.Indent();

		writer.GetIndent().ShouldBe("    ");
	}

	[Fact]
	public void CloseBlockWithSuffixOnIndentationLanguage()
	{
		CodeWriter writer = CreateWriter<GDScript>();
		writer.OpenBlock("func f");
		writer.WriteLine("pass");
		writer.CloseBlock(";");

		writer.ToString().ShouldBe("func f:\n    pass\n;\n");
	}

	[Fact]
	public void SemicolonsTrueOverridesIndentationLanguage()
	{
		CodeWriter writer = CreateWriter<GDScript>(new CodeBuilderOptions { Semicolons = true });
		writer.WriteStatement("return x");

		writer.ToString().ShouldBe("return x;\n");
	}

	[Fact]
	public void WriteDocCommentJavadocStyle()
	{
		CodeWriter writer = CreateWriter<TypeScript>();
		writer.WriteDocComment("Line one.\nLine two.");

		const string expected = """
			/**
			 * Line one.
			 * Line two.
			 */
			""";
		writer.ToString().ShouldBe(expected + "\n");
	}

	[Fact]
	public void WriteDocCommentLinePrefixStyle()
	{
		CodeWriter writer = CreateWriter<CSharp>();
		writer.WriteDocComment("Line one.\nLine two.");

		writer.ToString().ShouldBe("/// Line one.\n/// Line two.\n");
	}
}
