
namespace Auxil.CodeGeneration.Common.Syntax;

/// <summary>
/// A language profile: every syntactic detail that differs between
/// languages lives here so the shared renderer stays generic. Which
/// features a language supports at all is declared by its trait list
/// (see <see cref="Languages.ILanguage"/>); this profile only carries
/// tokens and styles.
/// </summary>
[PublicAPI]
public sealed class LanguageSyntax
{
    /// <summary>How the language delimits blocks.</summary>
    public required BlockStyle BlockStyle { get; init; }

    /// <summary>Opening token for brace blocks, e.g. " {" for TypeScript.</summary>
    public string BlockOpen { get; init; } = " {";

    /// <summary>Closing token for brace blocks, e.g. "}" for TypeScript.</summary>
    public string BlockClose { get; init; } = "}";

    /// <summary>Suffix appended to block headers in indentation languages, e.g. ":" for GDScript.</summary>
    public string HeaderTerminator { get; init; } = string.Empty;

    /// <summary>Line comment prefix, e.g. "//" (TypeScript) or "#" (GDScript).</summary>
    public required string LineCommentPrefix { get; init; }

    /// <summary>Tokens used to render documentation comments.</summary>
    public required DocCommentTokens DocComment { get; init; }

    /// <summary>End-of-statement token, e.g. ";" (TypeScript) or "" (GDScript).</summary>
    public required string StatementTerminator { get; init; }

    /// <summary>How parameters are written.</summary>
    public required ParameterStyle ParameterStyle { get; init; }

    /// <summary>How return types are written.</summary>
    public required ReturnTypeStyle ReturnTypeStyle { get; init; }

    /// <summary>How the file lays out its content.</summary>
    public required FileLayoutStyle FileLayout { get; init; }

    /// <summary>Modifier flags that this language knows, mapped to their keyword.</summary>
    public required IReadOnlyDictionary<CodeModifier, string> ModifierKeywords { get; init; }

    /// <summary>Canonical type name to language-specific type name.</summary>
    public required IReadOnlyDictionary<string, string> TypeMap { get; init; }

    // ---- Declaration keywords ----

    /// <summary>The class keyword.</summary>
    public string KeywordClass { get; init; } = "class";

    /// <summary>The interface keyword.</summary>
    public string KeywordInterface { get; init; } = "interface";

    /// <summary>The enum keyword.</summary>
    public string KeywordEnum { get; init; } = "enum";

    /// <summary>The top-level function keyword (TypeScript "function"); empty when the language has no top-level functions.</summary>
    public string KeywordFunction { get; init; } = "function";

    /// <summary>The method keyword (GDScript "func"); empty when the signature stands bare.</summary>
    public string KeywordMethod { get; init; } = string.Empty;

    /// <summary>The constructor keyword (TypeScript "constructor", GDScript "_init"); empty when the class name is used.</summary>
    public string KeywordConstructor { get; init; } = "constructor";

    /// <summary>The signal keyword.</summary>
    public string KeywordSignal { get; init; } = "signal";

    /// <summary>The type-alias keyword (TypeScript "type").</summary>
    public string KeywordTypeAlias { get; init; } = "type";

    /// <summary>The match/switch keyword.</summary>
    public string KeywordMatch { get; init; } = "switch";

    /// <summary>The empty-block placeholder (GDScript "pass"); empty when empty blocks are left empty.</summary>
    public string KeywordPass { get; init; } = string.Empty;

    /// <summary>The base-class keyword; empty with Colon base lists.</summary>
    public string KeywordExtends { get; init; } = "extends";

    /// <summary>The implements keyword; empty when the language has no implements lists.</summary>
    public string KeywordImplements { get; init; } = "implements";

    /// <summary>The default arm pattern (TypeScript/C# "default:", GDScript "_").</summary>
    public string KeywordDefaultArm { get; init; } = "default:";

    /// <summary>The else-if keyword (GDScript "elif", others "else if").</summary>
    public string KeywordElseIf { get; init; } = "else if";

    /// <summary>The keyword before a class-member field (GDScript "var"); empty when fields stand bare.</summary>
    public string FieldKeyword { get; init; } = string.Empty;

