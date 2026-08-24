using Auxil.CodeGeneration.Common.Syntax.Shapes;

namespace Auxil.Tests.CodeGeneration;

/// <summary>
/// A scratch language proving the shape-hook design: it overrides the
/// Field and Signature hooks ("int ...health", "def move(dx) -> bool")
/// and falls through to the shared defaults for everything else.
/// </summary>
public struct Blurb : ILanguage, IHasImports, IHasConstructors
{
	public static string Name => "Blurb";

	public static LanguageSyntax Syntax => new()
	{
		BlockStyle = BlockStyle.Braces,
		LineCommentPrefix = "//",
		DocComment = DocCommentTokens.LinePrefixStyle("/// "),
		StatementTerminator = ";",
		ParameterStyle = ParameterStyle.TypeFirst,
		ReturnTypeStyle = ReturnTypeStyle.BeforeName,
		FileLayout = FileLayoutStyle.Module,
		ModifierKeywords = new Dictionary<CodeModifier, string>(),
		TypeMap = new Dictionary<string, string> { ["int"] = "int", ["bool"] = "bool" }
	};

	public static Shapes Shapes { get; } = new()
	{
		Field = static (ctx, name, type, _, defaultValue, _, _, _) =>
		{
			string line = $"{ctx.Syntax.MapType(type!)} ...{name}";
			if (defaultValue is not null)
			{
				line += $" = {defaultValue}";
			}

			return line;
		},
		Signature = static (ctx, kind, name, parameters, _, returnType, className, _) =>
		{
			string actualName = kind == SignatureKind.Constructor ? className : name;
			string keyword = kind switch
			{
				SignatureKind.Constructor => "init",
				SignatureKind.Function => "fn",
				_ => "def"
			};

			string signature = $"{keyword} {actualName}({string.Join(", ", parameters.Select(p => p.Name))})";
			if (returnType is not null)
			{
				signature += $" -> {ctx.Syntax.MapType(returnType)}";
			}

			return signature;
		}
	};
}

public class BlurbTest
{
	[Fact]
	public void FieldRendersWithCustomShape()
	{
		CodeBuilder<Blurb> code = new();
		code.File("thing.blurb")
			.Class("Thing")
			.Field("health", CodeTypes.Int, "100").EndField()
			.EndClass()
			.EndFile();

		code.Render().Trim().ShouldBe("class Thing {\n    int ...health = 100;\n}");
	}

	[Fact]
	public void MethodUsesCustomSignature()
	{
		CodeBuilder<Blurb> code = new();
		code.File("thing.blurb")
			.Class("Thing")
			.Method("move")
			.Parameter("dx", CodeTypes.Int)
			.Parameter("dy", CodeTypes.Int)
			.Returns(CodeTypes.Bool)
			.Body(b => b.Return("dx > 0"))
			.EndMethod()
			.EndClass()
			.EndFile();

		const string expected = """
			class Thing {
			    def move(dx, dy) -> bool {
			        return dx > 0;
			    }
			}
			""";

		code.Render().Trim().ShouldBe(expected);
	}

	[Fact]
	public void UnshapedStatementsFallThroughToSharedDefaults()
	{
		CodeBuilder<Blurb> code = new();
		code.File("thing.blurb")
			.Class("Thing")
			.Method("go")
			.Body(b => b
				.If("ready", i => i.Line("run()"))
				.ForEach("item", "items", f => f.Line("use(item)")))
			.EndMethod()
			.EndClass()
			.EndFile();

		const string expected = """
			class Thing {
			    def go() {
			        if (ready) {
			            run()
			        }
			        for (const item of items) {
			            use(item)
			        }
			    }
			}
			""";

		code.Render().Trim().ShouldBe(expected);
	}

	[Fact]
	public void ConstructorUsesClassNameAndCustomKeyword()
	{
		CodeBuilder<Blurb> code = new();
		code.File("thing.blurb")
			.Class("Thing")
			.Constructor().Parameter("dx", CodeTypes.Int).EndConstructor()
			.EndClass()
			.EndFile();

		code.Render().Trim().ShouldBe("class Thing {\n    init Thing(dx) {\n    }\n}");
	}
}
