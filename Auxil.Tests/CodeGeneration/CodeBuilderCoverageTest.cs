namespace Auxil.Tests.CodeGeneration;

public class CodeBuilderCoverageTest
{
	private static string Render<TLanguage>(Action<FileScope<TLanguage>> configure)
		where TLanguage : struct, ILanguage
	{
		CodeBuilder<TLanguage> builder = new();
		FileScope<TLanguage> file = builder.File("test.gd");
		configure(file);
		file.EndFile();
		return builder.Render().Trim();
	}

	[Fact]
	public void InsertBeforeNotFoundThrows()
	{
		CodeBuilder<GDScript> code = new();
		code.File("t.gd").Method("go").Body(b => b.Line("run()")).EndMethod().EndFile();

		Should.Throw<CodeBuilderException>(() => code.InsertBefore("nope", b => b.Line("x()")))
			.Message.ShouldBe("No statement with id 'nope' found.");
	}

	[Fact]
	public void RemoveNotFoundThrows()
	{
		CodeBuilder<GDScript> code = new();
		code.File("t.gd").Method("go").Body(b => b.Line("run()")).EndMethod().EndFile();

		Should.Throw<CodeBuilderException>(() => code.Remove("nope"))
			.Message.ShouldBe("No statement with id 'nope' found.");
	}

	[Fact]
	public void EditingRecursesIntoNestedBlocks()
	{
		CodeBuilder<TypeScript> code = new();
		code.File("t.ts")
			.Class("C")
			.Method("go")
			.Body(b => b
				.ForEach("x", "xs", f => f.Line("use(x)", id: "inForEach"))
				.While("running", w => w.Line("tick()", id: "inWhile"))
				.Match("state", m => m.Arm("\"a\"", arm => arm.Line("a()", id: "inArm")))
				.Try(t => t.Line("risky()", id: "inTry"))
				.Catch(c => c.Line("recover()"))
				.If("ok", i => i.Line("fine()", id: "inThen"))
				.ElseIf("meh", e => e.Line("meh()", id: "inElseIf")))
			.EndMethod()
			.EndClass()
			.EndFile();

		code.ReplaceStatement("inForEach", b => b.Line("replaced1()"));
		code.ReplaceStatement("inWhile", b => b.Line("replaced2()"));
		code.ReplaceStatement("inArm", b => b.Line("replaced3()"));
		code.ReplaceStatement("inTry", b => b.Line("replaced4()"));
		code.ReplaceStatement("inElseIf", b => b.Line("replaced5()"));

		string result = code.Render().Trim();
		result.ShouldContain("replaced1()");
		result.ShouldContain("replaced2()");
		result.ShouldContain("replaced3()");
		result.ShouldContain("replaced4()");
		result.ShouldContain("replaced5()");
		result.ShouldNotContain("use(x)");
		result.ShouldNotContain("tick()");
		result.ShouldNotContain("a()");
		result.ShouldNotContain("risky()");
		result.ShouldNotContain("meh()");
	}

	[Fact]
	public void EditingInsideNamespace()
	{
		CodeBuilder<CSharp> code = new();
		code.File("t.cs")
			.Namespace("N")
			.Class("C")
			.Method("go")
			.Body(b => b.Line("x()", id: "s"))
			.EndMethod()
			.EndClass()
			.EndNamespace()
			.EndFile();

		code.ReplaceStatement("s", b => b.Line("y()"));

		code.Render().Trim().ShouldContain("y()");
		code.Render().Trim().ShouldNotContain("x()");
	}

	[Fact]
	public void FileScopeEndAliasReturnsBuilder()
	{
		CodeBuilder<GDScript> code = new();
		FileScope<GDScript> file = code.File("t.gd");
		file.Method("go").Body("pass").EndMethod();

		CodeBuilder<GDScript> result = file.End();

		result.ShouldBeSameAs(code);
		result.Render().Trim().ShouldBe("func go():\n    pass");
	}

