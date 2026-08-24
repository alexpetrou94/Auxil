namespace Auxil.Tests.CodeGeneration;

/// <summary>
/// Locks in the Catch/Finally/ElseIf/Else binding semantics: each binds
/// to the most recent Try/If statement in the CURRENT body — nested
/// callbacks build separate bodies, so a statement nested in a sub-block
/// is never the target.
/// </summary>
public class BodyBindingTest
{
	private static string Render(Action<BodyBuilder<CSharp>> configure)
	{
		CodeBuilder<CSharp> cs = new();
		FileScope<CSharp> file = cs.File("test.cs");
		file.Class("C").Method("go").Body(configure).EndMethod().EndClass();
		file.EndFile();
		return cs.Render().Trim();
	}

	[Fact]
	public void CatchBindsToMostRecentSiblingTry()
	{
		string result = Render(b => b
			.Try(t => t.Line("first()"))
			.Catch("e", c => c.Line("handle-first()"))
			.Try(t => t.Line("second()"))
			.Catch("e", c => c.Line("handle()")));

		const string expected = """
			class C
			{
			    void go()
			    {
			        try
			        {
			            first()
			        }
			        catch (Exception e)
			        {
			            handle-first()
			        }
			        try
			        {
			            second()
			        }
			        catch (Exception e)
			        {
			            handle()
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void CatchInEnclosingBodyBindsToEnclosingTry()
	{
		string result = Render(b => b
			.Try(t => t.Line("outer()"))
			.If("cond", i => i
				.Try(x => x.Line("inner()"))
				.Catch("e", c => c.Line("catch-inner()")))
			.Catch("e", c => c.Line("catch-outer()")));

		const string expected = """
			class C
			{
			    void go()
			    {
			        try
			        {
			            outer()
			        }
			        catch (Exception e)
			        {
			            catch-outer()
			        }
			        if (cond)
			        {
			            try
			            {
			                inner()
			            }
			            catch (Exception e)
			            {
			                catch-inner()
			            }
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void CatchInsideNestedCallbackBindsToNestedTry()
	{
		string result = Render(b => b
			.If("cond", i => i
				.Try(x => x.Line("inner()"))
				.Catch("e", c => c.Line("catch-inner()"))));

		const string expected = """
			class C
			{
			    void go()
			    {
			        if (cond)
			        {
			            try
			            {
			                inner()
			            }
			            catch (Exception e)
			            {
			                catch-inner()
			            }
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void InterleavedTriesBindPairwise()
	{
		string result = Render(b => b
			.Try(t => t.Line("a()"))
			.Catch("e", c => c.Line("ca()"))
			.Try(t => t.Line("b()"))
			.Catch("e", c => c.Line("cb()"))
			.Finally(f => f.Line("fin()")));

		const string expected = """
			class C
			{
			    void go()
			    {
			        try
			        {
			            a()
			        }
			        catch (Exception e)
			        {
			            ca()
			        }
			        try
			        {
			            b()
			        }
			        catch (Exception e)
			        {
			            cb()
			        }
			        finally
			        {
			            fin()
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void ElseBindsToMostRecentIf()
	{
		string result = Render(b => b
			.If("a", i => i.Line("x()"))
			.If("b", i => i.Line("y()"))
			.Else(e => e.Line("z()")));

		const string expected = """
			class C
			{
			    void go()
			    {
			        if (a)
			        {
			            x()
			        }
			        if (b)
			        {
			            y()
			        }
			        else
			        {
			            z()
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}

	[Fact]
	public void ElseIfBindsToMostRecentIf()
	{
		string result = Render(b => b
			.If("a", i => i.Line("x()"))
			.If("b", i => i.Line("y()"))
			.ElseIf("c", e => e.Line("z()")));

		const string expected = """
			class C
			{
			    void go()
			    {
			        if (a)
			        {
			            x()
			        }
			        if (b)
			        {
			            y()
			        }
			        else if (c)
			        {
			            z()
			        }
			    }
			}
			""";

		result.ShouldBe(expected);
	}
}
