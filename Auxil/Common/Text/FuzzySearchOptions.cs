namespace Auxil.Common.Text;

/// <summary>Options controlling fuzzy text matching.</summary>
[PublicAPI]
public record FuzzySearchOptions
{
    /// <summary>Score awarded when the word starts like the query.</summary>
    public int WordStartScore { get; init; } = 2;
    /// <summary>Score awarded when the word ends like the query.</summary>
    public int WordEndScore { get; init; } = 2;
    /// <summary>Maximum score contribution based on length similarity.</summary>
    public int LengthScore { get; init; } = 1;
    /// <summary>Ignores case when matching.</summary>
    public bool CaseInsensitive { get; init; } = true;
    /// <summary>Maximum number of results to return; <c>0</c> returns all.</summary>
    public int MaxResults { get; init; } = 1000;
    /// <summary>Runs the scoring pass in parallel using PLINQ.</summary>
    public bool UseParallel { get; init; }
    /// <summary>Characters stripped from both the query and the search words before matching.</summary>
    public IReadOnlyList<char> IgnoreCharacters { get; init; } = [' ', '\t', '\r', '\n'];
}
