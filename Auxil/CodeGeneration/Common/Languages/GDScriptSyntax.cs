using Auxil.CodeGeneration.Common.Syntax;

namespace Auxil.CodeGeneration.Common.Languages;

/// <summary>
/// GDScript language profile: indentation-based blocks, colon-terminated
/// headers, # comments, ## doc comments, no semicolons, no generics.
/// </summary>
[PublicAPI]
public static class GDScriptSyntax
{
    /// <summary>Creates the GDScript language profile.</summary>
    public static LanguageSyntax Create()
    {
        return new LanguageSyntax
        {
            BlockStyle = BlockStyle.Indentation,
            BlockClose = string.Empty,
            HeaderTerminator = ":",
            LineCommentPrefix = "#",
            DocComment = DocCommentTokens.LinePrefixStyle("## "),
            StatementTerminator = string.Empty,
            ParameterStyle = ParameterStyle.NameFirst,
            ReturnTypeStyle = ReturnTypeStyle.ArrowAfterParens,
            FileLayout = FileLayoutStyle.ScriptClass,
            ModifierKeywords = new Dictionary<CodeModifier, string>
            {
                [CodeModifier.Static] = "static",
                [CodeModifier.Const] = "const",
                [CodeModifier.Abstract] = "@abstract"
            },
            TypeMap = new Dictionary<string, string>
            {
                ["string"] = "String",
                ["int"] = "int",
                ["float"] = "float",
                ["double"] = "float",
                ["bool"] = "bool",
                ["void"] = "void",
                ["object"] = "Object",
                ["array"] = "Array",
                ["dictionary"] = "Dictionary",
                ["null"] = "null",
                ["dynamic"] = "Variant"
            },
            KeywordMethod = "func",
            KeywordConstructor = "_init",
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
}
