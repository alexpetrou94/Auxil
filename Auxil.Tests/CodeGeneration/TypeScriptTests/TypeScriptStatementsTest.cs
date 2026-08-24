namespace Auxil.Tests.CodeGeneration.TypeScriptTests;

public class TypeScriptStatementsTest
{
	private static string Render(Action<FileScope<TypeScript>> configure)
	{
		CodeBuilder<TypeScript> ts = new();
		FileScope<TypeScript> file = ts.File("test.ts");
		configure(file);
		file.EndFile();
		return ts.Render().Trim();
	}

	[Fact]
	public void IfElseIfElse()
	{
		string result = Render(ts => ts
			.Class("C")
			.Method("move")
			.Body(b => b
				.If("health <= 0", i => i.Return())
				.ElseIf("stamina < 10", e => e.Line("walk()"))
				.Else(e => e.Line("run()")))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C {
			    move() {
			        if (health <= 0) {
			            return;
			        }
			        else if (stamina < 10) {
			            walk()
			        }
			        else {
			            run()
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void ForEach()
	{
		string result = Render(ts => ts
			.Class("C")
			.Method("sumAll")
			.Body(b => b
				.Line("let sum = 0")
				.ForEach("item", "items", f => f.Line("sum += item"))
				.Return("sum"))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C {
			    sumAll() {
			        let sum = 0
			        for (const item of items) {
			            sum += item
			        }
			        return sum;
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void MatchBecomesSwitchWithDefault()
	{
		string result = Render(ts => ts
			.Class("C")
			.Method("handle")
			.Body(b => b
				.Match("state", m => m
					.Arm("\"idle\"", arm => arm.Line("idle()"))
					.Arm("_", arm => arm.Return())))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C {
			    handle() {
			        switch (state) {
			            case "idle":
			                idle()
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
		string result = Render(ts => ts
			.Class("C")
			.Method("risky")
			.Body(b => b
				.Try(t => t.Line("dangerous()"))
				.Catch("err", c => c.Line("handle(err)"))
				.Finally(f => f.Line("cleanup()")))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C {
			    risky() {
			        try {
			            dangerous()
			        }
			        catch (err) {
			            handle(err)
			        }
			        finally {
			            cleanup()
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void NestedStatementsWithBreakContinue()
	{
		string result = Render(ts => ts
			.Class("C")
			.Method("process")
			.Body(b => b
				.ForEach("item", "items", f => f
					.If("item.dead", i => i.Continue())
					.If("item == target", i => i.Break())
					.Line("process(item)")))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C {
			    process() {
			        for (const item of items) {
			            if (item.dead) {
			                continue;
			            }
			            if (item == target) {
			                break;
			            }
			            process(item)
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void EmptyBlockRendersEmptyBraces()
	{
		string result = Render(ts => ts
			.Class("C")
			.Method("guard")
			.Body(b => b
				.If("enabled", _ => { }))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C {
			    guard() {
			        if (enabled) {
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void WhileWithReturn()
	{
		string result = Render(ts => ts
			.Class("C")
			.Method("find")
			.Body(b => b
				.While("true", w => w
					.Line("const item = next()")
					.If("item == null", i => i.Return("null"))
					.Line("consume(item)")))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C {
			    find() {
			        while (true) {
			            const item = next()
			            if (item == null) {
			                return null;
			            }
			            consume(item)
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void LocalVariables()
	{
		string result = Render(ts => ts
			.Class("C")
			.Method("compute")
			.Body(b => b
				.Var("total", CodeTypes.Int, "0")
				.Const("LIMIT", CodeTypes.Int, "100")
				.Var("name", CodeTypes.String)
				.Return("total"))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C {
			    compute() {
			        let total: number = 0;
			        const LIMIT: number = 100;
			        let name: string;
			        return total;
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void Throw()
	{
		string result = Render(ts => ts
			.Class("C")
			.Method("applyAmount")
			.Body(b => b
				.If("amount < 0", i => i.Throw("new Error(\"negative\")"))
				.Line("apply(amount)"))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C {
			    applyAmount() {
			        if (amount < 0) {
			            throw new Error("negative");
			        }
			        apply(amount)
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void DoWhile()
	{
		string result = Render(ts => ts
			.Class("C")
			.Method("loop")
			.Body(b => b
				.Var("i", CodeTypes.Int, "0")
				.DoWhile("i < 10", w => w.Line("work(i)")))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C {
			    loop() {
			        let i: number = 0;
			        do {
			            work(i)
			        }
			        while (i < 10);
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void CSharpStyleFor()
	{
		string result = Render(ts => ts
			.Class("C")
			.Method("processAll")
			.Body(b => b
				.For("let i = 0", "i < items.length", "i++", f => f.Line("process(items[i])")))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C {
			    processAll() {
			        for (let i = 0; i < items.length; i++) {
			            process(items[i])
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}
}
