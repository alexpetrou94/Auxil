namespace Auxil.Tests.CodeGeneration.GDScriptTests;

public class RenderGuardTest
{
	[Fact]
	public void SecondFileThrows()
	{
		CodeBuilder<GDScript> gd = new();
		gd.File("a.gd");
		Should.Throw<CodeBuilderException>(() => gd.File("b.gd"))
			.Message.ShouldBe("A file is already open.");
	}

	[Fact]
	public void RenderWithoutFileThrows()
	{
		CodeBuilder<GDScript> gd = new();
		Should.Throw<CodeBuilderException>(gd.Render)
			.Message.ShouldBe("No file open. Call File() first.");
	}

	[Fact]
	public void RenderWhileFileOpenThrows()
	{
		CodeBuilder<GDScript> gd = new();
		gd.File("a.gd");
		Should.Throw<CodeBuilderException>(gd.Render)
			.Message.ShouldBe("File is still open. Call EndFile() before rendering.");
	}
}
