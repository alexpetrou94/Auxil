namespace Auxil.Tests.CodeGeneration.GDScriptTests;

public class GDScriptNestingTest
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
	public void InnerClassInsideClass()
	{
		string result = Render(gd => gd
			.Class("Outer")
			.Field("value", CodeTypes.Int).EndField()
			.Class("Inner")
			.Method("run").Body("print(\"inner\")").EndMethod()
			.EndClass()
			.EndClass());

		const string expected = """
			class Outer:
			    var value: int

			    class Inner:
			        func run():
			            print("inner")
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void DeeplyNestedClasses()
	{
		string result = Render(gd => gd
			.Class("A")
			.Class("B")
			.Class("C")
			.Method("go").Body("pass").EndMethod()
			.EndClass()
			.EndClass()
			.EndClass());

		const string expected = """
			class A:
			    class B:
			        class C:
			            func go():
			                pass
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void EnumInsideClass()
	{
		string result = Render(gd => gd
			.Class("Character")
			.Enum("Direction")
			.Member("NORTH")
			.Member("SOUTH")
			.EndEnum()
			.EndClass());

		const string expected = """
			class Character:
			    enum Direction {
			        NORTH,
			        SOUTH
			    }
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void InterfaceInsideClassIsSkippedWithWarning()
	{
		CodeBuilder<GDScript> gd = new();
		gd.File("test.gd").Class("C").EndClass().EndFile();
		((CodeClass)gd.Model!.Children[0]).Members.Add(new CodeInterface { Name = "I" });

		string result = gd.Render().Trim();
		result.ShouldBe("class C:\n    pass");
		gd.Warnings.ShouldContain("Interfaces are not supported in GDScript; skipped.");
	}

	[Fact]
	public void CommentInsideClass()
	{
		string result = Render(gd => gd
			.Class("C")
			.Comment("A note about the class")
			.Field("x", CodeTypes.Int).EndField()
			.EndClass());

		const string expected = """
			class C:
			    # A note about the class
			    var x: int
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void AbstractClassAndMethod()
	{
		string result = Render(gd => gd
			.Class("Shape").Abstract()
			.Method("draw").Abstract().EndMethod()
			.Method("describe").Body("return \"shape\"").EndMethod()
			.EndClass());

		const string expected = """
			@abstract class Shape:
			    @abstract func draw()

			    func describe():
			        return "shape"
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void StaticVariable()
	{
		string result = Render(gd => gd
			.Class("Counter")
			.Field("instances", CodeTypes.Int, "0").Static().EndField()
			.EndClass());

		const string expected = """
			class Counter:
			    static var instances: int = 0
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void MultiLineDocComment()
	{
		string result = Render(gd => gd
			.Method("update")
			.Doc("Updates the entity.\nCalled every frame.")
			.Body("pass")
			.EndMethod());

		const string expected = """
			## Updates the entity.
			## Called every frame.
			func update():
			    pass
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void ExtendsWithoutClassName()
	{
		string result = Render(gd => gd
			.Extends("Node")
			.Field("x", CodeTypes.Int).EndField());

		const string expected = """
			extends Node

			var x: int
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void EndAliasReturnsToParent()
	{
		string result = Render(gd => gd
			.Enum("State")
			.Member("IDLE")
			.End()
			.Field("x", CodeTypes.Int).EndField());

		const string expected = """
			enum State {
			    IDLE
			}

			var x: int
			""";

		result.ShouldBe(expected);
	}
}
