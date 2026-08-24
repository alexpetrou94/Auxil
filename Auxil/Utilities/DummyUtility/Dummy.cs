namespace Auxil.Utilities.DummyUtility;

/// <summary>Generates dummy text such as lorem ipsum and English gibberish.</summary>
[PublicAPI]
public static class Dummy
{
    /// <summary>Returns a random word from the given data set.</summary>
    /// <param name="type">The word set to draw from.</param>
    public static string Word(DummyDataType type)
        => type switch
        {
            DummyDataType.LoremIpsum => StaticDummyData.LoremWords.RandomItem(),
            DummyDataType.EnglishGibberish => StaticDummyData.RandomEnglishWords.RandomItem(),
            _ => throw new ArgumentException($"{type} is not a valid DummyDataType.")
        };

    /// <summary>Returns the requested number of random words.</summary>
    /// <param name="type">The word set to draw from.</param>
    /// <param name="count">The number of words to produce.</param>
    public static List<string> Words(DummyDataType type, int count = 3)
        => Enumerable.Range(1, count).Select(_ => Word(type)).ToList();

    /// <summary>Returns a capitalized sentence ending with a period.</summary>
    /// <param name="type">The word set to draw from.</param>
    /// <param name="minWordCount">The minimum number of words, inclusive.</param>
    /// <param name="maxWordCount">The maximum number of words, inclusive.</param>
    public static string Sentence(DummyDataType type, int minWordCount = 5, int maxWordCount = 12)
    {
        int numberOfWords = Random.Shared.Next(minWordCount, maxWordCount + 1);
        string sentence = string.Join(" ", Words(type, numberOfWords));

        return sentence.Capitalize().Suffix(".");
    }

    /// <summary>Returns the requested number of sentences joined by a separator.</summary>
    /// <param name="type">The word set to draw from.</param>
    /// <param name="count">The number of sentences to produce.</param>
    /// <param name="separator">The separator between sentences.</param>
    public static string Sentences(DummyDataType type, int count = 3, string separator = "\n")
    {
        IEnumerable<string> sentences
            = Enumerable.Range(1, count).Select(_ => Sentence(type));

        return string.Join(separator, sentences);
    }

    /// <summary>Returns a paragraph made of the requested number of sentences.</summary>
    /// <param name="type">The word set to draw from.</param>
    /// <param name="minSentenceCount">The minimum number of sentences, inclusive.</param>
    /// <param name="maxSentenceCount">The maximum number of sentences, inclusive.</param>
    public static string Paragraph(DummyDataType type, int minSentenceCount = 3, int maxSentenceCount = 7)
        => Sentences(type, Random.Shared.Next(minSentenceCount, maxSentenceCount + 1));

    /// <summary>Returns the requested number of paragraphs joined by a separator.</summary>
    /// <param name="type">The word set to draw from.</param>
    /// <param name="count">The number of paragraphs to produce.</param>
    /// <param name="separator">The separator between paragraphs.</param>
    public static string Paragraphs(DummyDataType type, int count = 3, string separator = "\n\n")
    {
        IEnumerable<string> paragraphs
            = Enumerable.Range(1, count).Select(_ => Paragraph(type));

        return string.Join(separator, paragraphs);
    }
}
