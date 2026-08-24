using Auxil.CodeGeneration.Common.Syntax;

namespace Auxil.CodeGeneration.Common.Languages;

/// <summary>
/// TypeScript language profile: brace blocks, // comments,
/// /** */ doc comments, semicolons, full modifier and generics support.
/// </summary>
[PublicAPI]
public static class TypeScriptSyntax
{
    /// <summary>Creates the TypeScript language profile.</summary>
    public static LanguageSyntax Create()
    {
        return new LanguageSyntax
        {
            BlockStyle = BlockStyle.Braces,
            BlockOpen = " {",
            BlockClose = "}",
            LineCommentPrefix = "//",
            DocComment = DocCommentTokens.JavadocStyle,
            StatementTerminator = ";",
            ParameterStyle = ParameterStyle.NameFirst,
            ReturnTypeStyle = ReturnTypeStyle.ColonAfterParens,
            FileLayout = FileLayoutStyle.Module,
            ModifierKeywords = new Dictionary<CodeModifier, string>
            {
                [CodeModifier.Public] = "public",
                [CodeModifier.Private] = "private",
                [CodeModifier.Protected] = "protected",
                [CodeModifier.Static] = "static",
                [CodeModifier.Abstract] = "abstract",
                [CodeModifier.Readonly] = "readonly",
                [CodeModifier.Async] = "async",
                [CodeModifier.Export] = "export",
                [CodeModifier.Override] = "override"
            },
            TypeMap = new Dictionary<string, string>
            {
                ["string"] = "string",
                ["int"] = "number",
                ["float"] = "number",
                ["double"] = "number",
                ["bool"] = "boolean",
                ["void"] = "void",
                ["object"] = "object",
                ["null"] = "null",
                ["dynamic"] = "any"
            },
            KeywordConstructor = "constructor",
            KeywordFunction = "function",
            TopLevelFieldKeyword = "let",
            ForEachKeyword = "for",
            ForEachVariable = "const",
            ForEachConnector = "of",
            LocalVarKeyword = "let",
            EnumCloseSuffix = ";",
            EnumTrailingComma = false
        };
    }
}
