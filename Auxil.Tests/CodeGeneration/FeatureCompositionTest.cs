using Auxil.CodeGeneration.Common.Rendering;

namespace Auxil.Tests.CodeGeneration;

/// <summary>
/// A scratch language composed of traits and a syntax profile, proving
/// composition drives the pipeline: no per-language renderer code
/// is involved.
/// </summary>
public struct ScratchLanguage : ILanguage, IHasScriptClass, IHasSignals
{
	public static string Name => "Scratch";

	public static LanguageSyntax Syntax => new()
	{
		BlockStyle = BlockStyle.Indentation,
		HeaderTerminator = ":",
		LineCommentPrefix = "//",
		DocComment = DocCommentTokens.LinePrefixStyle("/// "),
		StatementTerminator = string.Empty,
		ParameterStyle = ParameterStyle.NameFirst,
		ReturnTypeStyle = ReturnTypeStyle.ArrowAfterParens,
		FileLayout = FileLayoutStyle.ScriptClass,
		ModifierKeywords = new Dictionary<CodeModifier, string>(),
		TypeMap = new Dictionary<string, string>(),
		KeywordMethod = "def",
		KeywordConstructor = "_init",
		KeywordSignal = "event",
		KeywordMatch = "match",
		KeywordPass = "pass",
		KeywordExtends = "extends",
		KeywordImplements = string.Empty,
		KeywordDefaultArm = "_",
		KeywordElseIf = "elif",
		FieldKeyword = "var",
		TopLevelFieldKeyword = "var",
		GenericStyle = GenericStyle.None,
		MatchArmStyle = MatchArmStyle.Colon,
		PropertyStyle = PropertyStyle.BlockForm,
		ConditionParens = false,
		ForEachKeyword = "for",
		ForEachVariable = string.Empty,
		ForEachConnector = "in",
		LocalVarKeyword = "var",
		SupportsNullable = false,
		EnumTrailingComma = false
	};
}

public class FeatureCompositionTest
{
	[Fact]
	public void CustomLanguageComposesFromTraitsAndSyntax()
	{
		CodeBuilder<ScratchLanguage> code = new();
		code.File("scratch.scr")
			.ClassName("Widget")
			.Signal("changed").EndSignal()
			.Method("run")
			.Body(b => b
				.If("ready", i => i.Line("work()"))
				.Return("done"))
			.EndMethod()
			.EndFile();

		const string expected = """
			class_name Widget

			event changed()

			def run():
			    if ready:
			        work()
			    return done
			""";

		code.Render().Trim().ShouldBe(expected);
	}

	[Fact]
	public void CustomLanguageWarnsForMissingTraitNodes()
	{
		CodeBuilder<ScratchLanguage> code = new();
		code.File("scratch.scr").EndFile();
		code.Model!.Children.Add(new CodeImport { From = "./types" });

		code.Render().Trim().ShouldBeEmpty();
		code.Warnings.ShouldContain("Import './types' is not supported in Scratch; skipped.");
	}

	[Fact]
	public void ConstructorInjectionWarnsInLanguageWithoutConstructors()
	{
		CodeBuilder<ScratchLanguage> code = new();
		code.File("scratch.scr").EndFile();
		code.Model!.Children.Add(new CodeConstructor());

		code.Render().Trim().ShouldBeEmpty();
		code.Warnings.ShouldContain("Constructors are not supported in Scratch; skipped.");
	}

	[Fact]
	public void BuiltInLanguageTraitsMatchTheirProfiles()
	{
		LanguageProfile gd = LanguageProfile.Get<GDScript>();
		LanguageProfile ts = LanguageProfile.Get<TypeScript>();
		LanguageProfile cs = LanguageProfile.Get<CSharp>();

		gd.Supports(typeof(IHasSignals)).ShouldBeTrue();
		gd.Supports(typeof(IHasImports)).ShouldBeFalse();
		gd.Supports(typeof(IHasExceptionHandling)).ShouldBeFalse();

		ts.Supports(typeof(IHasSignals)).ShouldBeFalse();
		ts.Supports(typeof(IHasImports)).ShouldBeTrue();
		ts.Supports(typeof(IHasTopLevelFunctions)).ShouldBeTrue();

		cs.Supports(typeof(IHasSignals)).ShouldBeFalse();
		cs.Supports(typeof(IHasNamespaces)).ShouldBeTrue();
		cs.Supports(typeof(IHasSealed)).ShouldBeTrue();
	}
}
