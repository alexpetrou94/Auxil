namespace Auxil.Tests.CodeGeneration.TypeScriptTests;

public class TypeScriptNullableTest
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
	public void InterfacePropertyIsNullable()
	{
		string result = Render(ts => ts
			.Interface("I")
			.Property("name", CodeTypes.String).Nullable().EndProperty()
			.EndInterface());

		result.ShouldContain("name?: string;");
	}

	[Fact]
	public void InterfaceFieldIsNullable()
	{
		string result = Render(ts => ts
			.Interface("I")
			.Field("name", CodeTypes.String).Nullable().EndField()
			.EndInterface());

		result.ShouldContain("name?: string;");
	}

	[Fact]
	public void ClassFieldIsNullable()
	{
		string result = Render(ts => ts
			.Class("C")
			.Field("name", CodeTypes.String).Nullable().EndField()
			.EndClass());

		result.ShouldContain("name?: string;");
	}

	[Fact]
	public void ClassPropertyIsNullable()
	{
		string result = Render(ts => ts
			.Class("C")
			.Property("name", CodeTypes.String).Nullable().EndProperty()
			.EndClass());

		result.ShouldContain("name?: string;");
	}

	[Fact]
	public void TopLevelPropertyIsNullable()
	{
		string result = Render(ts => ts
			.Property("name", CodeTypes.String).Nullable().EndProperty());

		result.ShouldContain("let name?: string;");
	}

	[Fact]
	public void MethodParameterIsNullable()
	{
		string result = Render(ts => ts
			.Class("C")
			.Method("greet")
			.Parameter("name", CodeTypes.String).Nullable()
			.Body("console.log(name);")
			.EndMethod()
			.EndClass());

		result.ShouldContain("greet(name?: string) {");
	}

	[Fact]
	public void MethodParameterIsNullableWithDefault()
	{
		string result = Render(ts => ts
			.Class("C")
			.Method("greet")
			.Parameter("name", CodeTypes.String, "\"world\"").Nullable()
			.Body("console.log(name);")
			.EndMethod()
			.EndClass());

		result.ShouldContain("greet(name?: string = \"world\") {");
	}

	[Fact]
	public void InterfaceMethodParameterIsNullable()
	{
		string result = Render(ts => ts
			.Interface("I")
			.Method("greet")
			.Parameter("name", CodeTypes.String).Nullable()
			.EndMethod()
			.EndInterface());

		result.ShouldContain("greet(name?: string);");
	}

	[Fact]
	public void InterfaceMethodDefaultBecomesOptionalWithoutDefaultValue()
	{
		string result = Render(ts => ts
			.Interface("I")
			.Method("greet")
			.Parameter("name", CodeTypes.String, "\"world\"")
			.EndMethod()
			.EndInterface());

		result.ShouldContain("greet(name?: string);");
	}

	[Fact]
	public void InterfaceFieldNullableWithDefaultKeepsWarning()
	{
		CodeBuilder<TypeScript> ts = new();
		ts.File("test.ts")
			.Interface("I")
			.Field("limit", CodeTypes.Int, "10").Nullable().EndField()
			.EndInterface()
			.EndFile();

		string result = ts.Render().Trim();
		result.ShouldBe("interface I {\n    limit?: number;\n}");
		ts.Warnings.ShouldContain("Default value on interface property 'limit' is not supported in TypeScript; skipped.");
	}
}
