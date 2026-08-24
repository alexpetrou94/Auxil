namespace Auxil.CodeGeneration.Common.Syntax.Shapes;

/// <summary>Shapes a field declaration; returns the declaration text without modifiers or terminator.</summary>
[PublicAPI]
public delegate string FieldShape(
    ShapeContext ctx, string name, CodeType? type, bool nullable, string? defaultValue, bool isConst, string keyword, bool isInterfaceMember);

/// <summary>Which declaration kind a signature belongs to.</summary>
[PublicAPI]
public enum SignatureKind
{
    /// <summary>A method (class member).</summary>
    Method,

    /// <summary>A constructor.</summary>
    Constructor,

    /// <summary>A top-level function.</summary>
    Function
}

/// <summary>Shapes a method/constructor/function signature; returns the signature text without modifiers or terminator.</summary>
[PublicAPI]
public delegate string SignatureShape(
    ShapeContext ctx, SignatureKind kind, string name, List<CodeParameter> parameters, List<CodeGenericParameter> generics, CodeType? returnType, string className, bool interfaceMode);

/// <summary>Shapes a property declaration, writing it (including accessor bodies).</summary>
[PublicAPI]
public delegate void PropertyShape(ShapeContext ctx, CodeProperty property, string prefix, bool isTopLevel, bool isInterfaceMember);

/// <summary>Shapes a class declaration header; returns the header text without modifiers.</summary>
[PublicAPI]
public delegate string ClassHeaderShape(
    ShapeContext ctx, string name, List<CodeGenericParameter> generics, string? extendsType, List<string> implementsTypes);

/// <summary>Shapes an interface declaration header; returns the header text without modifiers.</summary>
[PublicAPI]
public delegate string InterfaceHeaderShape(ShapeContext ctx, string name, List<CodeGenericParameter> generics, List<string> extendsTypes);

/// <summary>Shapes an enum declaration, writing it; headerPrefix is the joined modifier prefix.</summary>
[PublicAPI]
public delegate void EnumShape(ShapeContext ctx, CodeEnum enm, string headerPrefix);

/// <summary>Shapes an if/else-if/else statement, writing it.</summary>
[PublicAPI]
public delegate void IfShape(ShapeContext ctx, CodeIf statement);

/// <summary>Shapes a foreach statement, writing it.</summary>
[PublicAPI]
public delegate void ForEachShape(ShapeContext ctx, string variable, string iterable, List<CodeStatement> body);

/// <summary>Shapes a while statement, writing it.</summary>
[PublicAPI]
public delegate void WhileShape(ShapeContext ctx, string condition, List<CodeStatement> body);

/// <summary>Shapes a C-style for statement, writing it.</summary>
[PublicAPI]
public delegate void ForShape(ShapeContext ctx, string initializer, string condition, string increment, List<CodeStatement> body);

/// <summary>Shapes a do/while statement, writing it.</summary>
[PublicAPI]
public delegate void DoWhileShape(ShapeContext ctx, string condition, List<CodeStatement> body);

/// <summary>Shapes a match/switch statement, writing it.</summary>
[PublicAPI]
public delegate void MatchShape(ShapeContext ctx, CodeMatch statement);

/// <summary>Shapes a try/catch/finally statement, writing it.</summary>
[PublicAPI]
public delegate void TryShape(ShapeContext ctx, CodeTry statement);

/// <summary>Shapes a raw block statement, writing it.</summary>
[PublicAPI]
public delegate void RawBlockShape(ShapeContext ctx, string header, List<CodeStatement> body, string? closeSuffix);

/// <summary>Shapes a local variable declaration; returns the declaration text or null to skip.</summary>
[PublicAPI]
public delegate string? LocalVariableShape(ShapeContext ctx, CodeLocalVariable variable);

/// <summary>Shapes an object literal statement, writing it (members comma-separated, terminator on the closing brace).</summary>
[PublicAPI]
public delegate void ObjectLiteralShape(ShapeContext ctx, CodeObjectLiteral literal);
