using JetBrains.Annotations;
using Auxil.Common;

namespace Auxil.Tests.Common;

[TestSubject(typeof(Tags<>))]
public class TagsTest
{
    [Fact]
    public void Unique_AddReturnsFalseForDuplicateTag()
    {
        Tags<string> tags = Tags<string>.Unique();

        tags.Add("a").ShouldBeTrue();
        tags.Add("a").ShouldBeFalse();
        tags.Count("a").ShouldBe(1);
    }

    [Fact]
    public void Unique_ConsumeRemovesTag()
    {
        Tags<string> tags = Tags<string>.Unique();
        tags.Add("a");

        tags.Consume("a");

        tags.Has("a").ShouldBeFalse();
        tags.Count("a").ShouldBe(0);
    }

    [Fact]
    public void Unique_ConsumeMissingTagIsNoOp()
    {
        Tags<string> tags = Tags<string>.Unique();

        tags.Consume("a");

        tags.Has("a").ShouldBeFalse();
    }

    [Fact]
    public void Stackable_AddIncrementsCount()
    {
        Tags<string> tags = Tags<string>.Stackable();

        tags.Add("a").ShouldBeTrue();
        tags.Add("a").ShouldBeTrue();
        tags.Add("a").ShouldBeTrue();

        tags.Count("a").ShouldBe(3);
        tags.Has("a").ShouldBeTrue();
    }

    [Fact]
    public void Stackable_ConsumeDecrementsCount()
    {
        Tags<string> tags = Tags<string>.Stackable();
        tags.Add("a");
        tags.Add("a");

        tags.Consume("a");

        tags.Count("a").ShouldBe(1);
        tags.Has("a").ShouldBeTrue();
    }

    [Fact]
    public void Stackable_ConsumeToZeroRemovesTag()
    {
        Tags<string> tags = Tags<string>.Stackable();
        tags.Add("a");

        tags.Consume("a");
        tags.Consume("a");

        tags.Has("a").ShouldBeFalse();
        tags.Count("a").ShouldBe(0);
    }

    [Fact]
    public void Stackable_ConsumeBelowZeroDoesNotGoNegative()
    {
        Tags<string> tags = Tags<string>.Stackable();
        tags.Add("a");

        tags.Consume("a");
        tags.Consume("a");
        tags.Consume("a");

        tags.Has("a").ShouldBeFalse();
        tags.Count("a").ShouldBe(0);
    }

    [Fact]
    public void Remove_RemovesTag()
    {
        Tags<string> tags = Tags<string>.Unique();
        tags.Add("a");
        tags.Add("b");

        tags.Remove("a");

        tags.Has("a").ShouldBeFalse();
        tags.Has("b").ShouldBeTrue();
    }

    [Fact]
    public void Reset_ClearsAllTags()
    {
        Tags<string> tags = Tags<string>.Unique();
        tags.Add("a");
        tags.Add("b");

        tags.Reset();

        tags.Has("a").ShouldBeFalse();
        tags.Has("b").ShouldBeFalse();
    }

    [Fact]
    public void Constructor_DefaultIsUnique()
    {
        Tags<string> tags = new();

        tags.IsStackable.ShouldBeFalse();
        tags.Add("a").ShouldBeTrue();
        tags.Add("a").ShouldBeFalse();
    }

    [Fact]
    public void Factories_SetStackability()
    {
        Tags<string> unique = Tags<string>.Unique();
        Tags<string> stackable = Tags<string>.Stackable();

        unique.IsStackable.ShouldBeFalse();
        stackable.IsStackable.ShouldBeTrue();
    }

    [Fact]
    public void ThrowsOnNullTag()
    {
        Tags<string> tags = Tags<string>.Unique();

        Should.Throw<ArgumentNullException>(() => tags.Add(null!));
        Should.Throw<ArgumentNullException>(() => tags.Consume(null!));
        Should.Throw<ArgumentNullException>(() => tags.Count(null!));
        Should.Throw<ArgumentNullException>(() => tags.Remove(null!));
        Should.Throw<ArgumentNullException>(() => tags.Has(null!));
    }
}
