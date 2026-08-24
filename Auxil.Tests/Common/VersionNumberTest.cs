using JetBrains.Annotations;
using Auxil.Common;

namespace Auxil.Tests.Common;

[TestSubject(typeof(VersionNumber))]
public class VersionNumberTest
{
    [Theory]
    [InlineData(1, 2, 3, 0, VersionDevelopmentStage.None, "1.2.3")]
    [InlineData(1, 2, 3, 0, VersionDevelopmentStage.PreAlpha, "1.2.3-pre")]
    [InlineData(1, 2, 3, 0, VersionDevelopmentStage.Alpha, "1.2.3-alpha")]
    [InlineData(1, 2, 3, 0, VersionDevelopmentStage.Beta, "1.2.3-beta")]
    [InlineData(1, 2, 3, 0, VersionDevelopmentStage.Rc, "1.2.3-rc")]
    [InlineData(1, 2, 3, 0, VersionDevelopmentStage.Final, "1.2.3")]
    [InlineData(1, 2, 3, 5, VersionDevelopmentStage.None, "1.2.3+5")]
    [InlineData(1, 2, 3, 5, VersionDevelopmentStage.Alpha, "1.2.3-alpha+5")]
    public void ToString_UsesSemverFormat(uint major, uint minor, uint patch, uint build, VersionDevelopmentStage stage, string expected)
    {
        VersionNumber version = new(major, minor, patch, build, stage);

        version.ToString().ShouldBe(expected);
    }

    [Fact]
    public void Equality_EqualVersionsAreEqual()
    {
        VersionNumber a = new(1, 2, 3, 4, VersionDevelopmentStage.Alpha);
        VersionNumber b = new(1, 2, 3, 4, VersionDevelopmentStage.Alpha);

        a.Equals(b).ShouldBeTrue();
        (a == b).ShouldBeTrue();
        (a != b).ShouldBeFalse();
    }

    [Fact]
    public void Equality_DifferentVersionsAreNotEqual()
    {
        VersionNumber a = new(1, 2, 3);
        VersionNumber b = new(1, 2, 4);

        a.Equals(b).ShouldBeFalse();
        (a == b).ShouldBeFalse();
        (a != b).ShouldBeTrue();
    }

