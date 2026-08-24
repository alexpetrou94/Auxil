using System.Text.Json.Nodes;
using JetBrains.Annotations;
using Auxil.Extensions;

namespace Auxil.Tests.Extensions;

[TestSubject(typeof(JsonExtensions))]
public class JsonExtensionsTest
{
    private static JsonNode? Parse(string json) => JsonNode.Parse(json);

    [Fact]
    public void Get_ReadsNestedPath()
    {
        JsonNode? node = Parse("""{"a":{"b":{"c":5}}}""");

        node.Get<int>("a.b.c").ShouldBe(5);
    }

    [Fact]
    public void Get_ReturnsDefaultForMissingProperty()
    {
        JsonNode? node = Parse("""{"a":{"b":5}}""");

        node.Get("a.c", 42).ShouldBe(42);
    }

    [Fact]
    public void Get_ReturnsDefaultForMissingPath()
    {
        JsonNode? node = Parse("""{"a":5}""");

        node.Get("x.y.z", 42).ShouldBe(42);
    }

    [Fact]
    public void Get_ReturnsDefaultWhenPathCrossesNonObject()
    {
        JsonNode? node = Parse("""{"a":5}""");

        node.Get("a.b", 42).ShouldBe(42);
    }

    [Fact]
    public void Get_ReturnsDefaultForNullNode()
    {
        JsonNode? node = null;

        node.Get("a", 42).ShouldBe(42);
    }

    [Fact]
    public void Get_ReadsBoolDirectly()
    {
        JsonNode? node = Parse("""{"flag":true}""");

        node.Get<bool>("flag").ShouldBeTrue();
    }

    [Fact]
    public void Get_ConvertsStringToBool()
    {
        JsonNode? node = Parse("""{"flag":"true","other":"false"}""");

        node.Get<bool>("flag").ShouldBeTrue();
        node.Get<bool>("other").ShouldBeFalse();
    }

    [Fact]
    public void Get_ConvertsStringToInt()
    {
        JsonNode? node = Parse("""{"count":"5"}""");

        node.Get<int>("count").ShouldBe(5);
    }

    [Fact]
    public void Get_ConvertsStringToDouble()
    {
        JsonNode? node = Parse("""{"pi":"3.5"}""");

        node.Get<double>("pi").ShouldBe(3.5);
    }

    [Fact]
    public void Get_ReadsStringFromStringNode()
    {
        JsonNode? node = Parse("""{"name":"hello"}""");

        node.Get<string>("name").ShouldBe("hello");
    }

    [Fact]
    public void Get_ReturnsDefaultForUnparseableString()
    {
        JsonNode? node = Parse("""{"count":"abc"}""");

        node.Get("count", 42).ShouldBe(42);
    }

    [Fact]
    public void Get_ReturnsDefaultForStringTargetOnNonStringNode()
    {
        JsonNode? node = Parse("""{"flag":true}""");

        node.Get<string>("flag", "fallback").ShouldBe("fallback");
    }

    [Fact]
    public void Get_EmptyPathReadsRootValue()
    {
        JsonNode node = JsonValue.Create(7);

        node.Get<int>("").ShouldBe(7);
    }

    [Fact]
    public void Get_ReturnsDefaultForNonValueRoot()
    {
        JsonNode? node = Parse("""{"a":1}""");

        node.Get<int>("").ShouldBe(0);
    }

    [Fact]
    public void Get_ReturnsDefaultForNullValueNode()
    {
        JsonNode? node = JsonValue.Create<string?>(null);

        node.Get("", 42).ShouldBe(42);
    }

    [Fact]
    public void Get_ThrowsOnNullPath()
    {
        JsonNode? node = Parse("""{"a":1}""");

        Should.Throw<ArgumentNullException>(() => node.Get<int>(null!));
    }
}
