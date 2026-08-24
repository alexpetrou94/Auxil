namespace Auxil.Tests.CodeGeneration.TypeScriptTests;

public class TypeScriptNestingTest
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
	public void NestedClassIsSkippedWithWarning()
	{
		CodeBuilder<TypeScript> ts = new();
		ts.File("test.ts").Class("Outer").EndClass().EndFile();
		((CodeClass)ts.Model!.Children[0]).Members.Add(new CodeClass { Name = "Inner" });

		string result = ts.Render().Trim();
		result.ShouldBe("class Outer {\n}");
		ts.Warnings.ShouldContain("Nested class 'Inner' is not supported in TypeScript; skipped.");
	}

	[Fact]
	public void EnumInsideClassIsSkippedWithWarning()
	{
		CodeBuilder<TypeScript> ts = new();
		ts.File("test.ts").Class("C").Enum("E").Member("A").EndEnum().EndClass().EndFile();

		string result = ts.Render().Trim();
		result.ShouldBe("class C {\n}");
		ts.Warnings.ShouldContain("Enum 'E' inside a class is not supported in TypeScript; skipped.");
	}

	[Fact]
	public void InterfaceInsideClassIsSkippedWithWarning()
	{
		CodeBuilder<TypeScript> ts = new();
		ts.File("test.ts").Class("C").Interface("I").EndInterface().EndClass().EndFile();

		string result = ts.Render().Trim();
		result.ShouldBe("class C {\n}");
		ts.Warnings.ShouldContain("Interface 'I' inside a class is not supported in TypeScript; skipped.");
	}

	[Fact]
	public void CommentInsideClass()
	{
		string result = Render(ts => ts
			.Class("C")
			.Comment("A note")
			.Field("x", CodeTypes.Int).EndField()
			.EndClass());

		const string expected = """
			class C {
			    // A note
			    x: number;
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void InterfaceExtendsMultiple()
	{
		string result = Render(ts => ts
			.Interface("I")
			.Extends("A", "B")
			.Method("m").Returns(CodeTypes.Void).EndMethod()
			.EndInterface());

		const string expected = """
			interface I extends A, B {
			    m(): void;
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void InterfaceWithJSDoc()
	{
		string result = Render(ts => ts
			.Interface("I")
			.Doc("A repository interface.")
			.Method("find").Returns("T").EndMethod()
			.EndInterface());

		const string expected = """
			/**
			 * A repository interface.
			 */
			interface I {
			    find(): T;
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void EmptyMethodBody()
	{
		string result = Render(ts => ts
			.Class("C")
			.Method("noop").EndMethod()
			.EndClass());

		const string expected = """
			class C {
			    noop() {
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void MethodWithoutReturnType()
	{
		string result = Render(ts => ts
			.Class("C")
			.Method("go").Body(b => b.Line("this.running = true;")).EndMethod()
			.EndClass());

		result.ShouldContain("go() {");
		result.ShouldContain("this.running = true;");
	}

	[Fact]
	public void ParameterWithDefaultValue()
	{
		string result = Render(ts => ts
			.Class("C")
			.Method("greet")
			.Parameter("name", CodeTypes.String, "\"world\"")
			.Body("console.log(name);")
			.EndMethod()
			.EndClass());

		result.ShouldContain("greet(name: string = \"world\") {");
	}

	[Fact]
	public void AsyncTopLevelFunction()
	{
		string result = Render(ts => ts
			.Function("fetchData").Async()
			.Returns("Promise<string>")
			.Body("return Promise.resolve(\"ok\");")
			.EndFunction());

		const string expected = """
			async function fetchData(): Promise<string> {
			    return Promise.resolve("ok");
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void PrivateReadonlyField()
	{
		string result = Render(ts => ts
			.Class("C")
			.Field("name", CodeTypes.String).Private().Readonly().EndField()
			.EndClass());

		result.ShouldContain("private readonly name: string;");
	}

	[Fact]
	public void InterfacePropertyWithDefaultValueIsSkippedWithWarning()
	{
		CodeBuilder<TypeScript> ts = new();
		ts.File("test.ts")
			.Interface("I")
			.Field("limit", CodeTypes.Int, "10").EndField()
			.EndInterface()
			.EndFile();

		string result = ts.Render().Trim();
		result.ShouldBe("interface I {\n    limit: number;\n}");
		ts.Warnings.ShouldContain("Default value on interface property 'limit' is not supported in TypeScript; skipped.");
	}

	[Fact]
	public void EndAliasReturnsToParent()
	{
		string result = Render(ts => ts
			.Enum("E")
			.Member("A")
			.End()
			.Class("C").EndClass());

		const string expected = """
			enum E {
			    A
			};

			class C {
			}
			""";

		result.ShouldBe(expected);
	}
}
