namespace Auxil.Common;

/// <summary>Represents a collection of tags that can behave as a set or as a multiset.</summary>
/// <typeparam name="T">The tag type.</typeparam>
[PublicAPI]
public class Tags<T> where T : notnull
{
    private readonly Dictionary<T, int> _tagLookup = [];
    /// <summary>Whether duplicate tags accumulate a count instead of being rejected.</summary>
    public bool IsStackable { get; }

    /// <summary>Creates a tag collection with the given stackability.</summary>
    /// <param name="stackable">Whether duplicate tags accumulate a count.</param>
    public Tags(bool stackable = false)
    {
        IsStackable = stackable;
    }

    /// <summary>Creates a tag collection that rejects duplicate tags.</summary>
    public static Tags<T> Unique() => new(false);

    /// <summary>Creates a tag collection that counts duplicate tags.</summary>
    public static Tags<T> Stackable() => new(true);

    /// <summary>Adds a tag, returning whether the collection changed.</summary>
    /// <param name="tag">The tag to add.</param>
    public bool Add(T tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        if (!IsStackable)
        {
            return _tagLookup.TryAdd(tag, 1);
        }

        if (!Has(tag))
        {
            _tagLookup[tag] = 1;
            return true;
        }

        _tagLookup[tag]++;
        return true;
    }

    /// <summary>Consumes one occurrence of the tag, removing it when its count reaches zero.</summary>
    /// <param name="tag">The tag to consume.</param>
    public void Consume(T tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        if (!Has(tag))
        {
            return;
        }

        if (IsStackable)
        {
            _tagLookup[tag]--;

            if (_tagLookup[tag] <= 0)
            {
                Remove(tag);
            }

            return;
        }

        Remove(tag);
    }

    /// <summary>Returns how many occurrences of the tag are present.</summary>
    /// <param name="tag">The tag to count.</param>
    public int Count(T tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        _tagLookup.TryGetValue(tag, out int count);
        return count;
    }

    /// <summary>Removes all tags.</summary>
    public void Reset() => _tagLookup.Clear();

    /// <summary>Removes the tag entirely.</summary>
    /// <param name="tag">The tag to remove.</param>
    public void Remove(T tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        _tagLookup.Remove(tag);
    }

    /// <summary>Whether the tag is present.</summary>
    /// <param name="tag">The tag to check.</param>
    public bool Has(T tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        return _tagLookup.ContainsKey(tag);
    }
}
