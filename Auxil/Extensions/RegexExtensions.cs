using System.Text.RegularExpressions;
using Auxil.Common.Text;

namespace Auxil.Extensions;

/// <summary>Provides convenience members for working with regular expressions.</summary>
[PublicAPI]
public static class RegexExtensions
{
    private const string MatchedStringKey = "0";

    /// <summary>Runs the regex and returns a structured result with the named groups captured.</summary>
    /// <param name="regex">The regular expression.</param>
    /// <param name="input">The input text.</param>
    public static RegexResult BetterMatch(this Regex regex, string input)
    {
        ArgumentNullException.ThrowIfNull(regex);
        ArgumentNullException.ThrowIfNull(input);

        return BuildRegexResult(regex.Match(input), input, regex);
    }

    /// <summary>Runs the regex over all matches and returns structured results with named groups captured.</summary>
    /// <param name="regex">The regular expression.</param>
    /// <param name="input">The input text.</param>
    public static List<RegexResult> BetterMatches(this Regex regex, string input)
    {
        ArgumentNullException.ThrowIfNull(regex);
        ArgumentNullException.ThrowIfNull(input);

        return regex.Matches(input)
            .Select(match => BuildRegexResult(match, input, regex))
            .ToList();
    }

    private static RegexResult BuildRegexResult(Match match, string input, Regex regex)
    {
        if (!match.Success)
        {
            return new RegexResult
            {
                InputText = input,
                Pattern = regex.ToString()
            };
        }

        Dictionary<string, string> namedCaptures = [];
        int startIndex = 0;
        int endIndex = 0;

        foreach (string name in regex.GetGroupNames())
        {
            Group group = match.Groups[name];

            if (name == "0")
            {
                namedCaptures.Add(MatchedStringKey, group.Value);
                startIndex = group.Index;
                endIndex = group.Index + group.Length;
                continue;
            }

            namedCaptures.Add(name, group.Value);
        }

        return new RegexResult
        {
            InputText = input,
            Pattern = regex.ToString(),
            Groups = namedCaptures,
            Success = true,
            StartIndex = startIndex,
            EndIndex = endIndex
        };
    }
}
