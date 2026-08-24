using System.Collections;
using JetBrains.Annotations;
using Auxil.Extensions;

namespace Auxil.Tests.Extensions;

[TestSubject(typeof(ListExtensions))]
public class ListExtensionsTest
{
    [Fact]
    public void GetAndRemove_RemovesTheItemAtTheGivenIndex()
    {
        List<int> list = [1, 2, 3];

        int removed = list.GetAndRemove(1);

        removed.ShouldBe(2);
        list.ShouldBe(new[] { 1, 3 });
    }

    [Fact]
    public void GetAndRemove_RemovesFirstItem()
    {
        List<int> list = [1, 2, 3];

        list.GetAndRemove(0).ShouldBe(1);
        list.ShouldBe(new[] { 2, 3 });
    }

    [Fact]
    public void GetAndRemove_RemovesLastItem()
    {
        List<int> list = [1, 2, 3];

        list.GetAndRemove(2).ShouldBe(3);
        list.ShouldBe(new[] { 1, 2 });
    }

    [Fact]
    public void GetAndRemove_ThrowsForNegativeIndex()
    {
        List<int> list = [1, 2, 3];

        Should.Throw<ArgumentOutOfRangeException>(() => list.GetAndRemove(-1));
    }

    [Fact]
    public void RemoveIfExists_RemovesValidIndex()
    {
        IList list = new List<int> { 1, 2, 3 };

        list.RemoveIfExists(1);

        list.ShouldBe(new[] { 1, 3 });
    }

