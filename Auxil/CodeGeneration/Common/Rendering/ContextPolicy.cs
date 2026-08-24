namespace Auxil.CodeGeneration.Common.Rendering;

/// <summary>
/// Context restrictions that are not language-level features: a node
/// kind a language accepts in one place but not another (e.g. enums in
/// TypeScript at file level but not inside classes). Keyed by language
/// name; absent entries mean the node is allowed in that context.
/// </summary>
internal static class ContextPolicy
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<Type, Func<CodeNode, string>>> FileLevel = new Dictionary<string, IReadOnlyDictionary<Type, Func<CodeNode, string>>>
    {
        ["C#"] = new Dictionary<Type, Func<CodeNode, string>>
        {
            [typeof(CodeField)] = n => $"Field '{((CodeField)n).Name}' at file level is not supported in C#; skipped. Use Field() inside a class.",
            [typeof(CodeProperty)] = n => $"Property '{((CodeProperty)n).Name}' at file level is not supported in C#; skipped.",
            [typeof(CodeMethod)] = n => $"Method '{((CodeMethod)n).Name}' at file level is not supported in C#; skipped. Use Method() inside a class.",
            [typeof(CodeConstructor)] = _ => "Constructor at file level is not supported in C#; skipped."
        },
        ["TypeScript"] = new Dictionary<Type, Func<CodeNode, string>>
        {
            [typeof(CodeMethod)] = n => $"Method '{((CodeMethod)n).Name}' at file level is not supported in TypeScript; skipped. Use Function() instead.",
            [typeof(CodeConstructor)] = _ => "Constructor at file level is not supported in TypeScript; skipped."
        }
    };

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<Type, Func<CodeNode, string>>> ClassMember = new Dictionary<string, IReadOnlyDictionary<Type, Func<CodeNode, string>>>
    {
        ["TypeScript"] = new Dictionary<Type, Func<CodeNode, string>>
        {
            [typeof(CodeEnum)] = n => $"Enum '{((CodeEnum)n).Name}' inside a class is not supported in TypeScript; skipped.",
            [typeof(CodeInterface)] = n => $"Interface '{((CodeInterface)n).Name}' inside a class is not supported in TypeScript; skipped.",
            [typeof(CodeFunction)] = n => $"Function '{((CodeFunction)n).Name}' inside a class is not supported in TypeScript; skipped. Use Method() instead.",
            [typeof(CodeTypeAlias)] = n => $"Type alias '{((CodeTypeAlias)n).Name}' inside a class is not supported in TypeScript; skipped."
        },
        ["C#"] = new Dictionary<Type, Func<CodeNode, string>>
        {
            [typeof(CodeFunction)] = n => $"Function '{((CodeFunction)n).Name}' inside a class is not supported in C#; skipped. Use Method() instead."
        }
    };

    public static string? ForbiddenIn(RenderContext ctx, CodeNode node, NodeContext context)
    {
        IReadOnlyDictionary<Type, Func<CodeNode, string>>? policy = context switch
        {
            NodeContext.FileLevel => FileLevel.GetValueOrDefault(ctx.LanguageName),
            NodeContext.ClassMember => ClassMember.GetValueOrDefault(ctx.LanguageName),
            _ => null
        };

        if (policy is null || !policy.TryGetValue(node.GetType(), out Func<CodeNode, string>? warning))
        {
            return null;
        }

        return warning(node);
    }
}
