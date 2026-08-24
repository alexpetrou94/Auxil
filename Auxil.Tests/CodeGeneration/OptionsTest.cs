namespace Auxil.Tests.CodeGeneration;

public class OptionsTest
{
	[Fact]
	public void CustomIndentIsUsed()
	{
		CodeBuilderOptions options = new() { Indent = "\t" };
		CodeBuilder<TypeScript> ts = new(options);
		ts.File("test.ts")
			.Class("C")
			.Field("name", CodeTypes.String).EndField()
			.EndClass()
			.EndFile();

		ts.Render().ShouldContain("\tname: string;");
	}

	[Fact]
	public void TwoSpaceIndentIsUsed()
	{
		CodeBuilderOptions options = new() { Indent = "  " };
		CodeBuilder<GDScript> gd = new(options);
		gd.File("test.gd")
			.Method("m").Body("pass").EndMethod()
			.EndFile();

		gd.Render().ShouldContain("  pass");
	}

	[Fact]
	public void CustomLineEndingIsUsed()
	{
		CodeBuilderOptions options = new() { LineEnding = "\r\n" };
		CodeBuilder<TypeScript> ts = new(options);
		ts.File("test.ts").Class("C").EndClass().EndFile();

		ts.Render().ShouldContain("class C {\r\n}");
	}

	[Fact]
	public void ToStringRendersModel()
	{
		CodeBuilder<GDScript> gd = new();
		gd.File("test.gd")
			.Method("m").Body("pass").EndMethod()
			.EndFile();

		gd.ToString().Trim().ShouldBe("func m():\n    pass");
	}

	[Fact]
	public void WarningsRepopulateOnRerender()
	{
		CodeBuilder<GDScript> gd = new();
		gd.File("test.gd").EndFile();
		gd.Model!.Children.Add(new CodeInterface { Name = "I" });

		gd.Render();
		gd.Warnings.ShouldContain("Interfaces are not supported in GDScript; skipped.");

		gd.Render();
		gd.Warnings.ShouldContain("Interfaces are not supported in GDScript; skipped.");
	}

	[Fact]
	public void ModelIsExposedAfterBuild()
	{
		CodeBuilder<GDScript> gd = new();
		gd.File("test.gd").ClassName("Player").EndFile();

		CodeFile model = gd.Model!;
		model.ShouldNotBeNull();
		model.FileName.ShouldBe("test.gd");
		model.ClassName.ShouldBe("Player");
	}

	[Fact]
	public void KAndRBraceStyle()
	{
		CodeBuilderOptions options = new() { BraceStyle = BraceStyle.KAndR };
		CodeBuilder<TypeScript> ts = new(options);
		ts.File("test.ts")
			.Class("C")
			.Method("go")
			.Body(b => b
				.If("a", i => i.Line("x()"))
				.ElseIf("b", e => e.Line("y()"))
				.Else(e => e.Line("z()"))
				.Try(t => t.Line("t()"))
				.Catch("err", c => c.Line("h()"))
				.Finally(f => f.Line("clean()"))
				.DoWhile("!done", w => w.Line("step()")))
			.EndMethod()
			.EndClass()
			.EndFile();

		const string expected = """
			class C {
			    go() {
			        if (a) {
			            x()
			        } else if (b) {
			            y()
			        } else {
			            z()
			        }
			        try {
			            t()
			        } catch (err) {
			            h()
			        } finally {
			            clean()
			        }
			        do {
			            step()
			        } while (!done);
			    }
			}
			""";

		ts.Render().Trim().ShouldBe(expected);
	}

	[Fact]
	public void BraceStyleDoesNotAffectGDScript()
	{
		CodeBuilderOptions options = new() { BraceStyle = BraceStyle.KAndR };
		CodeBuilder<GDScript> gd = new(options);
		gd.File("test.gd")
			.Method("go")
			.Body(b => b
				.If("a", i => i.Line("x()"))
				.Else(e => e.Line("y()")))
			.EndMethod()
			.EndFile();

		const string expected = """
			func go():
			    if a:
			        x()
			    else:
			        y()
			""";

		gd.Render().Trim().ShouldBe(expected);
	}
}