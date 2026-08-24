namespace Auxil.Tests.CodeGeneration.GDScriptTests;

public class GDScriptBuilderTest
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
	public void BasicScript()
	{
		string result = Render(gd => gd
			.ClassName("Player")
			.Extends("Node")
			.Field("health", CodeTypes.Int, "100").EndField()
			.Method("_ready").Body("pass").EndMethod());

		const string expected = """
		                        class_name Player
		                        extends Node

		                        var health: int = 100

		                        func _ready():
		                            pass
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void UntitledScriptWithoutClassIdentity()
	{
		string result = Render(gd => gd
			.Field("value", CodeTypes.Int).EndField());

		result.ShouldBe("var value: int");
	}

	[Fact]
	public void FieldsAreAdjacentWithoutBlankLines()
	{
		string result = Render(gd => gd
			.Field("a", CodeTypes.Int).EndField()
			.Field("b", CodeTypes.Int).EndField()
			.Method("sum").Body("pass").EndMethod());

		const string expected = """
		                        var a: int
		                        var b: int

		                        func sum():
		                            pass
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void SignalWithTypedParameters()
	{
		string result = Render(gd => gd
			.Signal("hit")
			.Parameter("damage", CodeTypes.Int)
			.Parameter("source", CodeTypes.String)
			.EndSignal());

		result.ShouldBe("signal hit(damage: int, source: String)");
	}

	[Fact]
	public void SignalWithoutParameters()
	{
		string result = Render(gd => gd
			.Signal("died").EndSignal());

		result.ShouldBe("signal died()");
	}

	[Fact]
	public void EnumWithValues()
	{
		string result = Render(gd => gd
			.Enum("State")
			.Member("IDLE")
			.Member("RUNNING", "2")
			.Member("DEAD")
			.EndEnum());

		const string expected = """
		                        enum State {
		                            IDLE,
		                            RUNNING = 2,
		                            DEAD
		                        }
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void Annotations()
	{
		string result = Render(gd => gd
			.Field("speed", CodeTypes.Float, "5.0").Annotate("export").EndField()
			.Field("sprite", "Sprite2D", "get_node(\"Sprite2D\")").Annotate("onready").EndField());

		const string expected = """
		                        @export
		                        var speed: float = 5.0

		                        @onready
		                        var sprite: Sprite2D = get_node("Sprite2D")
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void AnnotationWithArguments()
	{
		string result = Render(gd => gd
			.Field("volume", CodeTypes.Float, "0.5").Annotate("export_range", "0.0", "1.0").EndField());

		const string expected = """
		                        @export_range(0.0, 1.0)
		                        var volume: float = 0.5
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void FileAnnotations()
	{
		string result = Render(gd => gd
			.Annotate("tool")
			.ClassName("Level")
			.Extends("Node"));

		const string expected = """
			@tool
			class_name Level
			extends Node
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void DocComments()
	{
		string result = Render(gd => gd
			.Field("health", CodeTypes.Int)
			.Doc("Current health of the entity.")
			.EndField());

		const string expected = """
		                        ## Current health of the entity.
		                        var health: int
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void FileDocComment()
	{
		string result = Render(gd => gd
			.Doc("The player character.")
			.ClassName("Player")
			.Extends("CharacterBody2D"));

		const string expected = """
		                        ## The player character.
		                        class_name Player
		                        extends CharacterBody2D
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void MethodWithParametersReturnAndBody()
	{
		string result = Render(gd => gd
			.Method("take_damage")
			.Parameter("amount", CodeTypes.Int)
			.Returns(CodeTypes.Bool)
			.Body(b => b
				.Line("health -= amount")
				.Line("return health > 0"))
			.EndMethod());

		const string expected = """
		                        func take_damage(amount: int) -> bool:
		                            health -= amount
		                            return health > 0
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void ParameterWithDefaultValue()
	{
		string result = Render(gd => gd
			.Method("move")
			.Parameter("direction", CodeTypes.String, "\"up\"")
			.Body("pass")
			.EndMethod());

		result.ShouldBe("func move(direction: String = \"up\"):\n    pass");
	}

	[Fact]
	public void RawBlockIndentsContentsWithHeaderTerminator()
	{
		string result = Render(gd => gd
			.Method("go")
			.Body(b => b
				.Block("guard", g => g.Line("health = 100"))
				.Line("run()"))
			.EndMethod());

		const string expected = """
		                        func go():
		                            guard:
		                                health = 100
		                            run()
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void RawMultiLineLineIndentsEveryLine()
	{
		string result = Render(gd => gd
			.Method("go")
			.Body(b => b.Line("first()\n\nsecond()"))
			.EndMethod());

		const string expected = """
		                        func go():
		                            first()

		                            second()
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void NullableIsIgnored()
	{
		string result = Render(gd => gd
			.Field("name", CodeTypes.String).Nullable().EndField()
			.Method("move")
			.Parameter("direction", CodeTypes.String).Nullable()
			.Body("pass")
			.EndMethod());

		const string expected = """
		                        var name: String

		                        func move(direction: String):
		                            pass
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void EmptyMethodGetsPass()
	{
		string result = Render(gd => gd
			.Method("_ready").EndMethod());

		const string expected = """
		                        func _ready():
		                            pass
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void Constructor()
	{
		string result = Render(gd => gd
			.Constructor()
			.Parameter("health", CodeTypes.Int)
			.Body(b => b.Line("self.health = health"))
			.EndConstructor());

		const string expected = """
		                        func _init(health: int):
		                            self.health = health
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void InnerClass()
	{
		string result = Render(gd => gd
			.Class("Data")
			.Field("value", CodeTypes.Int).EndField()
			.EndClass());

		const string expected = """
		                        class Data:
		                            var value: int
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void ConstField()
	{
		string result = Render(gd => gd
			.Field("MAX_HEALTH", CodeTypes.Int, "100").Const().EndField());

		result.ShouldBe("const MAX_HEALTH: int = 100");
	}

	[Fact]
	public void StaticMethod()
	{
		string result = Render(gd => gd
			.Method("create").Static().Returns(CodeTypes.String).Body("return \"player\"").EndMethod());

		const string expected = """
		                        static func create() -> String:
		                            return "player"
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void BodyWithCommentsAndBlankLines()
	{
		string result = Render(gd => gd
			.Method("process").Body(b => b
				.Comment("Handle input")
				.Line("if Input.is_action_just_pressed(\"jump\"):")
				.BlankLine()
				.Line("jump()"))
			.EndMethod());

		const string expected = """
		                        func process():
		                            # Handle input
		                            if Input.is_action_just_pressed("jump"):

		                            jump()
		                        """;

		result.ShouldBe(expected);
	}

	[Fact]
	public void TypeMapping()
	{
		string result = Render(gd => gd
			.Field("ratio", CodeTypes.Double).EndField()
			.Field("name", CodeTypes.String).EndField()
			.Field("data", CodeTypes.Dynamic).EndField()
			.Field("custom", "MyCustomType").EndField());

		const string expected = """
			var ratio: float
			var name: String
			var data: Variant
			var custom: MyCustomType
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void LoopsBuildMultipleMembers()
	{
		CodeBuilder<GDScript> gd = new();
		FileScope<GDScript> file = gd.File("stats.gd");
		file.ClassName("Stats");

		foreach (string stat in new[] { "strength", "agility", "intelligence" })
		{
			file.Field(stat, CodeTypes.Int, "1").EndField();
		}

		file.EndFile();

		string result = gd.Render().Trim();
		const string expected = """
		                        class_name Stats

		                        var strength: int = 1
		                        var agility: int = 1
		                        var intelligence: int = 1
		                        """;

		result.ShouldBe(expected);
	}
}
