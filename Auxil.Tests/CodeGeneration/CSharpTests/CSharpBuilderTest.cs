namespace Auxil.Tests.CodeGeneration.CSharpTests;

public class CSharpBuilderTest
{
	private static string Render(Action<FileScope<CSharp>> configure)
	{
		CodeBuilder<CSharp> cs = new();
		FileScope<CSharp> file = cs.File("test.cs");
		configure(file);
		file.EndFile();
		return cs.Render().Trim();
	}

	[Fact]
	public void NamespaceWithClassAndUsings()
	{
		string result = Render(cs => cs
			.Import("System.Collections.Generic")
			.Namespace("UPhoricStudio.Game")
			.Class("Repository")
			.Field("items", "List<string>").EndField()
			.EndClass()
			.EndNamespace());

		const string expected = """
			using System.Collections.Generic;

			namespace UPhoricStudio.Game;

			class Repository
			{
			    List<string> items;
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void AliasImport()
	{
		string result = Render(cs => cs
			.Import("System.Collections.Generic", "SCG")
			.Class("Repository").EndClass());

		const string expected = """
			using SCG = System.Collections.Generic;

			class Repository
			{
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void BasicClass()
	{
		string result = Render(cs => cs
			.Class("Player")
			.Public()
			.Extends("Entity")
			.Implements("IDamageable")
			.Field("health", CodeTypes.Int, "100").Private().EndField()
			.Property("Name", CodeTypes.String).EndProperty()
			.Method("TakeDamage")
			.Public()
			.Parameter("amount", CodeTypes.Int)
			.Returns(CodeTypes.Bool)
			.Body(b => b
				.Line("Health -= amount;")
				.Return("Health > 0"))
			.EndMethod()
			.EndClass());

		const string expected = """
			public class Player : Entity, IDamageable
			{
			    private int health = 100;
			    string Name { get; set; }

			    public bool TakeDamage(int amount)
			    {
			        Health -= amount;
			        return Health > 0;
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void ConstructorNamedAfterClass()
	{
		string result = Render(cs => cs
			.Class("Player")
			.Public()
			.Constructor()
			.Public()
			.Parameter("health", CodeTypes.Int)
			.Body(b => b.Line("Health = health;"))
			.EndConstructor()
			.EndClass());

		const string expected = """
			public class Player
			{
			    public Player(int health)
			    {
			        Health = health;
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void RawBlockFollowsAllmanByDefault()
	{
		string result = Render(cs => cs
			.Class("C")
			.Method("go")
			.Body(b => b
				.Block("guard", g => g.Line("Console.WriteLine(\"x\");"))
				.Line("run();"))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C
			{
			    void go()
			    {
			        guard
			        {
			            Console.WriteLine("x");
			        }
			        run();
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void RawBlockFollowsKAndRWhenConfigured()
	{
		CodeBuilderOptions options = new() { BraceStyle = BraceStyle.KAndR };
		CodeBuilder<CSharp> cs = new(options);
		cs.File("test.cs")
			.Class("C")
			.Method("go")
			.Body(b => b.Block("guard", g => g.Line("Console.WriteLine(\"x\");")))
			.EndMethod()
			.EndClass()
			.EndFile();

		string result = cs.Render().Trim();

		const string expected = """
			class C {
			    void go() {
			        guard {
			            Console.WriteLine("x");
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void RawMultiLineLineIndentsEveryLine()
	{
		string result = Render(cs => cs
			.Class("C")
			.Method("go")
			.Body(b => b.Line("Console.WriteLine(\"first\");\n\nConsole.WriteLine(\"second\");"))
			.EndMethod()
			.EndClass());

		const string expected = """
			class C
			{
			    void go()
			    {
			        Console.WriteLine("first");

			        Console.WriteLine("second");
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void GenericClassWithWhereClause()
	{
		string result = Render(cs => cs
			.Class("Repository")
			.Public()
			.Generic("T", "class")
			.Field("items", "List<T>").EndField()
			.EndClass());

		const string expected = """
			public class Repository<T> where T : class
			{
			    List<T> items;
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void GenericDefaultValueIsSkippedWithWarning()
	{
		CodeBuilder<CSharp> cs = new();
		cs.File("test.cs")
			.Class("Repository").Generic("T", null, "object").EndClass()
			.EndFile();

		string result = cs.Render().Trim();
		result.ShouldBe("class Repository<T>\n{\n}");
		cs.Warnings.ShouldContain("Default generic type argument on 'T' is not supported in C#; skipped.");
	}

	[Fact]
	public void InterfaceWithPropertyAndMethod()
	{
		string result = Render(cs => cs
			.Interface("IRepository")
			.Public()
			.Generic("T")
			.Extends("IDisposable")
			.Property("Count", CodeTypes.Int).GetOnly().EndProperty()
			.Method("Find").Returns("T").EndMethod()
			.EndInterface());

		const string expected = """
			public interface IRepository<T> : IDisposable
			{
			    int Count { get; }

			    T Find();
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void InterfaceMethodRendersMetadataAndGenericWarning()
	{
		CodeBuilder<CSharp> cs = new();
		cs.File("test.cs")
			.Interface("IRepository")
			.Method("Find")
			.Doc("Finds a value.")
			.Annotate("Obsolete")
			.Generic("T", defaultValue: "object")
			.Returns("T")
			.EndMethod()
			.EndInterface()
			.EndFile();

		string result = cs.Render().Trim();
		result.ShouldContain("/// Finds a value.");
		result.ShouldContain("[Obsolete]");
		result.ShouldContain("T Find<T>();");
		cs.Warnings.ShouldContain("Default generic type argument on 'T' is not supported in C#; skipped.");
	}

	[Fact]
	public void EnumRendersMetadata()
	{
		string result = Render(cs => cs
			.Enum("State")
			.Public()
			.Doc("Application state.")
			.Annotate("Flags")
			.Member("Idle")
			.EndEnum());

		result.ShouldContain("/// Application state.");
		result.ShouldContain("[Flags]");
		result.ShouldContain("public enum State");
	}

	[Fact]
	public void ModuleFileRendersDocumentationAndAnnotations()
	{
		CodeBuilder<CSharp> cs = new();
		cs.File("test.cs")
			.Doc("Generated declarations.")
			.Annotate("Generated")
			.Import("System")
			.Class("C")
			.EndClass()
			.EndFile();

		string result = cs.Render().Trim();
		result.ShouldBe("/// Generated declarations.\n[Generated]\nusing System;\n\nclass C\n{\n}");
	}

	[Fact]
	public void NullableFieldRendersNullableType()
	{
		string result = Render(cs => cs
			.Class("C")
			.Field("count", CodeTypes.Int).Nullable().EndField()
			.EndClass());

		const string expected = """
			class C
			{
			    int? count;
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void NullablePropertyRendersNullableType()
	{
		string result = Render(cs => cs
			.Class("C")
			.Property("name", CodeTypes.String).Nullable().EndProperty()
			.EndClass());

		const string expected = """
			class C
			{
			    string? name { get; set; }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void NullableInterfacePropertyRendersNullableType()
	{
		string result = Render(cs => cs
			.Interface("I")
			.Property("name", CodeTypes.String).Nullable().EndProperty()
			.EndInterface());

		const string expected = """
			interface I
			{
			    string? name { get; set; }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void NullableParameterRendersNullableType()
	{
		string result = Render(cs => cs
			.Class("C")
			.Method("greet")
			.Parameter("name", CodeTypes.String).Nullable()
			.EndMethod()
			.EndClass());

		const string expected = """
			class C
			{
			    void greet(string? name)
			    {
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void PropertyWithAccessorBodies()
	{
		string result = Render(cs => cs
			.Class("Player")
			.Public()
			.Field("_health", CodeTypes.Int).Private().EndField()
			.Property("Health", CodeTypes.Int).Public()
			.Getter(g => g.Line("return _health;"))
			.Setter(s => s.Line("_health = value;"))
			.EndProperty()
			.EndClass());

		const string expected = """
			public class Player
			{
			    private int _health;
			    public int Health
			    {
			        get
			        {
			            return _health;
			        }
			        set
			        {
			            _health = value;
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void GetOnlyAutoProperty()
	{
		string result = Render(cs => cs
			.Class("Player")
			.Public()
			.Property("Health", CodeTypes.Int).Public().GetOnly().EndProperty()
			.EndClass());

		const string expected = """
			public class Player
			{
			    public int Health { get; }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void Enum()
	{
		string result = Render(cs => cs
			.Enum("State")
			.Public()
			.Member("Idle")
			.Member("Running", "2")
			.EndEnum());

		const string expected = """
			public enum State
			{
			    Idle,
			    Running = 2
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void AbstractMethod()
	{
		string result = Render(cs => cs
			.Class("Shape")
			.Public()
			.Method("Draw").Public().Abstract().Returns(CodeTypes.Void).EndMethod()
			.EndClass());

		const string expected = """
			public class Shape
			{
			    public abstract void Draw();
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void AbstractMethodWithBodyDropsBody()
	{
		string result = Render(cs => cs
			.Class("Shape")
			.Public()
			.Method("Draw").Public().Abstract().Returns(CodeTypes.Void).Body("throw new NotImplementedException();").EndMethod()
			.EndClass());

		const string expected = """
			public class Shape
			{
			    public abstract void Draw();
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void AsyncMethod()
	{
		string result = Render(cs => cs
			.Class("Service")
			.Public()
			.Method("Fetch").Public().Async().Returns("Task<string>").Body("return Task.FromResult(\"ok\");").EndMethod()
			.EndClass());

		const string expected = """
			public class Service
			{
			    public async Task<string> Fetch()
			    {
			        return Task.FromResult("ok");
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void Attributes()
	{
		string result = Render(cs => cs
			.Class("Player").Annotate("Serializable").EndClass());

		const string expected = """
			[Serializable]
			class Player
			{
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void DocCommentsUseXmlStyle()
	{
		string result = Render(cs => cs
			.Class("Player")
			.Doc("The player character.\nSecond line.")
			.EndClass());

		const string expected = """
			/// The player character.
			/// Second line.
			class Player
			{
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void TypeMapping()
	{
		string result = Render(cs => cs
			.Class("Types")
			.Field("name", CodeTypes.String).EndField()
			.Field("count", CodeTypes.Int).EndField()
			.Field("ratio", CodeTypes.Double).EndField()
			.Field("enabled", CodeTypes.Bool).EndField()
			.EndClass());

		const string expected = """
			class Types
			{
			    string name;
			    int count;
			    double ratio;
			    bool enabled;
			}
			""";

		result.ShouldBe(expected);
	}
}
