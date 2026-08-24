namespace Auxil.CodeGeneration.Common.Languages;

/// <summary>
/// Feature trait interfaces. A language struct implements the traits for
/// the features it supports; builder methods are gated on these traits,
/// so unsupported features are compile errors. The renderer derives its
/// capability list from the same traits.
/// </summary>
[PublicAPI]
public interface ILanguageFeature;

/// <summary>GDScript's file-as-class identity (class_name/extends).</summary>
[PublicAPI]
public interface IHasScriptClass : ILanguageFeature;

/// <summary>Signal declarations (GDScript signal).</summary>
[PublicAPI]
public interface IHasSignals : ILanguageFeature;

/// <summary>Named type alias declarations (TypeScript "type Name = ...").</summary>
[PublicAPI]
public interface IHasTypeAliases : ILanguageFeature;

/// <summary>Function declarations inside bodies (TypeScript nested functions, C# local functions).</summary>
[PublicAPI]
public interface IHasNestedFunctions : ILanguageFeature;

/// <summary>Object literal statements with comma-separated members (TypeScript).</summary>
[PublicAPI]
public interface IHasObjectLiterals : ILanguageFeature;

/// <summary>Import/using directives.</summary>
[PublicAPI]
public interface IHasImports : ILanguageFeature;

/// <summary>Namespace declarations.</summary>
[PublicAPI]
public interface IHasNamespaces : ILanguageFeature;

/// <summary>Interface declarations.</summary>
[PublicAPI]
public interface IHasInterfaces : ILanguageFeature;

/// <summary>Top-level functions.</summary>
[PublicAPI]
public interface IHasTopLevelFunctions : ILanguageFeature;

/// <summary>Nested class declarations.</summary>
[PublicAPI]
public interface IHasInnerClasses : ILanguageFeature;

/// <summary>Generic type parameters.</summary>
[PublicAPI]
public interface IHasGenerics : ILanguageFeature;

/// <summary>Constructor declarations.</summary>
[PublicAPI]
public interface IHasConstructors : ILanguageFeature;

/// <summary>Interface implementation lists (implements).</summary>
[PublicAPI]
public interface IHasImplements : ILanguageFeature;

/// <summary>Exception handling (try/catch/finally).</summary>
[PublicAPI]
public interface IHasExceptionHandling : ILanguageFeature;

/// <summary>Do/while loops.</summary>
[PublicAPI]
public interface IHasDoWhile : ILanguageFeature;

/// <summary>C-style for loops.</summary>
[PublicAPI]
public interface IHasCStyleFor : ILanguageFeature;

/// <summary>Throw statements.</summary>
[PublicAPI]
public interface IHasThrow : ILanguageFeature;

/// <summary>Match/switch statements.</summary>
[PublicAPI]
public interface IHasMatch : ILanguageFeature;

/// <summary>Visibility modifiers (public/private/protected/internal).</summary>
[PublicAPI]
public interface IHasVisibilityModifiers : ILanguageFeature;

/// <summary>The sealed modifier.</summary>
[PublicAPI]
public interface IHasSealed : ILanguageFeature;

/// <summary>The virtual modifier.</summary>
[PublicAPI]
public interface IHasVirtual : ILanguageFeature;

/// <summary>The override modifier.</summary>
[PublicAPI]
public interface IHasOverride : ILanguageFeature;

/// <summary>The readonly modifier.</summary>
[PublicAPI]
public interface IHasReadonly : ILanguageFeature;

/// <summary>The const modifier.</summary>
[PublicAPI]
public interface IHasConst : ILanguageFeature;

/// <summary>The async modifier.</summary>
[PublicAPI]
public interface IHasAsync : ILanguageFeature;

/// <summary>The module export modifier (TypeScript export).</summary>
[PublicAPI]
public interface IHasExport : ILanguageFeature;
