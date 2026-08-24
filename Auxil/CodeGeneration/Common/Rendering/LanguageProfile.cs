using Auxil.CodeGeneration.Common.Languages;
using Auxil.CodeGeneration.Common.Syntax;
using Auxil.CodeGeneration.Common.Syntax.Shapes;

namespace Auxil.CodeGeneration.Common.Rendering;

/// <summary>
/// A composed language: the syntax profile plus the trait set derived
/// from the language type's interface list. Capability checks during
/// rendering consult the traits, so a language can only render what its
/// feature list declares.
/// </summary>
[PublicAPI]
public sealed class LanguageProfile
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, LanguageProfile> Cache = new();

    private LanguageProfile(Type languageType, string name, LanguageSyntax syntax, Shapes shapes, IReadOnlySet<Type> traits)
    {
        LanguageType = languageType;
        Name = name;
        Syntax = syntax;
        Shapes = shapes;
        Traits = traits;
    }

    /// <summary>The language type.</summary>
    public Type LanguageType { get; }

    /// <summary>The language name, used in warnings.</summary>
    public string Name { get; }

    /// <summary>The language's syntax profile.</summary>
    public LanguageSyntax Syntax { get; }

    /// <summary>The language's shape bundle.</summary>
    public Shapes Shapes { get; }

    /// <summary>The feature traits the language implements.</summary>
    public IReadOnlySet<Type> Traits { get; }

    /// <summary>Whether the language supports the given feature trait.</summary>
    public bool Supports(Type trait)
    {
        return Traits.Contains(trait);
    }

    /// <summary>Gets (and caches) the profile for a language type.</summary>
    public static LanguageProfile Get<TLanguage>()
        where TLanguage : struct, ILanguage
    {
        return Cache.GetOrAdd(typeof(TLanguage), static _ => Create<TLanguage>());
    }

    private static LanguageProfile Create<TLanguage>()
        where TLanguage : struct, ILanguage
    {
        Type languageType = typeof(TLanguage);
        IReadOnlySet<Type> traits = languageType.GetInterfaces()
            .Where(i => typeof(ILanguageFeature).IsAssignableFrom(i))
            .ToHashSet();
        return new LanguageProfile(languageType, TLanguage.Name, TLanguage.Syntax, TLanguage.Shapes, traits);
    }
}
