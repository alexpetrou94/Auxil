using JetBrains.Annotations;
using Auxil.Utilities.DummyUtility;
using Auxil.Extensions;

namespace Auxil.Tests.Utilities;

[TestSubject(typeof(Dummy))]
public class DummyTest
{
    [Theory]
    [InlineData(DummyDataType.LoremIpsum)]
    [InlineData(DummyDataType.EnglishGibberish)]
    public void Word_ReturnsNonEmptyWord(DummyDataType type)
    {
        Dummy.Word(type).ShouldNotBeEmpty();
    }

    [Fact]
    public void Word_ThrowsForInvalidType()
    {
        Should.Throw<ArgumentException>(() => Dummy.Word((DummyDataType)99));
    }

    [Fact]
    public void Words_ReturnsRequestedCount()
    {
        Dummy.Words(DummyDataType.LoremIpsum, 5).Count.ShouldBe(5);
    }

    [Fact]
    public void Sentence_RespectsInclusiveBounds()
    {
        string sentence = Dummy.Sentence(DummyDataType.LoremIpsum, 5, 5);

        sentence.Words.Count.ShouldBe(5);
    }

    [Fact]
    public void Sentence_IsCapitalizedAndEndsWithPeriod()
    {
        string sentence = Dummy.Sentence(DummyDataType.LoremIpsum, 3, 3);

        char.IsUpper(sentence[0]).ShouldBeTrue();
        sentence.EndsWith('.').ShouldBeTrue();
    }

    [Fact]
    public void Sentences_ReturnsRequestedCountWithSeparator()
    {
        string sentences = Dummy.Sentences(DummyDataType.LoremIpsum, 4, " | ");

        sentences.Split(" | ").Length.ShouldBe(4);
    }

    [Fact]
    public void Paragraph_RespectsInclusiveBounds()
    {
        string paragraph = Dummy.Paragraph(DummyDataType.LoremIpsum, 2, 2);

        paragraph.CountSentences().ShouldBe(2);
    }

    }
