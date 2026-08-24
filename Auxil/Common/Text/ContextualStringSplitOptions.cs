namespace Auxil.Common.Text;

/// <summary>Options controlling how <c>ContextualSplit</c> breaks text into words.</summary>
[PublicAPI]
public class ContextualStringSplitOptions
{
    /// <summary>Drops empty entries from the result.</summary>
    public bool RemoveEmptyEntries { get; set; }
    /// <summary>Trims whitespace from each entry.</summary>
    public bool TrimEntries { get; set; }
    /// <summary>Splits acronym runs (consecutive uppercase letters) into their own entries.</summary>
    public bool IncludeAcronyms { get; set; }
    /// <summary>Splits number runs into their own entries.</summary>
    public bool IncludeNumbers { get; set; }
}
