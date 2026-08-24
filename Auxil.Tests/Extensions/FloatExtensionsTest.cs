using JetBrains.Annotations;
using Auxil.Extensions;

namespace Auxil.Tests.Extensions;

[TestSubject(typeof(FloatExtensions))]
public class FloatExtensionsTest
{
    [Theory]
    [InlineData(3.9f, 3)]
    [InlineData(3.0f, 3)]
    [InlineData(-3.9f, -3)]
    [InlineData(0.0f, 0)]
    public void ToInt_TruncatesTowardZero(float value, int expected)
    {
        value.ToInt().ShouldBe(expected);
    }

    [Theory]
    [InlineData(2.345f, 2, 2.34f)]
    [InlineData(2.345f, 1, 2.3f)]
    [InlineData(2.345f, 0, 2f)]
    public void Round_RoundsToDigits(float value, int digits, float expected)
    {
        value.Round(digits).ShouldBe(expected);
    }
}
