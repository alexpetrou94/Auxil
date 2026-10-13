namespace Auxil.Pathfinding;

/// <summary>Which search engine a <see cref="Pathfinder"/> uses.</summary>
public enum PathfinderAlgorithm
{
    /// <summary>
    /// The general-purpose choice. A* with a bucket-list open list; handles terrain costs and is
    /// the fastest option on most maps. Start here.
    /// </summary>
    Default,

    /// <summary>
    /// A* with precomputed move masks. Uniform terrain only. A little faster than the default on
    /// cluttered or stacked maps. The masks are built at construction, so later map edits are not
    /// seen; use <see cref="Default"/> on maps that change.
    /// </summary>
    BitboardAStar,

    /// <summary>
    /// Jump Point Search. Uniform terrain only. Skips along straight lines and stops only at
    /// corners, so it examines far fewer tiles on cluttered maps.
    /// </summary>
    JumpPointSearch,

    /// <summary>
    /// JPS with precomputed jump and wall distances. Uniform terrain only, single layer. The
    /// fastest choice for large, static, open maps. The table is built at construction, so later
    /// map edits are not seen; use <see cref="Default"/> on maps that change.
    /// </summary>
    JpsPlus,

    /// <summary>
    /// Hierarchical A*: precomputes a small graph of cluster entrances, then searches that graph
    /// instead of the whole map. Uniform terrain only. Very fast on large maps, but the route is
    /// not guaranteed to be the cheapest one. The graph is built at construction, so later map
    /// edits are not seen; use <see cref="Default"/> on maps that change.
    /// </summary>
    HpaStar,
}