	[Fact]
	public void FileLevelCommentRenders()
	{
		string result = Render<GDScript>(gd => gd
			.Comment("A note about this file")
			.Method("go").Body("pass").EndMethod());

		const string expected = """
			# A note about this file
			func go():
			    pass
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void FileLevelPropertyWithStringType()
	{
		string result = Render<GDScript>(gd => gd
			.Property("Name", "String").EndProperty());

		result.ShouldBe("var Name: String");
	}

	[Fact]
	public void InterfaceCommentRendersInTypeScript()
	{
		string result = Render<TypeScript>(ts => ts
			.Interface("I")
			.Comment("An interface note")
			.Method("m").Returns(CodeTypes.Void).EndMethod()
			.EndInterface());

		const string expected = """
			interface I {
			    // An interface note
			    m(): void;
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void InterfacePropertyWithStringType()
	{
		string result = Render<TypeScript>(ts => ts
			.Interface("I")
			.Property("Count", "number").GetOnly().EndProperty()
			.EndInterface());

		const string expected = """
			interface I {
			    Count: number;
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void SignalInsideClassRendersInGDScript()
	{
		string result = Render<GDScript>(gd => gd
			.Class("Player")
			.Signal("hit").Parameter("damage", CodeTypes.Int).EndSignal()
			.EndClass());

		const string expected = """
			class Player:
			    signal hit(damage: int)
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void NestedNamespacesRender()
	{
		string result = Render<CSharp>(cs => cs
			.Namespace("Outer")
			.Namespace("Inner")
			.Class("C").EndClass()
			.EndNamespace()
			.EndNamespace());

		const string expected = """
			namespace Outer;

			namespace Inner;

			class C
			{
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void NamespaceWithMixedChildren()
	{
		string result = Render<CSharp>(cs => cs
			.Namespace("UPhoricStudio.Game")
			.Comment("Types")
			.Interface("IFoo").EndInterface()
			.Enum("State").Member("Idle").EndEnum()
			.Class("C").EndClass()
			.EndNamespace());

		const string expected = """
			namespace UPhoricStudio.Game;

			// Types

			interface IFoo
			{
			}

			enum State
			{
			    Idle
			}

			class C
			{
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void ParameterStringOverloadWithDefault()
	{
		string result = Render<CSharp>(cs => cs
			.Class("C")
			.Method("Run")
			.Parameter("count", "int", "5")
			.Body("pass")
			.EndMethod()
			.EndClass());

		result.ShouldContain("void Run(int count = 5)");
	}

	[Fact]
	public void ReturnsStringOverloads()
	{
		string result = Render<TypeScript>(ts => ts
			.Class("C")
			.Method("Get").Returns("User").Body("return null;").EndMethod()
			.EndClass());

		result.ShouldContain("Get(): User {");

		string function = Render<TypeScript>(ts => ts
			.Function("helper").Returns("User").Body("return null;").EndFunction());

		function.ShouldContain("function helper(): User {");
	}

	[Fact]
	public void UncommonModifiersRenderInCSharp()
	{
		string result = Render<CSharp>(cs => cs
			.Class("Service").Internal().Sealed()
			.Method("Start").Public().Virtual().Returns(CodeTypes.Void).Body("pass").EndMethod()
			.Method("Stop").Protected().Override().Returns(CodeTypes.Void).Body("pass").EndMethod()
			.EndClass());

		const string expected = """
			internal sealed class Service
			{
			    public virtual void Start()
			    {
			        pass
			    }

			    protected override void Stop()
			    {
			        pass
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void BodyBuilderLines()
	{
		string result = Render<GDScript>(gd => gd
			.Method("go")
			.Body(b => b.Lines("first();", "second();"))
			.EndMethod());

		const string expected = """
			func go():
			    first();
			    second();
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void LineWithIdSupportsEditing()
	{
		CodeBuilder<GDScript> code = new();
		code.File("t.gd")
			.Method("go")
			.Body(b => b
				.Line("a()", id: "first")
				.Line("b()"))
			.EndMethod()
			.EndFile();

		code.ReplaceStatement("first", b => b.Line("c()"));

		code.Render().Trim().ShouldBe("func go():\n    c()\n    b()");
	}

	[Fact]
	public void ConstStringOverload()
	{
		string result = Render<CSharp>(cs => cs
			.Class("C")
			.Method("Init")
			.Body(b => b.Const("MAX", "int", "100"))
			.EndMethod()
			.EndClass());

		result.ShouldContain("const int MAX = 100;");
	}

	[Fact]
	public void ElseIfWithoutIfThrows()
	{
		CodeBuilder<GDScript> code = new();
		MethodScope<GDScript, FileScope<GDScript>> method = code.File("t.gd").Method("go");

		Should.Throw<CodeBuilderException>(() => method.Body(b => b.ElseIf("x", e => e.Line("y()"))))
			.Message.ShouldBe("ElseIf/Else requires a preceding If statement.");
	}

	[Fact]
	public void ElseTwiceThrows()
	{
		CodeBuilder<GDScript> code = new();
		MethodScope<GDScript, FileScope<GDScript>> method = code.File("t.gd").Method("go");

		Should.Throw<CodeBuilderException>(() => method.Body(b => b
			.If("x", i => i.Line("y()"))
			.Else(e => e.Line("z()"))
			.Else(e => e.Line("w()"))))
			.Message.ShouldBe("Else can only be added once per If statement.");
	}

	[Fact]
	public void CatchWithoutTryThrows()
	{
		CodeBuilder<CSharp> code = new();
		MethodScope<CSharp, FileScope<CSharp>> method = code.File("t.cs").Method("go");

		Should.Throw<CodeBuilderException>(() => method.Body(b => b.Catch("e", c => c.Line("x()"))))
			.Message.ShouldBe("Catch requires a preceding Try statement.");
	}

	[Fact]
	public void FinallyWithoutTryThrows()
	{
		CodeBuilder<CSharp> code = new();
		MethodScope<CSharp, FileScope<CSharp>> method = code.File("t.cs").Method("go");

		Should.Throw<CodeBuilderException>(() => method.Body(b => b.Finally(f => f.Line("x()"))))
			.Message.ShouldBe("Finally requires a preceding Try statement.");
	}

	[Fact]
	public void FinallyTwiceThrows()
	{
		CodeBuilder<CSharp> code = new();
		MethodScope<CSharp, FileScope<CSharp>> method = code.File("t.cs").Method("go");

		Should.Throw<CodeBuilderException>(() => method.Body(b => b
			.Try(t => t.Line("x()"))
			.Finally(f => f.Line("y()"))
			.Finally(f => f.Line("z()"))))
			.Message.ShouldBe("Finally can only be added once per Try statement.");
	}

	[Fact]
	public void CodeTypeFactoryAndToString()
	{
		CodeType type = CodeType.Of("custom.Type");

		type.Name.ShouldBe("custom.Type");
		type.ToString().ShouldBe("custom.Type");
		CodeType.Of("x").ShouldBe(CodeType.Of("x"));
		CodeType.Of("x").ShouldNotBe(CodeType.Of("y"));
	}

	[Fact]
	public void CodeTypesArrayDictionaryNull()
	{
		string result = Render<GDScript>(gd => gd
			.Field("items", CodeTypes.Array).EndField()
			.Field("lookup", CodeTypes.Dictionary).EndField()
			.Field("nothing", CodeTypes.Null).EndField());

		const string expected = """
			var items: Array
			var lookup: Dictionary
			var nothing: null
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void FileLevelDeclarationsWarnInCSharp()
	{
		CodeBuilder<CSharp> cs = new();
		cs.File("t.cs").Method("M").Body("pass").EndMethod().EndFile();
		CodeFunction function = new() { Name = "F" };
		function.Body = new CodeBody();
		function.Body.Statements.Add(new CodeExpressionStatement { Line = "pass" });
		cs.Model!.Children.Add(function);
		cs.Model.Children.Add(new CodeProperty { Name = "P", Type = CodeTypes.Int });

		cs.Render().Trim().ShouldBeEmpty();
		cs.Warnings.ShouldContain("Method 'M' at file level is not supported in C#; skipped. Use Method() inside a class.");
		cs.Warnings.ShouldContain("Top-level function 'F' is not supported in C#; skipped. Use Method() instead.");
		cs.Warnings.ShouldContain("Property 'P' at file level is not supported in C#; skipped.");
	}

	[Fact]
	public void MethodGenericWithConstraintAndDefault()
	{
		string result = Render<TypeScript>(ts => ts
			.Class("C")
			.Method("Get").Generic("T", "Base", "unknown").Returns("T").Body("return null;").EndMethod()
			.EndClass());

		result.ShouldContain("Get<T extends Base = unknown>(): T {");
	}

	[Fact]
	public void FunctionGeneric()
	{
		string result = Render<TypeScript>(ts => ts
			.Function("helper").Generic("T").Returns("T").Body("return null;").EndFunction());

		result.ShouldContain("function helper<T>(): T {");
	}

	[Fact]
	public void FileLevelCommentRendersInTypeScript()
	{
		string result = Render<TypeScript>(ts => ts
			.Comment("A note")
			.Class("C").EndClass());

		const string expected = """
			// A note

			class C {
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void FileLevelPropertyRendersAsTopLevelField()
	{
		string result = Render<TypeScript>(ts => ts
			.Property("name", CodeTypes.String).EndProperty());

		result.ShouldBe("let name: string;");
	}

	[Fact]
	public void FileLevelMethodConstructorSignalWarnInTypeScript()
	{
		CodeBuilder<TypeScript> ts = new();
		ts.File("t.ts").Method("m").Body("pass").EndMethod().EndFile();
		ts.Model!.Children.Add(new CodeConstructor());
		ts.Model.Children.Add(new CodeSignal { Name = "s" });

		ts.Render().Trim().ShouldBeEmpty();
		ts.Warnings.ShouldContain("Method 'm' at file level is not supported in TypeScript; skipped. Use Function() instead.");
		ts.Warnings.ShouldContain("Constructor at file level is not supported in TypeScript; skipped.");
		ts.Warnings.ShouldContain("Signal 's' is not supported in TypeScript; skipped.");
	}

	[Fact]
	public void BodyBlankLineAndCommentInTypeScript()
	{
		string result = Render<TypeScript>(ts => ts
			.Class("C")
			.Method("go")
			.Body(b => b
				.Line("a()")
				.BlankLine()
				.Comment("note")
				.Line("b()"))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C {
			    go() {
			        a()

			        // note
			        b()
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void GenericConstraintRendersInTypeScript()
	{
		string result = Render<TypeScript>(ts => ts
			.Class("C").Generic("T", "Base").EndClass());

		result.ShouldContain("class C<T extends Base> {");
	}

	[Fact]
	public void UnsupportedModifierWarnsInTypeScript()
	{
		CodeBuilder<TypeScript> ts = new();
		ts.File("t.ts").Class("C").EndClass().EndFile();
		CodeMethod method = new() { Name = "m", Modifiers = CodeModifier.Virtual };
		method.Body = new CodeBody();
		method.Body.Statements.Add(new CodeExpressionStatement { Line = "pass" });
		((CodeClass)ts.Model!.Children[0]).Members.Add(method);

		ts.Render().Trim().ShouldBe("class C {\n    m() {\n        pass\n    }\n}");
		ts.Warnings.ShouldContain("Virtual modifier is not supported in TypeScript; skipped.");
	}

	[Fact]
	public void FileLevelConstructorAndSignalWarnInCSharp()
	{
		CodeBuilder<CSharp> cs = new();
		cs.File("t.cs").EndFile();
		cs.Model!.Children.Add(new CodeConstructor());
		cs.Model.Children.Add(new CodeSignal { Name = "s" });

		cs.Render().Trim().ShouldBeEmpty();
		cs.Warnings.ShouldContain("Constructor at file level is not supported in C#; skipped.");
		cs.Warnings.ShouldContain("Signal 's' is not supported in C#; skipped.");
	}

	[Fact]
	public void ClassCommentAndNestedInterfaceInCSharp()
	{
		string result = Render<CSharp>(cs => cs
			.Class("C")
			.Comment("note")
			.Interface("I").EndInterface()
			.Method("m").Body("pass").EndMethod()
			.EndClass());

		const string expected = """
			class C
			{
			    // note
			    interface I
			    {
			    }

			    void m()
			    {
			        pass
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void InterfaceCommentAndFieldInCSharp()
	{
		string result = Render<CSharp>(cs => cs
			.Interface("I")
			.Comment("note")
			.Field("count", CodeTypes.Int).EndField()
			.Property("Name", CodeTypes.String).EndProperty()
			.EndInterface());

		const string expected = """
			interface I
			{
			    // note
			    int count { get; set; }
			    string Name { get; set; }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void InterfaceAndMethodWhereClauses()
	{
		string result = Render<CSharp>(cs => cs
			.Interface("IRepo")
			.Generic("T", "class")
			.Method("Find").Generic("U", "IComparable").Returns("T").EndMethod()
			.EndInterface());

		const string expected = """
			interface IRepo<T> where T : class
			{
			    T Find<U>() where U : IComparable;
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void MethodWithoutBodyInCSharp()
	{
		string result = Render<CSharp>(cs => cs
			.Class("C")
			.Method("noop").EndMethod()
			.EndClass());

		const string expected = """
			class C
			{
			    void noop()
			    {
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void BodyBlankLineAndCommentInCSharp()
	{
		string result = Render<CSharp>(cs => cs
			.Class("C")
			.Method("go")
			.Body(b => b
				.Line("a();")
				.BlankLine()
				.Comment("note")
				.Line("b();"))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C
			{
			    void go()
			    {
			        a();

			        // note
			        b();
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void CodeTypesObject()
	{
		string result = Render<GDScript>(gd => gd
			.Field("obj", CodeTypes.Object).EndField());

		result.ShouldBe("var obj: Object");
	}

	[Fact]
	public void EditingFindsStatementsInConstructor()
	{
		CodeBuilder<CSharp> code = new();
		code.File("t.cs")
			.Class("C")
			.Constructor().Body(b => b.Line("setup()", id: "ctor")).EndConstructor()
			.EndClass()
			.EndFile();

		code.ReplaceStatement("ctor", b => b.Line("prepare()"));

		code.Render().Trim().ShouldContain("prepare()");
		code.Render().Trim().ShouldNotContain("setup()");
	}

	[Fact]
	public void EditingFindsStatementsInInterfaceMethod()
	{
		CodeBuilder<TypeScript> code = new();
		code.File("t.ts")
			.Interface("I")
			.Method("go").Body(b => b.Line("x()", id: "inIface")).EndMethod()
			.EndInterface()
			.EndFile();

		code.ReplaceStatement("inIface", b => b.Line("y()"));

		code.Render().Trim().ShouldBe("interface I {\n    go();\n}");
	}

	[Fact]
	public void EditingFindsStatementsInFunction()
	{
		CodeBuilder<TypeScript> code = new();
		code.File("t.ts")
			.Function("helper").Body(b => b.Line("x()", id: "inFunc")).EndFunction()
			.EndFile();

		code.ReplaceStatement("inFunc", b => b.Line("y()"));

		code.Render().Trim().ShouldBe("function helper() {\n    y()\n}");
	}

	[Fact]
	public void NotFoundWalksEveryNodeKind()
	{
		CodeBuilder<CSharp> code = new();
		code.File("t.cs")
			.Field("version", CodeTypes.Int).EndField()
			.Constructor().Body(b => b.Line("c()", id: "c")).EndConstructor()
			.Namespace("N")
			.Class("C")
			.Field("field", CodeTypes.Int).EndField()
			.Method("go")
			.Body(b => b
				.If("a", i => i.Line("x()", id: "then"))
				.ElseIf("b", e => e.Line("y()", id: "elif"))
				.Match("state", m => m
					.Arm("\"a\"", arm => arm.Line("z()", id: "arm")))
				.Try(t => t.Line("r()", id: "tryBody"))
				.Catch("err", ca => ca.Line("h()", id: "catchBody")))
			.EndMethod()
			.EndClass()
			.Interface("I")
			.Method("m").Body(b => b.Line("i()", id: "ifaceMethod")).EndMethod()
			.EndInterface()
			.EndNamespace()
			.EndFile();
		CodeFunction function = new() { Name = "helper" };
		function.Body = new CodeBody();
		function.Body.Statements.Add(new CodeExpressionStatement { Line = "h()", Id = "h" });
		code.Model!.Children.Insert(1, function);

		Should.Throw<CodeBuilderException>(() => code.ReplaceStatement("nope", b => b.Line("x()")))
			.Message.ShouldBe("No statement with id 'nope' found.");
	}
}