    [Fact]
    public void GetHashCode_EqualVersionsHaveEqualHashes()
    {
        VersionNumber a = new(1, 2, 3, 4, VersionDevelopmentStage.Alpha);
        VersionNumber b = new(1, 2, 3, 4, VersionDevelopmentStage.Alpha);

        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentVersionsDifferInHash()
    {
        VersionNumber a = new(1, 2, 3);
        VersionNumber b = new(2, 0, 0);

        a.GetHashCode().ShouldNotBe(b.GetHashCode());
    }

    [Theory]
    [InlineData(1, 2, 3, 1, 2, 4, -1)]
    [InlineData(1, 2, 4, 1, 2, 3, 1)]
    [InlineData(1, 2, 3, 2, 0, 0, -1)]
    [InlineData(1, 2, 3, 1, 2, 3, 0)]
    public void CompareTo_OrdersByComponents(uint majorA, uint minorA, uint patchA, uint majorB, uint minorB, uint patchB, int expected)
    {
        VersionNumber a = new(majorA, minorA, patchA);
        VersionNumber b = new(majorB, minorB, patchB);

        a.CompareTo(b).ShouldBe(expected);
    }

    [Fact]
    public void CompareTo_PrereleaseSortsBeforeRelease()
    {
        VersionNumber prerelease = new(1, 2, 3, Stage: VersionDevelopmentStage.Alpha);
        VersionNumber release = new(1, 2, 3);

        prerelease.CompareTo(release).ShouldBeNegative();
        release.CompareTo(prerelease).ShouldBePositive();
    }

    [Fact]
    public void CompareTo_PrereleaseStagesOrderCorrectly()
    {
        VersionNumber pre = new(1, 2, 3, Stage: VersionDevelopmentStage.PreAlpha);
        VersionNumber alpha = new(1, 2, 3, Stage: VersionDevelopmentStage.Alpha);
        VersionNumber beta = new(1, 2, 3, Stage: VersionDevelopmentStage.Beta);
        VersionNumber rc = new(1, 2, 3, Stage: VersionDevelopmentStage.Rc);

        pre.CompareTo(alpha).ShouldBeNegative();
        alpha.CompareTo(beta).ShouldBeNegative();
        beta.CompareTo(rc).ShouldBeNegative();
    }

    [Fact]
    public void CompareTo_PrereleasesWithSameStageCompareByBuild()
    {
        VersionNumber buildZero = new(1, 2, 3, Stage: VersionDevelopmentStage.Alpha);
        VersionNumber buildFive = new(1, 2, 3, 5, VersionDevelopmentStage.Alpha);

        buildZero.CompareTo(buildFive).ShouldBeNegative();
        buildFive.CompareTo(buildZero).ShouldBePositive();
    }

    [Fact]
    public void CompareTo_NoneAndFinalAreEqualReleases()
    {
        VersionNumber none = new(1, 2, 3, 4, VersionDevelopmentStage.None);
        VersionNumber final = new(1, 2, 3, 4, VersionDevelopmentStage.Final);

        none.CompareTo(final).ShouldBe(0);
        none.Equals(final).ShouldBeFalse();
    }

    [Fact]
    public void ComparisonOperators_Work()
    {
        VersionNumber low = new(1, 0, 0);
        VersionNumber high = new(2, 0, 0);

        (low < high).ShouldBeTrue();
        (low <= high).ShouldBeTrue();
        (high > low).ShouldBeTrue();
        (high >= low).ShouldBeTrue();
    }

    [Fact]
    public void Sort_UsesComparison()
    {
        List<VersionNumber> versions =
        [
            new(2, 0, 0),
            new(1, 2, 3, Stage: VersionDevelopmentStage.Alpha),
            new(1, 2, 3),
            new(1, 0, 0)
        ];

        versions.Sort();

        versions[0].ShouldBe(new VersionNumber(1, 0, 0));
        versions[1].ShouldBe(new VersionNumber(1, 2, 3, Stage: VersionDevelopmentStage.Alpha));
        versions[2].ShouldBe(new VersionNumber(1, 2, 3));
        versions[3].ShouldBe(new VersionNumber(2, 0, 0));
    }

    [Fact]
    public void CompareTo_Object_HandlesNullAndWrongType()
    {
        VersionNumber version = new(1, 0, 0);
        IComparable comparable = version;

        comparable.CompareTo(null).ShouldBePositive();
        Should.Throw<ArgumentException>(() => comparable.CompareTo("not a version"));
        comparable.CompareTo(new VersionNumber(1, 0, 0)).ShouldBe(0);
    }

    [Fact]
    public void BumpMajor_ReturnsNewVersionAndDoesNotMutate()
    {
        VersionNumber version = new(1, 2, 3);

        VersionNumber bumped = version.BumpMajor();

        bumped.ShouldBe(new VersionNumber(2, 0, 0));
        version.ShouldBe(new VersionNumber(1, 2, 3));
    }

    [Fact]
    public void BumpMinor_ReturnsNewVersion()
    {
        VersionNumber version = new(1, 2, 3);

        version.BumpMinor().ShouldBe(new VersionNumber(1, 3, 0));
    }

    [Fact]
    public void BumpPatch_ReturnsNewVersion()
    {
        VersionNumber version = new(1, 2, 3);

        version.BumpPatch().ShouldBe(new VersionNumber(1, 2, 4));
    }

    [Theory]
    [InlineData("1.2.3", 1, 2, 3, 0, VersionDevelopmentStage.None)]
    [InlineData("2", 2, 0, 0, 0, VersionDevelopmentStage.None)]
    [InlineData("1.2", 1, 2, 0, 0, VersionDevelopmentStage.None)]
    [InlineData("1.2.3-alpha", 1, 2, 3, 0, VersionDevelopmentStage.Alpha)]
    [InlineData("1.2.3-beta+5", 1, 2, 3, 5, VersionDevelopmentStage.Beta)]
    [InlineData("1.2.3-pre", 1, 2, 3, 0, VersionDevelopmentStage.PreAlpha)]
    [InlineData("1.2.3-rc", 1, 2, 3, 0, VersionDevelopmentStage.Rc)]
    [InlineData("1.2.3+7", 1, 2, 3, 7, VersionDevelopmentStage.None)]
    public void TryParse_ValidInputs(string text, uint major, uint minor, uint patch, uint build, VersionDevelopmentStage stage)
    {
        VersionNumber.TryParse(text, null, out VersionNumber result).ShouldBeTrue();
        result.ShouldBe(new VersionNumber(major, minor, patch, build, stage));
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("1.x")]
    [InlineData("1.2.x")]
    [InlineData("1.2.3.4.5")]
    [InlineData("1.2.3-unknown")]
    [InlineData("1.2.3+")]
    [InlineData("1.2.3+abc")]
    public void TryParse_InvalidInputs(string text)
    {
        VersionNumber.TryParse(text, null, out VersionNumber result).ShouldBeFalse();
        result.ShouldBe(default);
    }

    [Fact]
    public void Parse_ReturnsVersionForValidInput()
    {
        VersionNumber.Parse("1.2.3-beta+4", null).ShouldBe(new VersionNumber(1, 2, 3, 4, VersionDevelopmentStage.Beta));
    }

    [Fact]
    public void Parse_ThrowsFormatExceptionForInvalidInput()
    {
        Should.Throw<FormatException>(() => VersionNumber.Parse("not-a-version", null));
    }

    [Fact]
    public void Deconstruct_YieldsAllComponents()
    {
        VersionNumber version = new(1, 2, 3, 4, VersionDevelopmentStage.Alpha);

        (uint major, uint minor, uint patch, uint build, VersionDevelopmentStage stage) = version;

        major.ShouldBe(1u);
        minor.ShouldBe(2u);
        patch.ShouldBe(3u);
        build.ShouldBe(4u);
        stage.ShouldBe(VersionDevelopmentStage.Alpha);
    }
}
