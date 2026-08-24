using JetBrains.Annotations;
using Auxil.Common.Text;

namespace Auxil.Tests.Common.Text;

[TestSubject(typeof(FuzzySearch))]
public class FuzzySearchTest
{
    [Fact]
    public void FindMatches_RanksExactMatchFirst()
    {
        FuzzySearch fuzzySearch = new(["hello", "world", "hell"]);

        List<FuzzySearchResult> results = fuzzySearch.FindMatches("hello");

        results[0].Text.ShouldBe("hello");
        results.Count.ShouldBe(3);
    }

    [Fact]
    public void FindMatches_QueryStrippedToEmptyByIgnoreCharactersDoesNotThrow()
    {
        FuzzySearchOptions options = new()
        {
            IgnoreCharacters = ['a']
        };

        FuzzySearch fuzzySearch = new(["hello"]);

        List<FuzzySearchResult> results = fuzzySearch.FindMatches("aaa", options);

        results.Count.ShouldBe(1);
        results[0].Text.ShouldBe("hello");
    }

    [Fact]
    public void FindMatches_IsCaseInsensitiveByDefault()
    {
        FuzzySearch fuzzySearch = new(["hello"]);

        fuzzySearch.FindMatches("HELLO")[0].Text.ShouldBe("hello");
    }

    [Fact]
    public void FindMatches_RespectsCaseSensitivity()
    {
        FuzzySearchOptions caseSensitive = new()
        {
            CaseInsensitive = false
        };

        FuzzySearch fuzzySearch = new(["hello", "HELLO"]);

        fuzzySearch.FindMatches("HELLO", caseSensitive)[0].Text.ShouldBe("HELLO");
    }

    [Fact]
    public void FindMatches_IgnoresWhitespaceByDefault()
    {
        FuzzySearch fuzzySearch = new(["helloworld"]);

        fuzzySearch.FindMatches("hello world")[0].Text.ShouldBe("helloworld");
    }

    [Fact]
    public void FindMatches_LimitsResults()
    {
        FuzzySearchOptions options = new()
        {
            MaxResults = 2
        };

        FuzzySearch fuzzySearch = new(["one", "two", "three", "four"]);

        fuzzySearch.FindMatches("o", options).Count.ShouldBe(2);
    }

    [Fact]
    public void FindMatches_ZeroMaxResultsReturnsAll()
    {
        FuzzySearchOptions options = new()
        {
            MaxResults = 0
        };

        FuzzySearch fuzzySearch = new(["one", "two", "three"]);

        fuzzySearch.FindMatches("o", options).Count.ShouldBe(3);
    }

    [Fact]
    public void FindMatches_EmptyQueryReturnsNoResults()
    {
        FuzzySearch fuzzySearch = new(["hello"]);

        fuzzySearch.FindMatches("").ShouldBeEmpty();
        fuzzySearch.FindMatches("   ").ShouldBeEmpty();
    }

    [Fact]
    public void FindMatches_NullQueryReturnsNoResults()
    {
        FuzzySearch fuzzySearch = new(["hello"]);

        fuzzySearch.FindMatches(null!).ShouldBeEmpty();
    }

    [Fact]
    public void FindMatches_ShortSearchWordDoesNotThrow()
    {
        FuzzySearch fuzzySearch = new(["a"]);

        List<FuzzySearchResult> results = fuzzySearch.FindMatches("hello");

        results.Count.ShouldBe(1);
        results[0].Text.ShouldBe("a");
    }

    [Fact]
    public void FindMatches_EmptySearchListEntryDoesNotThrow()
    {
        FuzzySearch fuzzySearch = new(["", "hello"]);

        List<FuzzySearchResult> results = fuzzySearch.FindMatches("hello");

        results[0].Text.ShouldBe("hello");
    }

    [Fact]
    public void FindMatches_ShortQueryWithLongWordsDoesNotThrow()
    {
        FuzzySearch fuzzySearch = new(["ab"]);

        fuzzySearch.FindMatches("a").Count.ShouldBe(1);
    }

    [Fact]
    public void FindMatches_OrdersByScoreDescending()
    {
        FuzzySearch fuzzySearch = new(["xyz", "help", "hello"]);

        List<FuzzySearchResult> results = fuzzySearch.FindMatches("hello");

        results.Select(result => result.Text).ShouldBe([ "hello", "help", "xyz" ]);
    }

    [Fact]
    public void FindMatches_ParallelOptionProducesSameResults()
    {
        List<string> searchList = Enumerable.Range(1, 200).Select(i => $"item{i}").ToList();
        FuzzySearchOptions parallel = new()
        {
            UseParallel = true
        };

        FuzzySearch fuzzySearch = new(searchList);

        List<string> sequential = fuzzySearch.FindMatches("item1", new FuzzySearchOptions())
            .Select(result => result.Text)
            .ToList();

        List<string> parallelResults = fuzzySearch.FindMatches("item1", parallel)
            .Select(result => result.Text)
            .ToList();

        parallelResults.Count.ShouldBe(200);
        parallelResults[0].ShouldBe(sequential[0]);
        parallelResults.OrderBy(result => result).ShouldBe(sequential.OrderBy(result => result));
    }
}
