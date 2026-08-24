using System.Globalization;
using System.Numerics;

namespace Auxil.Common;

/// <summary>An immutable semantic version number with an optional development stage and build number.</summary>
[PublicAPI]
public readonly record struct VersionNumber(
    uint Major = 0,
    uint Minor = 0,
    uint Patch = 0,
    uint Build = 0,
    VersionDevelopmentStage Stage = VersionDevelopmentStage.None)
    : IComparable<VersionNumber>, IComparable, IComparisonOperators<VersionNumber, VersionNumber, bool>,
        IParsable<VersionNumber>
{
    /// <summary>Returns a new version with the major component incremented and minor and patch reset.</summary>
    public VersionNumber BumpMajor() => this with { Major = Major + 1, Minor = 0, Patch = 0 };

    /// <summary>Returns a new version with the minor component incremented and patch reset.</summary>
    public VersionNumber BumpMinor() => this with { Minor = Minor + 1, Patch = 0 };

    /// <summary>Returns a new version with the patch component incremented.</summary>
    public VersionNumber BumpPatch() => this with { Patch = Patch + 1 };

    /// <summary>Compares this version to another, ordering releases after prereleases of the same components.</summary>
    /// <param name="other">The version to compare against.</param>
    public int CompareTo(VersionNumber other)
    {
        int majorComparison = Major.CompareTo(other.Major);

        if (majorComparison != 0)
        {
            return majorComparison;
        }

        int minorComparison = Minor.CompareTo(other.Minor);

        if (minorComparison != 0)
        {
            return minorComparison;
        }

        int patchComparison = Patch.CompareTo(other.Patch);

        if (patchComparison != 0)
        {
            return patchComparison;
        }

        bool thisIsPrerelease = Stage is >= VersionDevelopmentStage.PreAlpha and <= VersionDevelopmentStage.Rc;
        bool otherIsPrerelease = other.Stage is >= VersionDevelopmentStage.PreAlpha and <= VersionDevelopmentStage.Rc;

        if (thisIsPrerelease != otherIsPrerelease)
        {
            return thisIsPrerelease ? -1 : 1;
        }

        if (thisIsPrerelease)
        {
            int stageComparison = Stage.CompareTo(other.Stage);

            if (stageComparison != 0)
            {
                return stageComparison;
            }
        }

        return Build.CompareTo(other.Build);
    }

    int IComparable.CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1;
        }

        if (obj is not VersionNumber other)
        {
            throw new ArgumentException($"Object must be of type {nameof(VersionNumber)}.", nameof(obj));
        }

        return CompareTo(other);
    }

    /// <summary>Whether this version is less than the other.</summary>
    public static bool operator <(VersionNumber left, VersionNumber right) => left.CompareTo(right) < 0;

    /// <summary>Whether this version is greater than the other.</summary>
    public static bool operator >(VersionNumber left, VersionNumber right) => left.CompareTo(right) > 0;

    /// <summary>Whether this version is less than or equal to the other.</summary>
    public static bool operator <=(VersionNumber left, VersionNumber right) => left.CompareTo(right) <= 0;

    /// <summary>Whether this version is greater than or equal to the other.</summary>
    public static bool operator >=(VersionNumber left, VersionNumber right) => left.CompareTo(right) >= 0;

    /// <summary>Parses a version string such as <c>1.2.3-beta+5</c>.</summary>
    /// <param name="s">The version string.</param>
    /// <param name="provider">Unused format provider.</param>
    public static VersionNumber Parse(string s, IFormatProvider? provider)
        => TryParse(s, provider, out VersionNumber result)
            ? result
            : throw new FormatException($"\"{s}\" is not a valid version number.");

    /// <summary>Tries to parse a version string such as <c>1.2.3-beta+5</c>.</summary>
    /// <param name="s">The version string.</param>
    /// <param name="provider">Unused format provider.</param>
    /// <param name="result">The parsed version on success.</param>
    public static bool TryParse(string? s, IFormatProvider? provider, out VersionNumber result)
    {
        result = default;

        if (string.IsNullOrEmpty(s))
        {
            return false;
        }

        string[] buildParts = s.Split('+', 2);
        uint build = 0;

        if (buildParts.Length == 2 && !uint.TryParse(buildParts[1], NumberStyles.None, CultureInfo.InvariantCulture, out build))
        {
            return false;
        }

        string[] stageParts = buildParts[0].Split('-', 2);
        string[] numberParts = stageParts[0].Split('.');

        if (numberParts.Length is < 1 or > 3)
        {
            return false;
        }

        if (!uint.TryParse(numberParts[0], NumberStyles.None, CultureInfo.InvariantCulture, out uint major))
        {
            return false;
        }

        uint minor = 0;
        uint patch = 0;

        if (numberParts.Length >= 2 && !uint.TryParse(numberParts[1], NumberStyles.None, CultureInfo.InvariantCulture, out minor))
        {
            return false;
        }

        if (numberParts.Length >= 3 && !uint.TryParse(numberParts[2], NumberStyles.None, CultureInfo.InvariantCulture, out patch))
        {
            return false;
        }

        VersionDevelopmentStage stage = VersionDevelopmentStage.None;

        if (stageParts.Length == 2)
        {
            stage = stageParts[1] switch
            {
                "pre" => VersionDevelopmentStage.PreAlpha,
                "alpha" => VersionDevelopmentStage.Alpha,
                "beta" => VersionDevelopmentStage.Beta,
                "rc" => VersionDevelopmentStage.Rc,
                _ => VersionDevelopmentStage.None
            };

            if (stage == VersionDevelopmentStage.None)
            {
                return false;
            }
        }

        result = new VersionNumber(major, minor, patch, build, stage);
        return true;
    }

    /// <summary>Returns the semver-style representation, e.g. <c>1.2.3-beta+5</c>.</summary>
    public override string ToString()
    {
        string versionString = $"{Major}.{Minor}.{Patch}";

        if (Stage is >= VersionDevelopmentStage.PreAlpha and <= VersionDevelopmentStage.Rc)
        {
            versionString += $"-{Stage.GetStringValue()}";
        }

        if (Build != 0)
        {
            versionString += $"+{Build}";
        }

        return versionString;
    }
}
