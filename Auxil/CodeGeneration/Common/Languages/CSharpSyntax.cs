using Auxil.CodeGeneration.Common.Syntax;

namespace Auxil.CodeGeneration.Common.Languages;

/// <summary>
/// C# language profile: brace blocks, // comments, /// XML doc comments,
/// semicolons, type-first parameters, return-type-before-name methods,
/// namespaces, where-clause generics, full modifier support.
/// </summary>
[PublicAPI]
public static class CSharpSyntax
{
    /// <summary>Creates the C# language profile.</summary>
    public static LanguageSyntax Create()
    {
        return new LanguageSyntax
        {
            BlockStyle = BlockStyle.Braces,
            BlockOpen = " {",
            BlockClose = "}",
            LineCommentPrefix = "//",
            DocComment = DocCommentTokens.LinePrefixStyle("/// "),
            StatementTerminator = ";",
            ParameterStyle = ParameterStyle.TypeFirst,
            ReturnTypeStyle = ReturnTypeStyle.BeforeName,
            FileLayout = FileLayoutStyle.Module,
            ModifierKeywords = new Dictionary<CodeModifier, string>
            {
                [CodeModifier.Public] = "public",
                [CodeModifier.Private] = "private",
                [CodeModifier.Protected] = "protected",
                [CodeModifier.Internal] = "internal",
                [CodeModifier.Static] = "static",
                [CodeModifier.Abstract] = "abstract",
                [CodeModifier.Virtual] = "virtual",
                [CodeModifier.Override] = "override",
                [CodeModifier.Sealed] = "sealed",
                [CodeModifier.Readonly] = "readonly",
                [CodeModifier.Const] = "const",
                [CodeModifier.Async] = "async"
            },
            TypeMap = new Dictionary<string, string>
            {
                ["string"] = "string",
                ["int"] = "int",
                ["float"] = "float",
                ["double"] = "double",
                ["bool"] = "bool",
                ["void"] = "void",
                ["object"] = "object",
                ["dynamic"] = "dynamic"
            },
            KeywordExtends = string.Empty,
            KeywordConstructor = string.Empty,
            KeywordFunction = string.Empty,
            BaseListStyle = BaseListStyle.Colon,
            GenericStyle = GenericStyle.WhereClauses,
            AnnotationStyle = AnnotationStyle.Square,
            PropertyStyle = PropertyStyle.AutoAccessors,
            ImportStyle = ImportStyle.Using,
            BraceOnOwnLine = true,
            ForEachKeyword = "foreach",
            ForEachVariable = "var",
            ForEachConnector = "in",
            LocalVarKeyword = "var",
            EnumTrailingComma = false
        };
    }
}
