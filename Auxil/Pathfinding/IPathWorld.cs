using Auxil.Pathfinding.Engine;

namespace Auxil.Pathfinding;

/// <summary>
/// The shared view of a searchable world that <see cref="Pathfinder"/>, <see cref="PathSearch"/>,
/// <see cref="CrowdField"/> and <see cref="PathfinderPool"/> work against: either a single
/// <see cref="PathMap"/> or a <see cref="PathMapStack"/>. Internal plumbing; not part of the
/// public API.
/// </summary>
internal interface IPathWorld
{
    Grid Grid { get; }

    int[] ExtraCosts { get; }

    bool HasTerrainCosts { get; }

    int Width { get; }

    int Height { get; }

    int LayerCount { get; }

    bool Contains(PathPoint point);

    bool IsBlocked(PathPoint point);

    int ToIndex(PathPoint point);

    PathPoint ToPoint(int index);
}
