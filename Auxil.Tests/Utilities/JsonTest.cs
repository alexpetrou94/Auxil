using System.Text.Json;
using JetBrains.Annotations;
using Json = Auxil.Utilities.Json;

namespace Auxil.Tests.Utilities;

[TestSubject(typeof(Json))]
public class JsonTest
{
    [Fact]
    public void Serialize_IsIndentedByDefault()
    {
        string json = Json.Serialize(new { Name = "test", Value = 1 });

        json.ShouldContain("\n  \"Name\": \"test\"");
        json.ShouldContain("\n  \"Value\": 1");
    }

    [Fact]
    public void Serialize_CompactWithOptions()
    {
        JsonSerializerOptions compact = new()
        {
            WriteIndented = false
        };

        string json = Json.Serialize(new { Name = "test" }, compact);

        json.ShouldBe("{\"Name\":\"test\"}");
    }

    [Fact]
    public void Serialize_NullProducesNullLiteral()
    {
        Json.Serialize<object?>(null).ShouldBe("null");
    }

    [Fact]
    public void Deserialize_RoundTripsValue()
    {
        TestPayload payload = new() { Name = "hello", Count = 42 };

        string json = Json.Serialize(payload);

        Json.Deserialize<TestPayload>(json).ShouldBe(payload);
    }

    [Theory]
    [InlineData("[1, 2, 3]")]
    [InlineData("[1,2,3]")]
    public void DeserializeSingleOrList_ArrayInput(string json)
    {
        Json.DeserializeSingleOrList<int>(json).ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void DeserializeSingleOrList_SingleObjectInput()
    {
        List<TestPayload> result = Json.DeserializeSingleOrList<TestPayload>("""{"Name":"single","Count":1}""");

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("single");
        result[0].Count.ShouldBe(1);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("null")]
    public void DeserializeSingleOrList_ReturnsEmptyListForNonContent(string json)
    {
        Json.DeserializeSingleOrList<int>(json).ShouldBeEmpty();
    }

    [Fact]
    public void DeserializeSingleOrList_ThrowsForInvalidJson()
    {
        Should.Throw<JsonException>(() => Json.DeserializeSingleOrList<int>("not json"));
    }

    [Fact]
    public void DeserializeSingleOrList_ThrowsOnNull()
    {
        Should.Throw<ArgumentNullException>(() => Json.DeserializeSingleOrList<int>(null!));
    }

    [Fact]
    public void Deserialize_ThrowsOnNull()
    {
        Should.Throw<ArgumentNullException>(() => Json.Deserialize<int>(null!));
    }

    private sealed class TestPayload : IEquatable<TestPayload>
    {
        public string Name { get; init; } = string.Empty;
        public int Count { get; init; }

        public bool Equals(TestPayload? other)
            => other is not null && Name == other.Name && Count == other.Count;

        public override bool Equals(object? obj) => Equals(obj as TestPayload);

        public override int GetHashCode() => HashCode.Combine(Name, Count);
    }
}
