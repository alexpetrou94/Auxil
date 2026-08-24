
namespace Auxil.Tests.CodeGeneration.TypeScriptTests;

public class TypeScriptTypeAliasTest
{
	private static string Render(Action<FileScope<TypeScript>> configure)
	{
		CodeBuilder<TypeScript> ts = new();
		FileScope<TypeScript> file = ts.File("api.ts");
		configure(file);
		file.EndFile();
		return ts.Render().Trim();
	}

	[Fact]
	public void TypeAliasRendersVerbatim()
	{
		string result = Render(ts => ts
			.TypeAlias("Headers", "Record<string, string>").EndTypeAlias());

		result.ShouldBe("type Headers = Record<string, string>;");
	}

	[Fact]
	public void ExportedTypeAliasWithGenericNameRendersVerbatim()
	{
		string result = Render(ts => ts
			.TypeAlias("ApiResult<T>", "{ data: T; error: null } | { data: null; error: ProblemDetail }").Export().EndTypeAlias());

		result.ShouldBe("export type ApiResult<T> = { data: T; error: null } | { data: null; error: ProblemDetail };");
	}

	[Fact]
	public void TypeAliasOrdersWithOtherDeclarations()
	{
		string result = Render(ts => ts
			.Import("./types", null, "User")
			.TypeAlias("Headers", "Record<string, string>").EndTypeAlias()
			.Class("Client").EndClass());

		const string expected = """
			import { User } from "./types";

			type Headers = Record<string, string>;

			class Client {
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void TypeAliasHonorsSemicolonsOption()
	{
		CodeBuilder<TypeScript> ts = new(new CodeBuilderOptions { Semicolons = false });
		ts.File("api.ts")
			.TypeAlias("Headers", "Record<string, string>").EndTypeAlias()
			.EndFile();

		ts.Render().Trim().ShouldBe("type Headers = Record<string, string>");
	}

	[Fact]
	public void TypeAliasInsideClassIsSkippedWithWarning()
	{
		CodeBuilder<TypeScript> ts = new();
		ts.File("api.ts").Class("Client").EndClass().EndFile();
		((CodeClass)ts.Model!.Children[0]).Members.Add(new CodeTypeAlias { Name = "Headers", Definition = "Record<string, string>" });

		string result = ts.Render().Trim();
		result.ShouldBe("class Client {\n}");
		ts.Warnings.ShouldContain("Type alias 'Headers' inside a class is not supported in TypeScript; skipped.");
	}

	[Fact]
	public void TypeAliasInjectionWarnsInGDScript()
	{
		CodeBuilder<GDScript> gd = new();
		gd.File("script.gd").EndFile();
		gd.Model!.Children.Add(new CodeTypeAlias { Name = "Headers", Definition = "Record<string, string>" });

		gd.Render().Trim().ShouldBeEmpty();
		gd.Warnings.ShouldContain("Type aliases are not supported in GDScript; skipped.");
	}
}
