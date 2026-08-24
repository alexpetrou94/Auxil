namespace Auxil.Tests.CodeGeneration.GDScriptTests;

public class GDScriptStatementsTest
{
	private static string Render(Action<FileScope<GDScript>> configure)
	{
		CodeBuilder<GDScript> gd = new();
		FileScope<GDScript> file = gd.File("test.gd");
		configure(file);
		file.EndFile();
		return gd.Render().Trim();
	}

	[Fact]
	public void IfElseIfElse()
	{
		string result = Render(gd => gd
			.Method("move")
			.Body(b => b
				.If("health <= 0", i => i.Return())
				.ElseIf("stamina < 10", e => e.Line("walk()"))
				.Else(e => e.Line("run()")))
			.EndMethod());

		const string expected = """
			func move():
			    if health <= 0:
			        return
			    elif stamina < 10:
			        walk()
			    else:
			        run()
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void ForEach()
	{
		string result = Render(gd => gd
			.Method("sum_all")
			.Body(b => b
				.Line("var sum = 0")
				.ForEach("item", "items", f => f.Line("sum += item"))
				.Return("sum"))
			.EndMethod());

		const string expected = """
			func sum_all():
			    var sum = 0
			    for item in items:
			        sum += item
			    return sum
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void WhileWithBreak()
	{
		string result = Render(gd => gd
			.Method("search")
			.Body(b => b
				.Line("var found = false")
				.While("found == false", w => w
					.Line("found = Scan()")
					.If("found", i => i.Break())))
			.EndMethod());

		const string expected = """
			func search():
			    var found = false
			    while found == false:
			        found = Scan()
			        if found:
			            break
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void MatchWithDefaultArm()
	{
		string result = Render(gd => gd
			.Method("handle")
			.Body(b => b
				.Match("state", m => m
					.Arm("\"idle\"", arm => arm.Line("idle()"))
					.Arm("_", arm => arm.Return())))
			.EndMethod());

		const string expected = """
			func handle():
			    match state:
			        "idle":
			            idle()
			        _:
			            return
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void TryCatchIsSkippedWithWarning()
	{
		CodeBuilder<GDScript> gd = new();
		gd.File("test.gd").Method("risky").EndMethod().EndFile();
		CodeMethod method = (CodeMethod)gd.Model!.Children[0];
		method.Body = new CodeBody();
		CodeTry tryStatement = new();
		tryStatement.Body.Add(new CodeExpressionStatement { Line = "dangerous()" });
		CodeCatch catchBlock = new() { Variable = "err" };
		catchBlock.Body.Add(new CodeExpressionStatement { Line = "handle()" });
		tryStatement.Catches.Add(catchBlock);
		method.Body.Statements.Add(tryStatement);

		string result = gd.Render().Trim();
		result.ShouldBe("func risky():\n    pass");
		gd.Warnings.ShouldContain("Try/catch statements are not supported in GDScript; skipped.");
	}

	[Fact]
	public void EmptyIfGetsPass()
	{
		string result = Render(gd => gd
			.Method("guard")
			.Body(b => b
				.If("enabled", _ => { })
				.Line("run()"))
			.EndMethod());

		const string expected = """
			func guard():
			    if enabled:
			        pass
			    run()
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void NestedStatements()
	{
		string result = Render(gd => gd
			.Method("process")
			.Body(b => b
				.ForEach("item", "items", f => f
					.If("item.dead", i => i.Continue())
					.Line("process(item)")))
			.EndMethod());

		const string expected = """
			func process():
			    for item in items:
			        if item.dead:
			            continue
			        process(item)
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void BodyComments()
	{
		string result = Render(gd => gd
			.Method("update")
			.Body(b => b
				.Comment("Handle input")
				.Line("jump()"))
			.EndMethod());

		const string expected = """
			func update():
			    # Handle input
			    jump()
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void ReturnWithoutValue()
	{
		string result = Render(gd => gd
			.Method("stop")
			.Body(b => b
				.If("!running", i => i.Return())
				.Line("stop_engine()"))
			.EndMethod());

		const string expected = """
			func stop():
			    if !running:
			        return
			    stop_engine()
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void LocalVariables()
	{
		string result = Render(gd => gd
			.Method("compute")
			.Body(b => b
				.Var("total", CodeTypes.Int, "0")
				.Const("LIMIT", CodeTypes.Int, "100")
				.Var("name", CodeTypes.String)
				.Return("total"))
			.EndMethod());

		const string expected = """
			func compute():
			    var total: int = 0
			    const LIMIT: int = 100
			    var name: String
			    return total
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void ThrowDoWhileForAreSkippedWithWarnings()
	{
		CodeBuilder<GDScript> gd = new();
		gd.File("test.gd").Method("risky").EndMethod().EndFile();
		CodeMethod method = (CodeMethod)gd.Model!.Children[0];
		method.Body = new CodeBody();
		method.Body.Statements.Add(new CodeThrow { Value = "Error(\"boom\")" });
		method.Body.Statements.Add(new CodeDoWhile { Condition = "!done" });
		method.Body.Statements.Add(new CodeFor { Initializer = "var i = 0", Condition = "i < n", Increment = "i += 1" });

		string result = gd.Render().Trim();
		result.ShouldBe("func risky():\n    pass");
		gd.Warnings.ShouldContain("Throw statements are not supported in GDScript; skipped.");
		gd.Warnings.ShouldContain("Do/while statements are not supported in GDScript; skipped.");
		gd.Warnings.ShouldContain("C-style for statements are not supported in GDScript; skipped. Use ForEach with range() instead.");
	}

	[Fact]
	public void SkippedStatementsMixWithRenderedOnes()
	{
		CodeBuilder<GDScript> gd = new();
		gd.File("test.gd").Method("work").EndMethod().EndFile();
		CodeMethod method = (CodeMethod)gd.Model!.Children[0];
		method.Body = new CodeBody();
		method.Body.Statements.Add(new CodeLocalVariable { Name = "i", Type = CodeTypes.Int, Value = "0" });
		CodeDoWhile doWhile = new() { Condition = "i < 10" };
		doWhile.Body.Add(new CodeExpressionStatement { Line = "i += 1" });
		method.Body.Statements.Add(doWhile);
		method.Body.Statements.Add(new CodeReturn { Value = "i" });

		string result = gd.Render().Trim();

		const string expected = """
			func work():
			    var i: int = 0
			    return i
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void BlockWithOnlySkippedStatementsGetsPass()
	{
		CodeBuilder<GDScript> gd = new();
		gd.File("test.gd").Method("guard").EndMethod().EndFile();
		CodeMethod method = (CodeMethod)gd.Model!.Children[0];
		method.Body = new CodeBody();
		CodeIf ifStatement = new() { Condition = "amount <= 0" };
		ifStatement.Then.Add(new CodeThrow { Value = "Error(\"boom\")" });
		method.Body.Statements.Add(ifStatement);
		method.Body.Statements.Add(new CodeExpressionStatement { Line = "apply(amount)" });

		string result = gd.Render().Trim();

		const string expected = """
			func guard():
			    if amount <= 0:
			        pass
			    apply(amount)
			""";

		result.ShouldBe(expected);
	}
}
