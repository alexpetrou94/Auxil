namespace Auxil.Common.Text;

/// <summary>Ranks words in a list by how closely they match a query.</summary>
[PublicAPI]
public class FuzzySearch
{
    /// <summary>The words to search.</summary>
    public List<string> SearchList { get; init; } = [];

    /// <summary>Creates a fuzzy search over an empty word list.</summary>
    public FuzzySearch()
    {
    }

    /// <summary>Creates a fuzzy search over the given words.</summary>
    /// <param name="searchList">The words to search.</param>
    public FuzzySearch(IEnumerable<string> searchList)
    {
        ArgumentNullException.ThrowIfNull(searchList);

        SearchList = searchList.ToList();
    }

    /// <summary>Returns the search words ranked by match quality, best first.</summary>
    /// <param name="text">The query text.</param>
    /// <param name="options">Matching options; defaults are used when <c>null</c>.</param>
    /// <returns>The ranked matches, or an empty list for empty or whitespace queries.</returns>
    public List<FuzzySearchResult> FindMatches(string text, FuzzySearchOptions? options = null)
    {
        if (text.IsNullOrWhiteSpace())
        {
            return [];
        }

        options ??= new FuzzySearchOptions();
        string processedText = ProcessWord(text, options);

        IEnumerable<FuzzySearchResult> scored = options.UseParallel
            ? SearchList.AsParallel().Select(searchWord => BuildResult(searchWord, processedText, options))
            : SearchList.Select(searchWord => BuildResult(searchWord, processedText, options));

        IEnumerable<FuzzySearchResult> ordered = scored.OrderByDescending(result => result.ScorePercentage);

        return options.MaxResults > 0
            ? ordered.Take(options.MaxResults).ToList()
            : ordered.ToList();
    }

    private static FuzzySearchResult BuildResult(string searchWord, string processedText, FuzzySearchOptions options)
    {
        int score = Score(processedText, ProcessWord(searchWord, options), options);
        int maxScore = options.WordStartScore + options.WordEndScore + options.LengthScore + searchWord.Length;

        return new FuzzySearchResult
        {
            Score = score,
            Text = searchWord,
            MaxScore = maxScore
        };
    }

    private static string ProcessWord(string text, FuzzySearchOptions options)
        => options.CaseInsensitive
            ? text.ToLower().Remove(options.IgnoreCharacters.ToArray())
            : text.Remove(options.IgnoreCharacters.ToArray());

    private static int Score(string textToSearch, string searchWord, FuzzySearchOptions options)
    {
        if (textToSearch.Length == 0 || searchWord.Length == 0)
        {
            return 0;
        }

        int score = 0;
        int lengthDifference = Math.Abs(textToSearch.Length - searchWord.Length);
        int lengthScore = Math.Max(0, options.LengthScore - lengthDifference);

        if (textToSearch.Length <= 3)
        {
            score += textToSearch[0] == searchWord[0] ? options.WordStartScore : 0;
            score += textToSearch[^1] == searchWord[^1] ? options.WordEndScore : 0;
        }

        else
        {
            int compareLength = Math.Min(2, searchWord.Length);

            score += textToSearch[..2] == searchWord[..compareLength] ? options.WordStartScore : 0;
            score += textToSearch[^2..] == searchWord[^compareLength..] ? options.WordEndScore : 0;
        }

        string lcs = textToSearch.LongestCommonSubsequence(searchWord);

        score += lengthScore;
        score += lcs.Length;

        return score;
    }
}
