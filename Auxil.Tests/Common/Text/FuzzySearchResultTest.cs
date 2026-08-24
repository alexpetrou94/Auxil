using JetBrains.Annotations;
using Auxil.Common.Text;

namespace Auxil.Tests.Common.Text;

[TestSubject(typeof(FuzzySearchResult))]
public class FuzzySearchResultTest
{
    [Fact]
    public void RecordSemantics_EqualResultsAreEqual()
    {
        FuzzySearchResult a = new() { Score = 5, MaxScore = 10, Text = "hello" };
        FuzzySearchResult b = new() { Score = 5, MaxScore = 10, Text = "hello" };

        a.ShouldBe(b);
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void ScorePercentage_IsRoundedPercentage()
    {
        FuzzySearchResult result = new() { Score = 5, MaxScore = 9 };

        result.ScorePercentage.ShouldBe(56);
    }

    [Fact]
    public void ScorePercentage_ZeroMaxScoreDoesNotDivideByZero()
    {
        FuzzySearchResult result = new() { Score = 5, MaxScore = 0 };

        result.ScorePercentage.ShouldBe(0);
    }

    [Fact]
    public void ToString_ContainsPercentageAndText()
    {
        FuzzySearchResult result = new() { Score = 10, MaxScore = 10, Text = "hello" };

        result.ToString().ShouldBe("(100%) hello");
    }
}
