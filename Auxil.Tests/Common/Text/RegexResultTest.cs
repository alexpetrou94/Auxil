using JetBrains.Annotations;
using Auxil.Common.Text;

namespace Auxil.Tests.Common.Text;

[TestSubject(typeof(RegexResult))]
public class RegexResultTest
{
    [Fact]
    public void RecordSemantics_EqualResultsAreEqual()
    {
        RegexResult a = new()
        {
            Success = true,
            Pattern = "x",
            InputText = "text",
            StartIndex = 1,
            EndIndex = 2,
            Groups = new Dictionary<string, string> { ["0"] = "x" }
        };

        RegexResult b = new()
        {
            Success = true,
            Pattern = "x",
            InputText = "text",
            StartIndex = 1,
            EndIndex = 2,
            Groups = new Dictionary<string, string> { ["0"] = "x" }
        };

        a.ShouldBe(b);
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void RecordSemantics_DifferentResultsAreNotEqual()
    {
        RegexResult a = new() { Success = true };
        RegexResult b = new() { Success = false };

        a.ShouldNotBe(b);
    }

    [Fact]
    public void Equals_NullIsNotEqual()
    {
        RegexResult result = new() { Success = true };

        result.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void Equals_SameInstanceIsEqual()
    {
        RegexResult result = new() { Success = true };

        result.Equals(result).ShouldBeTrue();
    }

    [Fact]
    public void Equals_DifferentGroupKeysAreNotEqual()
    {
        RegexResult a = new()
        {
            Groups = new Dictionary<string, string> { ["0"] = "x", ["a"] = "y" }
        };

        RegexResult b = new()
        {
            Groups = new Dictionary<string, string> { ["0"] = "x", ["b"] = "z" }
        };

        a.Equals(b).ShouldBeFalse();
        b.Equals(a).ShouldBeFalse();
    }

    [Fact]
    public void Equals_DifferentGroupValuesAreNotEqual()
    {
        RegexResult a = new()
        {
            Groups = new Dictionary<string, string> { ["0"] = "x" }
        };

        RegexResult b = new()
        {
            Groups = new Dictionary<string, string> { ["0"] = "y" }
        };

        a.Equals(b).ShouldBeFalse();
    }

    [Fact]
    public void GetCompleteMatch_ReturnsGroupZero()
    {
        RegexResult result = new()
        {
            Groups = new Dictionary<string, string> { ["0"] = "matched", ["word"] = "matched" }
        };

        result.GetCompleteMatch().ShouldBe("matched");
    }

    [Fact]
    public void GetCompleteMatch_EmptyWhenGroupZeroMissing()
    {
        RegexResult result = new();

        result.GetCompleteMatch().ShouldBe(string.Empty);
    }

    [Fact]
    public void GetString_ReturnsGroupValue()
    {
        RegexResult result = new()
        {
            Groups = new Dictionary<string, string> { ["word"] = "hello" }
        };

        result.GetString("word").ShouldBe("hello");
    }

    [Fact]
    public void GetString_EmptyWhenGroupMissing()
    {
        RegexResult result = new();

        result.GetString("missing").ShouldBe(string.Empty);
    }

    [Fact]
    public void ToString_ContainsAllFields()
    {
        RegexResult result = new()
        {
            Success = true,
            Pattern = @"\w+",
            InputText = "hello",
            StartIndex = 0,
            EndIndex = 5,
            Groups = new Dictionary<string, string> { ["0"] = "hello" }
        };

        string output = result.ToString();

        output.ShouldContain("Success: True");
        output.ShouldContain(@"Pattern: \w+");
        output.ShouldContain("InputText: hello");
        output.ShouldContain("StartIndex: 0");
        output.ShouldContain("EndIndex: 5");
        output.ShouldContain("hello");
    }
}
