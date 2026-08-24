namespace Auxil.CodeGeneration.Common.Syntax.Shapes;

/// <summary>
/// The context every shape hook receives: the writer, the language's
/// syntax profile, the warning sink, and the flow's statement renderer.
/// The statement renderer is injected so shape implementations can
/// render nested bodies without depending on the rendering layer.
/// </summary>
[PublicAPI]
public sealed class ShapeContext
{
    /// <summary>The writer receiving the rendered source.</summary>
    public required CodeWriter Writer { get; init; }

    /// <summary>The language's syntax profile.</summary>
    public required LanguageSyntax Syntax { get; init; }

    /// <summary>The language's shape bundle, so shape defaults can compose other hooks.</summary>
    public required Shapes Shapes { get; init; }

    /// <summary>The language name, used in warnings.</summary>
    public required string LanguageName { get; init; }

    /// <summary>Warnings collected during rendering.</summary>
    public required List<string> Warnings { get; init; }

    /// <summary>Renders a block of statements through the shared flow (including pass handling).</summary>
    public required Action<List<CodeStatement>> RenderStatements { get; init; }
}