    [Fact]
    public void RemoveIfExists_IgnoresNegativeIndex()
    {
        IList list = new List<int> { 1, 2, 3 };

        list.RemoveIfExists(-1);

        list.ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void RemoveIfExists_IgnoresIndexOutOfRange()
    {
        IList list = new List<int> { 1, 2, 3 };

        list.RemoveIfExists(5);

        list.ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void Join_WithStringSeparator()
    {
        List<string> values = ["a", "b", "c"];

        values.Join(", ").ShouldBe("a, b, c");
        values.Join().ShouldBe("a b c");
    }

    [Fact]
    public void Join_WithCharSeparator()
    {
        List<string> values = ["a", "b", "c"];

        values.Join('-').ShouldBe("a-b-c");
    }

    [Fact]
    public void Join_SupportsAnyEnumerable()
    {
        string[] array = ["x", "y"];

        array.Join('|').ShouldBe("x|y");
    }

    [Fact]
    public void ModifyElements_TransformsListInPlace()
    {
        List<int> list = [1, 2, 3];

        list.ModifyElements(value => value * 10);

        list.ShouldBe(new[] { 10, 20, 30 });
    }

    [Fact]
    public void IsEmpty_ReflectsCount()
    {
        List<int> list = [];

        list.IsEmpty.ShouldBeTrue();
        list.Add(1);
        list.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void IsEmpty_WorksForDictionaries()
    {
        Dictionary<string, int> dictionary = new();

        dictionary.IsEmpty.ShouldBeTrue();
        dictionary["a"] = 1;
        dictionary.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void IsEmpty_WorksForArrays()
    {
        int[] array = [];

        array.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_IsTrueForNullEmptyAndFalseForContent()
    {
        List<int>? nullList = null;
        List<int> emptyList = [];
        List<int> populatedList = [1];

        nullList.IsNullOrEmpty().ShouldBeTrue();
        emptyList.IsNullOrEmpty().ShouldBeTrue();
        populatedList.IsNullOrEmpty().ShouldBeFalse();
    }

    [Fact]
    public void PrintContents_EmptyList()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        List<int> list = [];

        list.PrintContents().ShouldBe("[ ]");
    }

    [Fact]
    public void PrintContents_SingleItemList()
    {
        List<int> list = [1];

        list.PrintContents().ShouldBe("[ 1 ]");
    }

    [Fact]
    public void PrintContents_MultipleItemsHorizontal()
    {
        List<int> list = [1, 2, 3];

        list.PrintContents().ShouldBe("[1, 2, 3]");
    }

    [Fact]
    public void PrintContents_MultipleItemsVertical()
    {
        List<int> list = [1, 2];

        list.PrintContents(verticalPrint: true).ShouldBe("[\n  1,\n  2\n]");
    }

    [Fact]
    public void PrintContents_NestedList()
    {
        List<object> list =
        [
            new List<int> { 1, 2 },
            3
        ];

        list.PrintContents().ShouldBe("[[1, 2], 3]");
    }

    [Fact]
    public void PrintContents_NullElementPrintsNull()
    {
        List<object?> list = [null];

        list.PrintContents().ShouldBe("[ NULL ]");
    }

    [Fact]
    public void PrintContents_EmptyDictionary()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        Dictionary<string, int> dictionary = new();

        dictionary.PrintContents().ShouldBe("{ }");
    }

    [Fact]
    public void PrintContents_DictionaryHorizontal()
    {
        Dictionary<string, int> dictionary = new()
        {
            ["a"] = 1,
            ["b"] = 2
        };

        dictionary.PrintContents().ShouldBe("{ a: 1, b: 2 }");
    }

    [Fact]
    public void PrintContents_DictionaryVertical()
    {
        Dictionary<string, int> dictionary = new()
        {
            ["a"] = 1,
            ["b"] = 2
        };

        dictionary.PrintContents(verticalPrint: true).ShouldBe("{\n  a: 1,\n  b: 2\n}");
    }

    [Fact]
    public void PrintContents_DictionaryWithListValue()
    {
        Dictionary<string, object> dictionary = new()
        {
            ["a"] = new List<int> { 1, 2 }
        };

        dictionary.PrintContents().ShouldBe("{ a: [1, 2] }");
    }

    [Fact]
    public void PrintContents_ListWithDictionaryItem()
    {
        List<object> list =
        [
            new Dictionary<string, int> { ["a"] = 1 }
        ];

        list.PrintContents().ShouldBe("[ { a: 1 } ]");
    }

    [Fact]
    public void PrintContents_NestedDictionaryInVerticalMode()
    {
        Dictionary<string, object> dictionary = new()
        {
            ["outer"] = new List<int> { 1, 2 }
        };

        dictionary.PrintContents(verticalPrint: true).ShouldBe("{\n  outer: [\n    1,\n    2\n  ]\n}");
    }

    [Fact]
    public void PrintContents_ThrowsOnNull()
    {
        IEnumerable? collection = null;

        Should.Throw<ArgumentNullException>(() => collection!.PrintContents());
    }

    [Fact]
    public void Add_AddsAllArguments()
    {
        List<int> list = [1];

        list.Add(2, 3, 4);

        list.ShouldBe(new[] { 1, 2, 3, 4 });
    }

    [Fact]
    public void AddIf_AddsWhenConditionIsTrue()
    {
        IList list = new List<int> { 1 };

        list.AddIf(2, condition: true);

        list.ShouldBe(new[] { 1, 2 });
    }

    [Fact]
    public void AddIf_SkipsWhenConditionIsFalse()
    {
        IList list = new List<int> { 1 };

        list.AddIf(2, condition: false);

        list.ShouldBe(new[] { 1 });
    }

    [Fact]
    public void ReplaceAllIfType_ReplacesMatchingItems()
    {
        IList list = new List<object> { 1, "text", 2 };

        list.ReplaceAllIfType(typeof(int), "number");

        list.ShouldBe(new object[] { "number", "text", "number" });
    }

    [Fact]
    public void ReplaceAllIfType_DoesNotReplaceOnDerivedTypes()
    {
        IList list = new List<object> { "text" };

        list.ReplaceAllIfType(typeof(object), "replaced");

        list.ShouldBe(new object[] { "text" });
    }

    [Fact]
    public void ReplaceAllIfType_NullElementIsNotReplaced()
    {
        IList list = new List<object?> { null, 1 };

        list.ReplaceAllIfType(typeof(int), "x");

        list.ShouldBe(new object?[] { null, "x" });
    }

    [Fact]
    public void ReplaceAllIfType_EmptyListIsNoOp()
    {
        IList list = new List<object>();

        list.ReplaceAllIfType(typeof(int), "x");

        list.ShouldBe(Array.Empty<object>());
    }

    [Fact]
    public void RandomItem_ReturnsElementFromList()
    {
        List<int> list = [1, 2, 3];

        for (int i = 0; i < 100; i++)
        {
            list.RandomItem().ShouldBeOneOf(1, 2, 3);
        }
    }

    [Fact]
    public void RandomItem_SingleElementList()
    {
        List<int> list = [42];

        list.RandomItem().ShouldBe(42);
    }

    [Fact]
    public void RandomItem_ThrowsForEmptyList()
    {
        List<int> list = [];

        Should.Throw<InvalidOperationException>(() => list.RandomItem());
    }

    [Fact]
    public void RandomItem_ThrowsOnNull()
    {
        List<int>? list = null;

        Should.Throw<ArgumentNullException>(() => list!.RandomItem());
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(-1, false)]
    [InlineData(3, false)]
    public void IsIndexValid(int index, bool expected)
    {
        List<int> list = [1, 2, 3];

        list.IsIndexValid(index).ShouldBe(expected);
    }

    [Fact]
    public void PrintContents_NonListEnumerableFallsBackToString()
    {
        HashSet<int> set = [1, 2];

        set.PrintContents().ShouldContain("HashSet");
    }
}
