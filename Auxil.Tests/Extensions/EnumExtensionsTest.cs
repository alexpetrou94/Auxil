using JetBrains.Annotations;
using Auxil.Common;
using Auxil.Common.Text;
using Auxil.Extensions;

namespace Auxil.Tests.Extensions;

[TestSubject(typeof(EnumExtensions))]
public class EnumExtensionsTest
{
    [Theory]
    [InlineData(VersionDevelopmentStage.None, "")]
    [InlineData(VersionDevelopmentStage.PreAlpha, "pre")]
    [InlineData(VersionDevelopmentStage.Alpha, "alpha")]
    [InlineData(VersionDevelopmentStage.Beta, "beta")]
    [InlineData(VersionDevelopmentStage.Rc, "rc")]
    [InlineData(VersionDevelopmentStage.Final, "")]
    public void GetStringValue_ReturnsAttributeValue(VersionDevelopmentStage stage, string expected)
    {
        stage.GetStringValue().ShouldBe(expected);
    }

    [Fact]
    public void GetStringValue_ReturnsEmptyForEnumWithoutAttribute()
    {
        NewLineCharacter.LineFeed.GetStringValue().ShouldBe(string.Empty);
        NewLineCharacter.None.GetStringValue().ShouldBe(string.Empty);
    }

    [Fact]
    public void GetStringValue_ReturnsEmptyForUndefinedEnumValue()
    {
        ((VersionDevelopmentStage)99).GetStringValue().ShouldBe(string.Empty);
    }

    [Fact]
    public void GetStringValue_IsCachedAcrossCalls()
    {
        VersionDevelopmentStage.Alpha.GetStringValue().ShouldBe("alpha");
        VersionDevelopmentStage.Beta.GetStringValue().ShouldBe("beta");
        VersionDevelopmentStage.Rc.GetStringValue().ShouldBe("rc");
    }

    }
