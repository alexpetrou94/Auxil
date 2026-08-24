namespace Auxil.Common.Text;

/// <summary>A structured snapshot of a regular expression match.</summary>
[PublicAPI]
public sealed record RegexResult
{
    /// <summary>The index of the first character of the match.</summary>
    public int EndIndex { get; init; }
    /// <summary>The captured group values keyed by group name, including <c>"0"</c> for the whole match.</summary>
    public IReadOnlyDictionary<string, string> Groups { get; init; } = new Dictionary<string, string>();
    /// <summary>The input text that was searched.</summary>
    public string InputText { get; init; } = string.Empty;
    /// <summary>The regular expression pattern that produced the match.</summary>
    public string Pattern { get; init; } = string.Empty;
    /// <summary>The index of the first character of the match.</summary>
    public int StartIndex { get; init; }
    /// <summary>Whether a match was found.</summary>
    public bool Success { get; init; }

    /// <summary>Returns the whole match text from group <c>"0"</c>, or an empty string when absent.</summary>
    public string GetCompleteMatch() => Groups.TryGetValue("0", out string? completeMatch) ? completeMatch : string.Empty;

    /// <summary>Returns the value of the named group, or an empty string when absent.</summary>
    /// <param name="groupName">The group name.</param>
    public string GetString(string groupName)
        => Groups.TryGetValue(groupName, out string? groupString) ? groupString : string.Empty;

    /// <summary>Whether this result equals another, comparing the groups structurally.</summary>
    public bool Equals(RegexResult? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return EndIndex == other.EndIndex
            && StartIndex == other.StartIndex
            && Success == other.Success
            && InputText == other.InputText
            && Pattern == other.Pattern
            && Groups.Count == other.Groups.Count
            && Groups.All(pair => other.Groups.TryGetValue(pair.Key, out string? value) && value == pair.Value);
    }

    /// <summary>Returns a hash code consistent with the structural equality.</summary>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(EndIndex);
        hash.Add(StartIndex);
        hash.Add(Success);
        hash.Add(InputText);
        hash.Add(Pattern);

        foreach (KeyValuePair<string, string> pair in Groups.OrderBy(pair => pair.Key))
        {
            hash.Add(pair.Key);
            hash.Add(pair.Value);
        }

        return hash.ToHashCode();
    }

    /// <summary>Returns a multi-line summary of the match.</summary>
    public override string ToString() =>
        string.Join('\n',
            $"Success: {Success}",
            $"Pattern: {Pattern}",
            $"InputText: {InputText}",
            $"Groups: {Groups.PrintContents()}",
            $"StartIndex: {StartIndex}",
            $"EndIndex: {EndIndex}");
}