    /// <summary>The keyword before a file-level field (TypeScript "let", GDScript "var"); empty when fields stand bare.</summary>
    public string TopLevelFieldKeyword { get; init; } = string.Empty;

    /// <summary>
    /// Whether the language places opening braces on their own line by
    /// default and honors the BraceStyle option (C# Allman). Brace-based
    /// languages without this token always use K&amp;R placement.
    /// </summary>
    public bool BraceOnOwnLine { get; init; }

    /// <summary>Whether the language has nullable markers (C# "T?", TypeScript "name?").</summary>
    public bool SupportsNullable { get; init; } = true;

    // ---- Declaration styles ----

    /// <summary>How base lists are written.</summary>
    public BaseListStyle BaseListStyle { get; init; } = BaseListStyle.Extends;

    /// <summary>How generic type parameters are written.</summary>
    public GenericStyle GenericStyle { get; init; } = GenericStyle.Inline;

    /// <summary>How annotations are written.</summary>
    public AnnotationStyle AnnotationStyle { get; init; } = AnnotationStyle.At;

    /// <summary>How match arms are written.</summary>
    public MatchArmStyle MatchArmStyle { get; init; } = MatchArmStyle.CaseWithBreak;

    /// <summary>How properties are written.</summary>
    public PropertyStyle PropertyStyle { get; init; } = PropertyStyle.AsField;

    /// <summary>How imports are written.</summary>
    public ImportStyle ImportStyle { get; init; } = ImportStyle.From;

    // ---- Statement skeletons ----

    /// <summary>Whether conditions are parenthesized, e.g. "if (x)" vs GDScript "if x:".</summary>
    public bool ConditionParens { get; init; } = true;

    /// <summary>The foreach keyword (TypeScript/GDScript "for", C# "foreach").</summary>
    public string ForEachKeyword { get; init; } = "for";

    /// <summary>The foreach loop-variable declarator (TypeScript "const", C# "var"); empty when none is written.</summary>
    public string ForEachVariable { get; init; } = "const";

    /// <summary>The foreach connector (TypeScript "of", others "in").</summary>
    public string ForEachConnector { get; init; } = "of";

    /// <summary>The local-variable keyword (TypeScript "let", GDScript/C# "var").</summary>
    public string LocalVarKeyword { get; init; } = "let";

    /// <summary>The constant keyword for locals and declarations.</summary>
    public string ConstKeyword { get; init; } = "const";

    /// <summary>Suffix after a closing enum brace (TypeScript ";").</summary>
    public string EnumCloseSuffix { get; init; } = string.Empty;

    /// <summary>Whether enum members get a trailing comma.</summary>
    public bool EnumTrailingComma { get; init; }

    /// <summary>
    /// Maps a canonical type name to this language's name.
    /// Unknown names (custom classes, engine types) pass through unchanged.
    /// </summary>
    public string MapType(CodeType type)
    {
        return TypeMap.TryGetValue(type.Name, out string? mapped) ? mapped : type.Name;
    }

    /// <summary>Whether this language renders the given modifier.</summary>
    public bool IsModifierSupported(CodeModifier modifier)
    {
        return ModifierKeywords.ContainsKey(modifier);
    }

    /// <summary>Keyword for a supported modifier, or null when unsupported.</summary>
    public string? GetModifierKeyword(CodeModifier modifier)
    {
        return ModifierKeywords.GetValueOrDefault(modifier);
    }

    /// <summary>
    /// Builds the modifier prefix for a declaration, in enum order,
    /// skipping the const modifier (rendered as the declaration keyword).
    /// </summary>
    public List<string> BuildModifierPrefix(CodeModifier modifiers)
    {
        List<string> result = [];

        foreach (CodeModifier modifier in Enum.GetValues<CodeModifier>())
        {
            if (modifier is CodeModifier.None or CodeModifier.Const)
            {
                continue;
            }

            if (modifiers.HasFlag(modifier) && GetModifierKeyword(modifier) is { } keyword)
            {
                result.Add(keyword);
            }
        }

        return result;
    }
}
