using JetBrains.Annotations;
using Auxil.Extensions;

namespace Auxil.Tests.Extensions;

[TestSubject(typeof(ArrayExtensions))]
public class ArrayExtensionsTest
{
    [Fact]
    public void Add_SingleElement()
    {
        int[] array = [1, 2];

        array.Add(3).ShouldBe([1, 2, 3]);
    }

    [Fact]
    public void Add_DoesNotMutateOriginal()
    {
        int[] array = [1, 2];

        array.Add(3);

        array.ShouldBe([1, 2]);
    }

    [Fact]
    public void AddRange_CombinesArrays()
    {
        int[] array = [1, 2];
        int[] other = [3, 4];

        array.AddRange(other).ShouldBe([1, 2, 3, 4]);
    }
}
