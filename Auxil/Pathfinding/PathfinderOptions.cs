namespace Auxil.Pathfinding;

/// <summary>Knobs for a <see cref="Pathfinder"/>. The defaults are suitable for most uses.</summary>
public sealed class PathfinderOptions
{
    /// <summary>
    /// Shorten the route with straight-line shortcuts wherever the map allows it. On by default,
    /// which turns a staircase of single steps into a few waypoints. Only applied over normal
    /// ground, so terrain costs are never ignored.
    /// </summary>
    public bool Smooth { get; set; } = true;
}
