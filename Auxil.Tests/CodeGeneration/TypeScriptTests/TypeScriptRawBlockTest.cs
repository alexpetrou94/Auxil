namespace Auxil.Tests.CodeGeneration.TypeScriptTests;

public class TypeScriptRawBlockTest
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
	public void CatalystNestedFunctionAndObjectLiteral()
	{
		string result = Render(ts => ts
			.Function("createClient")
			.Body(b =>
			{
				b.Block("async function request<T>(path: string, method: string, init?: RequestOptions): Promise<{ data: T; error: null } | { data: null; error: ProblemDetail }>", req =>
				{
					req.Line("const base = baseUrl.replace(/\\/$/, \"\");");
					req.If("init?.params", p =>
					{
						p.ForEach("[key, value]", "Object.entries(init.params).filter(([, v]) => v !== undefined)", e =>
							e.Line("url.searchParams.append(key, String(value));"));
					});
				});

				b.Block("return", ret =>
				{
					ret.Block("get:", g =>
					{
						g.Block("async getMedia(headers?: Record<string, string>): Promise<GetMediaResponseItem[]>", m =>
								m.Line("return request<GetMediaResponseItem[]>('/api/media/', 'get', { headers, signal });"),
							close: ",");
					}, close: ",");
				}, close: ";");
			})
			.EndFunction());

		const string expected = """
			function createClient() {
			    async function request<T>(path: string, method: string, init?: RequestOptions): Promise<{ data: T; error: null } | { data: null; error: ProblemDetail }> {
			        const base = baseUrl.replace(/\/$/, "");
			        if (init?.params) {
			            for (const [key, value] of Object.entries(init.params).filter(([, v]) => v !== undefined)) {
			                url.searchParams.append(key, String(value));
			            }
			        }
			    }
			    return {
			        get: {
			            async getMedia(headers?: Record<string, string>): Promise<GetMediaResponseItem[]> {
			                return request<GetMediaResponseItem[]>('/api/media/', 'get', { headers, signal });
			            },
			        },
			    };
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void BlockClosesBareWithoutSuffix()
	{
		string result = Render(ts => ts
			.Class("C")
			.Method("go")
			.Body(b => b
				.Block("guard", g => g.Line("log(\"entering\");"))
				.Line("run();"))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C {
			    go() {
			        guard {
			            log("entering");
			        }
			        run();
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void BlockSupportsStructuredStatementsInside()
	{
		string result = Render(ts => ts
			.Class("C")
			.Method("go")
			.Body(b => b
				.Block("setup", s => s
					.If("ready", i => i.Line("start();"))
					.ForEach("item", "items", e => e.Line("use(item);")))
				.Line("done();"))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C {
			    go() {
			        setup {
			            if (ready) {
			                start();
			            }
			            for (const item of items) {
			                use(item);
			            }
			        }
			        done();
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void RawMultiLineLineIndentsEveryLine()
	{
		string result = Render(ts => ts
			.Class("C")
			.Method("go")
			.Body(b => b.Line("first();\n\nsecond();"))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C {
			    go() {
			        first();

			        second();
			    }
			}
			""";

		result.ShouldBe(expected);
	}
}
