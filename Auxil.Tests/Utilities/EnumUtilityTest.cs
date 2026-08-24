using JetBrains.Annotations;
using Auxil.Common;
using Auxil.Utilities;

namespace Auxil.Tests.Utilities;

[TestSubject(typeof(EnumUtility<>))]
public class EnumUtilityTest
{
            [Fact]
    public void GetValueNames_ExcludesGivenValues()
    {
        EnumUtility<VersionDevelopmentStage>.GetValueNames([VersionDevelopmentStage.Alpha, VersionDevelopmentStage.Final])
            .ShouldBe(new[] { "None", "PreAlpha", "Beta", "Rc" });
    }

    [Fact]
    public void GetValueNames_ExcludingAllReturnsEmpty()
    {
        List<VersionDevelopmentStage> all = [.. Enum.GetValues<VersionDevelopmentStage>()];

        EnumUtility<VersionDevelopmentStage>.GetValueNames(all).ShouldBeEmpty();
    }
}
