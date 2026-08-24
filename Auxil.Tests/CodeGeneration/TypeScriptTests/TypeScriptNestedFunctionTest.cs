
namespace Auxil.Tests.CodeGeneration.TypeScriptTests;

public class TypeScriptNestedFunctionTest
{
	private static string Render(Action<FileScope<TypeScript>> configure)
	{
		CodeBuilder<TypeScript> ts = new();
		FileScope<TypeScript> file = ts.File("client.ts");
		configure(file);
		file.EndFile();
		return ts.Render().Trim();
	}

	[Fact]
	public void AsyncNestedFunctionWithStructuredSignature()
	{
		string result = Render(ts => ts
			.Class("Client")
			.Method("createClient")
			.Body(b => b
				.Function("request", f => f
					.Async()
					.Generic("T")
					.Parameter("path", "string")
					.Parameter("method", "string")
					.Parameter("init", "RequestOptions").Nullable()
					.Returns("Promise<ApiResult<T>>")
					.Body(req => req
						.Line("const base = baseUrl.replace(/\\/$/, \"\");")
						.If("init?.params", p => p.Line("url.searchParams.append(key, String(value));"))
						.Return("{} as never"))))
			.EndMethod()
			.EndClass());

		const string expected = """
			class Client {
			    createClient() {
			        async function request<T>(path: string, method: string, init?: RequestOptions): Promise<ApiResult<T>> {
			            const base = baseUrl.replace(/\/$/, "");
			            if (init?.params) {
			                url.searchParams.append(key, String(value));
			            }
			            return {} as never;
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void NestedFunctionWithDocComment()
	{
		string result = Render(ts => ts
			.Class("Client")
			.Method("createClient")
			.Body(b => b
				.Function("request", f => f
					.Doc("Sends a request and returns the typed result.")
					.Async()
					.Generic("T")
					.Parameter("path", "string")
					.Returns("Promise<ApiResult<T>>")
					.Body(req => req.Return("{} as never"))))
			.EndMethod()
			.EndClass());

		const string expected = """
			class Client {
			    createClient() {
			        /**
			         * Sends a request and returns the typed result.
			         */
			        async function request<T>(path: string): Promise<ApiResult<T>> {
			            return {} as never;
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void NestedFunctionStatementSupportsEditing()
	{
		CodeBuilder<TypeScript> ts = new();
		ts.File("client.ts")
			.Class("Client")
			.Method("createClient")
			.Body(b => b
				.Function("request", f => f
					.Body(r => r.Line("first()", id: "inner")),
					id: "fn"))
			.EndMethod()
			.EndClass()
			.EndFile();

		ts.ReplaceStatement("inner", b => b.Line("replaced()"));
		ts.Remove("fn");

		ts.Render().Trim().ShouldBe("class Client {\n    createClient() {\n    }\n}");
	}

	[Fact]
	public void NestedFunctionInjectionWarnsInGDScript()
	{
		CodeBuilder<GDScript> gd = new();
		gd.File("script.gd").Method("go").EndMethod().EndFile();
		CodeMethod method = (CodeMethod)gd.Model!.Children[0];
		method.Body = new CodeBody();
		method.Body.Statements.Add(new CodeNestedFunction { Name = "helper" });

		string result = gd.Render().Trim();
		result.ShouldBe("func go():\n    pass");
		gd.Warnings.ShouldContain("Nested functions are not supported in GDScript; skipped.");
	}
}
