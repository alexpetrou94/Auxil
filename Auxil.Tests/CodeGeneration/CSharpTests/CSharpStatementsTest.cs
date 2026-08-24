namespace Auxil.Tests.CodeGeneration.CSharpTests;

public class CSharpStatementsTest
{
	private static string Render(Action<FileScope<CSharp>> configure)
	{
		CodeBuilder<CSharp> cs = new();
		FileScope<CSharp> file = cs.File("test.cs");
		configure(file);
		file.EndFile();
		return cs.Render().Trim();
	}

	[Fact]
	public void IfElseIfElse()
	{
		string result = Render(cs => cs
			.Class("C")
			.Method("Move")
			.Body(b => b
				.If("health <= 0", i => i.Return())
				.ElseIf("stamina < 10", e => e.Line("Walk();"))
				.Else(e => e.Line("Run();")))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C
			{
			    void Move()
			    {
			        if (health <= 0)
			        {
			            return;
			        }
			        else if (stamina < 10)
			        {
			            Walk();
			        }
			        else
			        {
			            Run();
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void ForEach()
	{
		string result = Render(cs => cs
			.Class("C")
			.Method("SumAll")
			.Returns(CodeTypes.Int)
			.Body(b => b
				.Var("sum", CodeTypes.Int, "0")
				.ForEach("item", "items", f => f.Line("sum += item;"))
				.Return("sum"))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C
			{
			    int SumAll()
			    {
			        int sum = 0;
			        foreach (var item in items)
			        {
			            sum += item;
			        }
			        return sum;
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void WhileAndDoWhile()
	{
		string result = Render(cs => cs
			.Class("C")
			.Method("Loop")
			.Returns(CodeTypes.Int)
			.Body(b => b
				.Var("i", CodeTypes.Int, "0")
				.While("i < 10", w => w.Line("i++;"))
				.DoWhile("i < 20", d => d.Line("i++;"))
				.Return("i"))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C
			{
			    int Loop()
			    {
			        int i = 0;
			        while (i < 10)
			        {
			            i++;
			        }
			        do
			        {
			            i++;
			        }
			        while (i < 20);
			        return i;
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void CSharpStyleFor()
	{
		string result = Render(cs => cs
			.Class("C")
			.Method("ProcessAll")
			.Body(b => b
				.For("var i = 0", "i < items.Count", "i++", f => f.Line("Process(items[i]);")))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C
			{
			    void ProcessAll()
			    {
			        for (var i = 0; i < items.Count; i++)
			        {
			            Process(items[i]);
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void MatchBecomesSwitch()
	{
		string result = Render(cs => cs
			.Class("C")
			.Method("Handle")
			.Body(b => b
				.Match("state", m => m
					.Arm("\"idle\"", arm => arm.Line("Idle();"))
					.Arm("_", arm => arm.Return())))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C
			{
			    void Handle()
			    {
			        switch (state)
			        {
			            case "idle":
			                Idle();
			                break;
			            default:
			                return;
			                break;
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void TryCatchFinally()
	{
		string result = Render(cs => cs
			.Class("C")
			.Method("Risky")
			.Body(b => b
				.Try(t => t.Line("Dangerous();"))
				.Catch("err", c => c.Line("Handle(err);"))
				.Finally(f => f.Line("Cleanup();")))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C
			{
			    void Risky()
			    {
			        try
			        {
			            Dangerous();
			        }
			        catch (Exception err)
			        {
			            Handle(err);
			        }
			        finally
			        {
			            Cleanup();
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void TypedAndBareCatchesRenderValidCSharp()
	{
		string result = Render(cs => cs
			.Class("C")
			.Method("Risky")
			.Body(b => b
				.Try(t => t.Throw("new System.InvalidOperationException()"))
				.Catch("System.InvalidOperationException", "error", c => c.Line("Handle(error);"))
				.Catch(c => c.Line("Recover();")))
			.EndMethod()
			.EndClass());

		result.ShouldContain("catch (System.InvalidOperationException error)");
		result.ShouldContain("catch\n");
	}

	[Fact]
	public void TryWithoutHandlerFailsBeforeRendering()
	{
		CodeBuilder<CSharp> code = new();
		code.File("test.cs")
			.Class("C")
			.Method("Risky")
			.Body(b => b.Try(t => t.Line("Dangerous();")))
			.EndMethod()
			.EndClass()
			.EndFile();

		Should.Throw<CodeBuilderException>(() => code.Render())
			.Message.ShouldBe("A try statement requires at least one catch or finally block.");
	}

	[Fact]
	public void LocalsAndThrow()
	{
		string result = Render(cs => cs
			.Class("C")
			.Method("Guard")
			.Body(b => b
				.Const("MAX", CodeTypes.Int, "100")
				.Var("items", "List<int>", "new List<int>()")
				.If("items.Count > MAX", i => i.Throw("new InvalidOperationException(\"too many\")"))
				.Return())
			.EndMethod()
			.EndClass());

		const string expected = """
			class C
			{
			    void Guard()
			    {
			        const int MAX = 100;
			        List<int> items = new List<int>();
			        if (items.Count > MAX)
			        {
			            throw new InvalidOperationException("too many");
			        }
			        return;
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void UntypedLocalUsesVar()
	{
		string result = Render(cs => cs
			.Class("C")
			.Method("Init")
			.Returns("Player")
			.Body(b => b
				.Var("player", (CodeType?)null, "new Player()")
				.Return("player"))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C
			{
			    Player Init()
			    {
			        var player = new Player();
			        return player;
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void LocalWithoutTypeOrValueIsSkippedWithWarning()
	{
		CodeBuilder<CSharp> cs = new();
		cs.File("test.cs")
			.Class("C")
			.Method("Init")
			.Body(b => b
				.Var("mystery")
				.Return())
			.EndMethod()
			.EndClass()
			.EndFile();

		string result = cs.Render().Trim();
		result.ShouldBe("class C\n{\n    void Init()\n    {\n        return;\n    }\n}");
		cs.Warnings.ShouldContain("Local variable 'mystery' has no type or value; skipped.");
	}

	[Fact]
	public void KAndRBraceStyle()
	{
		CodeBuilderOptions options = new() { BraceStyle = BraceStyle.KAndR };
		CodeBuilder<CSharp> cs = new(options);
		cs.File("test.cs")
			.Class("C")
			.Method("Go")
			.Body(b => b
				.If("a", i => i.Line("X();"))
				.Else(e => e.Line("Y();"))
				.DoWhile("!done", d => d.Line("Step();")))
			.EndMethod()
			.EndClass()
			.EndFile();

		const string expected = """
			class C {
			    void Go() {
			        if (a) {
			            X();
			        } else {
			            Y();
			        }
			        do {
			            Step();
			        } while (!done);
			    }
			}
			""";

		cs.Render().Trim().ShouldBe(expected);
	}

	[Fact]
	public void LocalFunction()
	{
		string result = Render(cs => cs
			.Class("Calc")
			.Method("Run")
			.Body(b => b
				.Function("Helper", f => f
					.Static()
					.Parameter("x", "int")
					.Returns(CodeTypes.Int)
					.Body(h => h.Return("x * 2")))
				.Return("Helper(21)"))
			.EndMethod()
			.EndClass());

		const string expected = """
			class Calc
			{
			    void Run()
			    {
			        static int Helper(int x)
			        {
			            return x * 2;
			        }
			        return Helper(21);
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void LocalFunctionWithDocComment()
	{
		string result = Render(cs => cs
			.Class("Calc")
			.Method("Run")
			.Body(b => b
				.Function("Helper", f => f
					.Doc("Doubles a value.\nSecond line.")
					.Parameter("x", "int")
					.Returns(CodeTypes.Int)
					.Body(h => h.Return("x * 2"))))
			.EndMethod()
			.EndClass());

		const string expected = """
			class Calc
			{
			    void Run()
			    {
			        /// Doubles a value.
			        /// Second line.
			        int Helper(int x)
			        {
			            return x * 2;
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}
}
