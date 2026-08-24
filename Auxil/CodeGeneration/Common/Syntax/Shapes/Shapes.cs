namespace Auxil.CodeGeneration.Common.Syntax.Shapes;

/// <summary>
/// A language's shape bundle: one hook per construct. Every hook
/// defaults to the shared implementation (driven by the language's
/// syntax profile tokens); a language overrides only the hooks it wants
/// to look different. The bundle is exposed as a static virtual member
/// on <see cref="Languages.ILanguage"/>, so a language decides how it
/// looks, outside the default.
/// </summary>
[PublicAPI]
public sealed class Shapes
{
    /// <summary>Shapes field declarations.</summary>
    public FieldShape Field { get; init; } = DefaultShapes.Field;

    /// <summary>Shapes method/constructor/function signatures.</summary>
    public SignatureShape Signature { get; init; } = DefaultShapes.Signature;

    /// <summary>Shapes property declarations.</summary>
    public PropertyShape Property { get; init; } = DefaultShapes.Property;

    /// <summary>Shapes class declaration headers.</summary>
    public ClassHeaderShape ClassHeader { get; init; } = DefaultShapes.ClassHeader;

    /// <summary>Shapes interface declaration headers.</summary>
    public InterfaceHeaderShape InterfaceHeader { get; init; } = DefaultShapes.InterfaceHeader;

    /// <summary>Shapes enum declarations.</summary>
    public EnumShape Enum { get; init; } = DefaultShapes.Enum;

    /// <summary>Shapes if/else-if/else statements.</summary>
    public IfShape If { get; init; } = DefaultShapes.If;

    /// <summary>Shapes foreach statements.</summary>
    public ForEachShape ForEach { get; init; } = DefaultShapes.ForEach;

    /// <summary>Shapes while statements.</summary>
    public WhileShape While { get; init; } = DefaultShapes.While;

    /// <summary>Shapes C-style for statements.</summary>
    public ForShape For { get; init; } = DefaultShapes.For;

    /// <summary>Shapes do/while statements.</summary>
    public DoWhileShape DoWhile { get; init; } = DefaultShapes.DoWhile;

    /// <summary>Shapes match/switch statements.</summary>
    public MatchShape Match { get; init; } = DefaultShapes.Match;

    /// <summary>Shapes try/catch/finally statements.</summary>
    public TryShape Try { get; init; } = DefaultShapes.Try;

    /// <summary>Shapes raw block statements.</summary>
    public RawBlockShape RawBlock { get; init; } = DefaultShapes.RawBlock;

    /// <summary>Shapes local variable declarations.</summary>
    public LocalVariableShape LocalVariable { get; init; } = DefaultShapes.LocalVariable;

    /// <summary>Shapes object literal statements.</summary>
    public ObjectLiteralShape ObjectLiteral { get; init; } = DefaultShapes.ObjectLiteral;

    /// <summary>The bundle of shared default shapes.</summary>
    public static Shapes Default { get; } = new();
}
