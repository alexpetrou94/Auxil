namespace Auxil.Tests.CodeGeneration.GDScriptTests;

public class GDScriptWarningsTest
{
	[Fact]
	public void GenericsAreSkippedWithWarning()
	{
		CodeBuilder<GDScript> gd = new();
		gd.File("container.gd").ClassName("Container").EndFile();
		gd.Model!.Children.Add(new CodeClass { Name = "Holder" });
		((CodeClass)gd.Model.Children[^1]).Generics.Add(new CodeGenericParameter { Name = "T" });

		string result = gd.Render().Trim();

		result.ShouldBe("class_name Container\n\nclass Holder:\n    pass");
		gd.Warnings.ShouldContain("Generics on class Holder are not supported in GDScript; skipped.");
	}

	[Fact]
	public void InterfaceIsSkippedWithWarning()
	{
		CodeBuilder<GDScript> gd = new();
		gd.File("iface.gd").EndFile();
		gd.Model!.Children.Add(new CodeInterface { Name = "ITest" });

		gd.Render().Trim().ShouldBeEmpty();
		gd.Warnings.ShouldContain("Interfaces are not supported in GDScript; skipped.");
	}

	[Fact]
	public void TopLevelFunctionIsSkippedWithWarning()
	{
		CodeBuilder<GDScript> gd = new();
		gd.File("func.gd").EndFile();
		gd.Model!.Children.Add(new CodeFunction { Name = "helper" });

		gd.Render().Trim().ShouldBeEmpty();
		gd.Warnings.ShouldContain("Top-level function 'helper' is not supported in GDScript; skipped. Use Method() instead.");
	}

	[Fact]
	public void ImportIsSkippedWithWarning()
	{
		CodeBuilder<GDScript> gd = new();
		gd.File("imports.gd").EndFile();
		gd.Model!.Children.Add(new CodeImport { From = "./types" });
		((CodeImport)gd.Model.Children[^1]).NamedNames.Add("User");

		gd.Render().Trim().ShouldBeEmpty();
		gd.Warnings.ShouldContain("Import './types' is not supported in GDScript; skipped.");
	}

	[Fact]
	public void UnsupportedModifiersAreSkippedWithWarning()
	{
		CodeBuilder<GDScript> gd = new();
		gd.File("mods.gd").EndFile();
		CodeMethod method = new() { Name = "foo", Modifiers = CodeModifier.Public | CodeModifier.Async };
		method.Body = new CodeBody();
		method.Body.Statements.Add(new CodeExpressionStatement { Line = "pass" });
		gd.Model!.Children.Add(method);

		string result = gd.Render().Trim();
		result.ShouldBe("func foo():\n    pass");
		gd.Warnings.ShouldContain("Public modifier is not supported in GDScript; skipped.");
		gd.Warnings.ShouldContain("Async modifier is not supported in GDScript; skipped.");
	}
}
