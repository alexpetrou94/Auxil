using JetBrains.Annotations;
using Auxil.Extensions;

namespace Auxil.Tests.Extensions;

[TestSubject(typeof(ObjectExtensions))]
public class ObjectExtensionsTest
{
    [Fact]
    public void DeepClone_CopiesValueIndependently()
    {
        List<int> original = [1, 2, 3];

        List<int> clone = original.DeepClone();

        clone.ShouldBe(original);
        clone.ShouldNotBeSameAs(original);
    }

    [Fact]
    public void DeepClone_CopiesNestedObjectsIndependently()
    {
        Cloneable original = new()
        {
            Value = "original",
            Items = [1, 2]
        };

        Cloneable clone = original.DeepClone();

        clone.Value.ShouldBe("original");
        clone.Items.ShouldBe(new[] { 1, 2 });
        clone.Items.ShouldNotBeSameAs(original.Items);
    }

    [Fact]
    public void DeepClone_NullStaysNull()
    {
        string? value = null;
        string? clone = value.DeepClone();

        clone.ShouldBeNull();
    }

    [Fact]
    public void ShallowClone_CopiesValue()
    {
        Cloneable original = new()
        {
            Value = "original",
            Items = [1, 2]
        };

        Cloneable clone = original.ShallowClone();

        clone.Value.ShouldBe("original");
        clone.ShouldNotBeSameAs(original);
    }

    private sealed class Cloneable
    {
        public string Value { get; init; } = string.Empty;
        public List<int> Items { get; init; } = [];
    }
}
