namespace Auxil.Common.Text;

/// <summary>The result of a fuzzy match against a single search word.</summary>
[PublicAPI]
public sealed record FuzzySearchResult
{
    /// <summary>The raw match score.</summary>
    public int Score { get; init; }
    /// <summary>The maximum possible score for the matched word.</summary>
    public int MaxScore { get; init; }
    /// <summary>The matched word.</summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>The match score as a rounded percentage of the maximum possible score.</summary>
    public double ScorePercentage => MaxScore == 0 ? 0 : Math.Round((double)Score / MaxScore * 100);

    /// <summary>Returns the percentage and the matched word.</summary>
    public override string ToString() => $"({ScorePercentage}%) {Text}";
}
