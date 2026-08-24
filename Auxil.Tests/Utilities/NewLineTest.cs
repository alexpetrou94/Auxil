using JetBrains.Annotations;
using Auxil.Common.Text;
using Auxil.Utilities;

namespace Auxil.Tests.Utilities;

[TestSubject(typeof(NewLine))]
public class NewLineTest
{
    [Theory]
    [InlineData(NewLineCharacter.LineFeed, "\n")]
    [InlineData(NewLineCharacter.CarriageReturn, "\r")]
    [InlineData(NewLineCharacter.CarriageReturnLineFeed, "\r\n")]
    public void GetCharacterByType(NewLineCharacter newLineCharacter, string expected)
    {
        NewLine.GetCharacterByType(newLineCharacter).ShouldBe(expected);
    }

    [Theory]
    [InlineData(NewLineCharacter.None)]
    [InlineData(NewLineCharacter.Mixed)]
    [InlineData((NewLineCharacter)999)]
    public void GetCharacterByType_ThrowsForNonCharacterTypes(NewLineCharacter newLineCharacter)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => NewLine.GetCharacterByType(newLineCharacter));
    }
}
