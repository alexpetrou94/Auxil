using JetBrains.Annotations;
using Auxil.Extensions;

namespace Auxil.Tests.Extensions;

[TestSubject(typeof(DictionaryExtensions))]
public class DictionaryExtensionsTest
{
    [Fact]
    public void GetOrDefault_ReturnsExistingValue()
    {
        Dictionary<string, int> dictionary = new()
        {
            ["a"] = 1
        };

        dictionary.GetOrDefault("a", 99).ShouldBe(1);
    }

    [Fact]
    public void GetOrDefault_ReturnsDefaultForMissingKey()
    {
        Dictionary<string, int> dictionary = new()
        {
            ["a"] = 1
        };

        dictionary.GetOrDefault("b", 99).ShouldBe(99);
    }

    [Fact]
    public void GetOrDefault_SupportsNullDefault()
    {
        Dictionary<string, string?> dictionary = new()
        {
            ["a"] = "value"
        };

        dictionary.GetOrDefault("missing", null).ShouldBeNull();
    }

    [Fact]
    public void GetOrDefault_ThrowsOnNullArguments()
    {
        Dictionary<string, int>? nullDictionary = null;
        Dictionary<string, int> dictionary = new();

        Should.Throw<ArgumentNullException>(() => nullDictionary!.GetOrDefault("a", 0));
        Should.Throw<ArgumentNullException>(() => dictionary.GetOrDefault(null!, 0));
    }

    [Fact]
    public void Merge_KeepsReceiverValuesForDuplicateKeys()
    {
        Dictionary<string, int> receiver = new()
        {
            ["a"] = 1,
            ["b"] = 2
        };

        Dictionary<string, int> other = new()
        {
            ["b"] = 20,
            ["c"] = 3
        };

        IDictionary<string, int> merged = receiver.Merge(other);

        merged["a"].ShouldBe(1);
        merged["b"].ShouldBe(2);
        merged["c"].ShouldBe(3);
    }

    [Fact]
    public void Merge_OverrideKeyReplacesValues()
    {
        Dictionary<string, int> receiver = new()
        {
            ["a"] = 1,
            ["b"] = 2
        };

        Dictionary<string, int> other = new()
        {
            ["b"] = 20,
            ["c"] = 3
        };

        IDictionary<string, int> merged = receiver.Merge(other, overrideKey: true);

        merged["b"].ShouldBe(20);
        merged["c"].ShouldBe(3);
    }

    [Fact]
    public void Merge_DoesNotMutateReceiver()
    {
        Dictionary<string, int> receiver = new()
        {
            ["a"] = 1
        };

        Dictionary<string, int> other = new()
        {
            ["b"] = 2
        };

        receiver.Merge(other);

        receiver.ContainsKey("b").ShouldBeFalse();
    }

    [Fact]
    public void Merge_WithEmptyOtherReturnsCopy()
    {
        Dictionary<string, int> receiver = new()
        {
            ["a"] = 1
        };

        IDictionary<string, int> merged = receiver.Merge(new Dictionary<string, int>());

        merged.Count.ShouldBe(1);
        merged["a"].ShouldBe(1);
    }

    [Fact]
    public void Merge_ThrowsOnNullArguments()
    {
        Dictionary<string, int>? nullReceiver = null;
        Dictionary<string, int> receiver = new();
        Dictionary<string, int>? nullOther = null;

        Should.Throw<ArgumentNullException>(() => nullReceiver!.Merge(new Dictionary<string, int>()));
        Should.Throw<ArgumentNullException>(() => receiver.Merge(nullOther!));
    }
}
