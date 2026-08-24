namespace Auxil.Tests.CodeGeneration.TypeScriptTests;

public class TypeScriptBuilderTest
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
	public void BasicClass()
	{
		string result = Render(ts => ts
			.Class("ApiClient")
			.Export()
			.Extends("BaseClient")
			.Implements("IDisposable")
			.Field("baseUrl", CodeTypes.String).Private().EndField()
			.Constructor()
			.Parameter("baseUrl", CodeTypes.String)
			.Body(b => b.Line("this.baseUrl = baseUrl;"))
			.EndConstructor()
			.Method("get")
			.Public().Async()
			.Parameter("endpoint", CodeTypes.String)
			.Returns("Promise<User>")
			.Body(b => b.Line("return fetch(endpoint).then(r => r.json());"))
			.EndMethod()
			.EndClass());

		const string expected = """
		                        export class ApiClient extends BaseClient implements IDisposable {
		                            private baseUrl: string;

		                            constructor(baseUrl: string) {
		                                this.baseUrl = baseUrl;
		                            }

		                            public async get(endpoint: string): Promise<User> {
		                                return fetch(endpoint).then(r => r.json());
		                            }
		                        }
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void ClassWithGenerics()
	{
		string result = Render(ts => ts
			.Class("Repository")
			.Generic("T", null, "unknown")
			.Field("items", "T[]").EndField()
			.Method("findAll").Returns("T[]").Body("return this.items;").EndMethod()
			.EndClass());

		const string expected = """
		                        class Repository<T = unknown> {
		                            items: T[];

		                            findAll(): T[] {
		                                return this.items;
		                            }
		                        }
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void InterfaceWithPropertiesAndMethods()
	{
		string result = Render(ts => ts
			.Interface("Repository")
			.Export()
			.Generic("T", null, "unknown")
			.Extends("BaseRepository")
			.Field("items", "T[]").EndField()
			.Method("findAll").Returns("T[]").EndMethod()
			.EndInterface());

		const string expected = """
		                        export interface Repository<T = unknown> extends BaseRepository {
		                            items: T[];

		                            findAll(): T[];
		                        }
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void Enum()
	{
		string result = Render(ts => ts
			.Enum("HttpMethod")
			.Export()
			.Member("Get", "\"GET\"")
			.Member("Post", "\"POST\"")
			.EndEnum());

		const string expected = """
		                        export enum HttpMethod {
		                            Get = "GET",
		                            Post = "POST"
		                        };
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void TopLevelFunction()
	{
		string result = Render(ts => ts
			.Function("createUser")
			.Export()
			.Parameter("name", CodeTypes.String)
			.Returns("User")
			.Body(b => b.Line("return { name };"))
			.EndFunction());

		const string expected = """
		                        export function createUser(name: string): User {
		                            return { name };
		                        }
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void Imports()
	{
		string result = Render(ts => ts
			.Import("./types", null, "User", "ApiResponse")
			.Import("axios", "axios")
			.Class("Client").EndClass());

		const string expected = """
		                        import { User, ApiResponse } from "./types";
		                        import axios from "axios";

		                        class Client {
		                        }
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void TopLevelConstField()
	{
		string result = Render(ts => ts
			.Field("API_VERSION", CodeTypes.String, "\"v1\"").Const().Export().EndField());

		result.ShouldBe("export const API_VERSION: string = \"v1\";");
	}

	[Fact]
	public void TopLevelLetField()
	{
		string result = Render(ts => ts
			.Field("counter", CodeTypes.Int, "0").EndField());

		result.ShouldBe("let counter: number = 0;");
	}

	[Fact]
	public void AbstractMethod()
	{
		string result = Render(ts => ts
			.Class("Shape")
			.Method("draw").Abstract().Returns(CodeTypes.Void).EndMethod()
			.EndClass());

		const string expected = """
		                        class Shape {
		                            abstract draw(): void;
		                        }
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void JSDocComments()
	{
		string result = Render(ts => ts
			.Class("Service")
			.Doc("A service that does things.")
			.Method("run")
			.Doc("Runs the service.")
			.Returns(CodeTypes.Void)
			.Body(b => b.Line("this.running = true;"))
			.EndMethod()
			.EndClass());

		const string expected = """
			/**
			 * A service that does things.
			 */
			class Service {
			    /**
			     * Runs the service.
			     */
			    run(): void {
			        this.running = true;
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void Decorators()
	{
		string result = Render(ts => ts
			.Class("HealthController")
			.Annotate("Injectable")
			.Annotate("Controller", "\"/health\"")
			.EndClass());

		const string expected = """
		                        @Injectable
		                        @Controller("/health")
		                        class HealthController {
		                        }
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void NoSemicolonsOption()
	{
		CodeBuilderOptions options = new() { Semicolons = false };
		CodeBuilder<TypeScript> ts = new(options);
		ts.File("test.ts")
			.Import("./types", null, "User")
			.Class("Client")
			.Field("name", CodeTypes.String).EndField()
			.EndClass()
			.EndFile();

		string result = ts.Render().Trim();

		const string expected = """
		                        import { User } from "./types"

		                        class Client {
		                            name: string
		                        }
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void SignalIsSkippedWithWarning()
	{
		CodeBuilder<TypeScript> ts = new();
		ts.File("test.ts").Class("Player").EndClass().EndFile();
		((CodeClass)ts.Model!.Children[0]).Members.Add(new CodeSignal { Name = "hit" });

		string result = ts.Render().Trim();
		result.ShouldBe("class Player {\n}");
		ts.Warnings.ShouldContain("Signal 'hit' is not supported in TypeScript; skipped.");
	}
}
