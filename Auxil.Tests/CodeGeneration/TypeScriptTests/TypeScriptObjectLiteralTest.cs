
namespace Auxil.Tests.CodeGeneration.TypeScriptTests;

public class TypeScriptObjectLiteralTest
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
	public void ReturnObjectWithNestedVerbsAndFunctionMembers()
	{
		string result = Render(ts => ts
			.Class("Client")
			.Method("createClient")
			.Returns("Client")
			.Body(b => b
				.ObjectLiteral("return", obj => obj
					.Member("get", get => get
						.Member("getMedia", m => m
							.Async()
							.Parameter("headers", "Record<string, string>").Nullable()
							.Parameter("signal", "AbortSignal").Nullable()
							.Returns("Promise<ApiResult<GetMediaResponseItem[]>>")
							.Body(fn => fn.Return("request<GetMediaResponseItem[]>('/api/media/', 'get', { headers, signal })"))))))
			.EndMethod()
			.EndClass());

		const string expected = """
			class Client {
			    createClient(): Client {
			        return {
			            get: {
			                async getMedia(headers?: Record<string, string>, signal?: AbortSignal): Promise<ApiResult<GetMediaResponseItem[]>> {
			                    return request<GetMediaResponseItem[]>('/api/media/', 'get', { headers, signal });
			                },
			            },
			        };
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void MemberFunctionWithDocComment()
	{
		string result = Render(ts => ts
			.Class("Client")
			.Method("createClient")
			.Returns("Client")
			.Body(b => b
				.ObjectLiteral("return", obj => obj
					.Member("get", get => get
						.Member("getMedia", m => m
							.Doc("Fetches media items by uid")
							.Async()
							.Returns("Promise<ApiResult<GetMediaResponseItem[]>>")
							.Body(fn => fn.Return("request<GetMediaResponseItem[]>('/api/media/', 'get', { headers, signal })"))))))
			.EndMethod()
			.EndClass());

		const string expected = """
			class Client {
			    createClient(): Client {
			        return {
			            get: {
			                /**
			                 * Fetches media items by uid
			                 */
			                async getMedia(): Promise<ApiResult<GetMediaResponseItem[]>> {
			                    return request<GetMediaResponseItem[]>('/api/media/', 'get', { headers, signal });
			                },
			            },
			        };
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void ObjectLiteralWithExpressionMembersAndDepth()
	{
		string result = Render(ts => ts
			.Class("Client")
			.Method("config")
			.Body(b => b
				.ObjectLiteral(obj => obj
					.Member("version", "1")
					.Member("nested", n => n
						.Member("deep", d => d
							.Member("value", "\"x\"")))))
			.EndMethod()
			.EndClass());

		const string expected = """
			class Client {
			    config() {
			        {
			            version: 1,
			            nested: {
			                deep: {
			                    value: "x",
			                },
			            },
			        };
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void ObjectLiteralInjectionWarnsInGDScript()
	{
		CodeBuilder<GDScript> gd = new();
		gd.File("script.gd").Method("go").EndMethod().EndFile();
		CodeMethod method = (CodeMethod)gd.Model!.Children[0];
		method.Body = new CodeBody();
		method.Body.Statements.Add(new CodeObjectLiteral { Prefix = "return" });

		string result = gd.Render().Trim();
		result.ShouldBe("func go():\n    pass");
		gd.Warnings.ShouldContain("Object literals are not supported in GDScript; skipped.");
	}
}
