namespace Auxil.CodeGeneration.Common.Syntax;

/// <summary>
/// Where and how a function/method return type is written.
/// </summary>
[PublicAPI]
public enum ReturnTypeStyle
{
    /// <summary>TypeScript: foo(): string</summary>
    ColonAfterParens,

    /// <summary>GDScript: func foo() -&gt; string:</summary>
    ArrowAfterParens,

    /// <summary>C#/Java: string Foo()</summary>
    BeforeName
}
