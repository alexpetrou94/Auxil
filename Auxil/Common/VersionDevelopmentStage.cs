namespace Auxil.Common;

/// <summary>Defines the development stage of a version.</summary>
[PublicAPI]
public enum VersionDevelopmentStage
{
    /// <summary>No development stage (a release version).</summary>
    [StringValue("")] None,
    /// <summary>Pre-alpha stage.</summary>
    [StringValue("pre")] PreAlpha,
    /// <summary>Alpha stage.</summary>
    [StringValue("alpha")] Alpha,
    /// <summary>Beta stage.</summary>
    [StringValue("beta")] Beta,
    /// <summary>Release candidate stage.</summary>
    [StringValue("rc")] Rc,
    /// <summary>Final release stage.</summary>
    [StringValue("")] Final
}
