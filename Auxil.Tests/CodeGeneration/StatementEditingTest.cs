namespace Auxil.Tests.CodeGeneration;

public class StatementEditingTest
{
	[Fact]
	public void ReplaceStatementChangesKind()
	{
		CodeBuilder<GDScript> code = new();
		code.File("test.gd")
			.Method("go")
			.Body(b => b
				.Line("setup()", id: "setup")
				.Line("run()", id: "run"))
			.EndMethod()
			.EndFile();

		code.ReplaceStatement("setup", b => b.If("ready", i => i.Line("setup()")));

		const string expected = """
			func go():
			    if ready:
			        setup()
			    run()
			""";

		code.Render().Trim().ShouldBe(expected);
	}

	[Fact]
	public void InsertBefore()
	{
		CodeBuilder<GDScript> code = new();
		code.File("test.gd")
			.Method("go")
			.Body(b => b
				.Line("setup()", id: "setup")
				.Line("run()", id: "run"))
			.EndMethod()
			.EndFile();

		code.InsertBefore("run", b => b.Line("prepare()"));

		const string expected = """
			func go():
			    setup()
			    prepare()
			    run()
			""";

		code.Render().Trim().ShouldBe(expected);
	}

	[Fact]
	public void Remove()
	{
		CodeBuilder<GDScript> code = new();
		code.File("test.gd")
			.Method("go")
			.Body(b => b
				.Line("setup()", id: "setup")
				.Line("run()", id: "run"))
			.EndMethod()
			.EndFile();

		code.Remove("setup");

		const string expected = """
			func go():
			    run()
			""";

		code.Render().Trim().ShouldBe(expected);
	}

	[Fact]
	public void ReplaceInsideNestedBlock()
	{
		CodeBuilder<GDScript> code = new();
		code.File("test.gd")
			.Method("go")
			.Body(b => b
				.If("enabled", i => i.Line("a()", id: "inner"))
				.Line("b()", id: "outer"))
			.EndMethod()
			.EndFile();

		code.ReplaceStatement("inner", b => b.Line("c()"));

		const string expected = """
			func go():
			    if enabled:
			        c()
			    b()
			""";

		code.Render().Trim().ShouldBe(expected);
	}

	[Fact]
	public void ReplaceInsideRawBlock()
	{
		CodeBuilder<GDScript> code = new();
		code.File("test.gd")
			.Method("go")
			.Body(b => b
				.Block("guard", g => g.Line("a()", id: "inner"))
				.Line("b()", id: "outer"))
			.EndMethod()
			.EndFile();

		code.ReplaceStatement("inner", b => b.Line("c()"));

		const string expected = """
			func go():
			    guard:
			        c()
			    b()
			""";

		code.Render().Trim().ShouldBe(expected);
	}

	[Fact]
	public void ReplaceWithMultipleStatements()
	{
		CodeBuilder<GDScript> code = new();
		code.File("test.gd")
			.Method("go")
			.Body(b => b.Line("single()", id: "s"))
			.EndMethod()
			.EndFile();

		code.ReplaceStatement("s", b => b
			.Line("first()")
			.Line("second()"));

		const string expected = """
			func go():
			    first()
			    second()
			""";

		code.Render().Trim().ShouldBe(expected);
	}

	[Fact]
	public void NotFoundThrows()
	{
		CodeBuilder<GDScript> code = new();
		code.File("test.gd")
			.Method("go")
			.Body(b => b.Line("run()"))
			.EndMethod()
			.EndFile();

		Should.Throw<CodeBuilderException>(() => code.ReplaceStatement("nope", b => b.Line("x()")))
			.Message.ShouldBe("No statement with id 'nope' found.");
	}

	[Fact]
	public void ReplaceInsideDoWhileAndForBodies()
	{
		CodeBuilder<TypeScript> code = new();
		code.File("test.ts")
			.Class("C")
			.Method("go")
			.Body(b => b
				.DoWhile("!done", w => w.Line("step()", id: "inner"))
				.For("let i = 0", "i < n", "i++", f => f.Line("tick()", id: "inner2")))
			.EndMethod()
			.EndClass()
			.EndFile();

		code.ReplaceStatement("inner", b => b.Line("work()"));
		code.Remove("inner2");

		const string expected = """
			class C {
			    go() {
			        do {
			            work()
			        }
			        while (!done);
			        for (let i = 0; i < n; i++) {
			        }
			    }
			}
			""";

		code.Render().Trim().ShouldBe(expected);
	}

	[Fact]
	public void EditingFindsStatementsInPropertyAccessors()
	{
		CodeBuilder<CSharp> code = new();
		code.File("test.cs")
			.Class("C")
			.Property("Value", CodeTypes.Int)
			.Getter(g => g.Line("return _value;", id: "getter"))
			.Setter(s => s.Line("_value = value;", id: "setter"))
			.EndProperty()
			.EndClass()
			.EndFile();

		code.ReplaceStatement("getter", b => b.Line("return ReadValue();"));
		code.InsertBefore("setter", b => b.Line("Validate(value);"));
		code.Remove("setter");

		string result = code.Render().Trim();
		result.ShouldContain("return ReadValue();");
		result.ShouldContain("Validate(value);");
		result.ShouldNotContain("_value = value;");
	}
}
