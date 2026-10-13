namespace Auxil.Pathfinding.Engine;

/// <summary>
/// A path finder over a <see cref="Grid"/>. <see cref="Init"/> may allocate and precompute freely;
/// <see cref="FindPath"/> is the query operation and must not allocate after warmup.
/// </summary>
internal interface IPathFinder
{
    /// <summary>Prepares the finder for a grid. Everything the search needs is allocated here.</summary>
    void Init(Grid grid);

    /// <summary>Finds a route from start to goal. Returns false when no route exists.</summary>
    /// <param name="start">Start cell id (see <see cref="Grid.Index"/>).</param>
    /// <param name="goal">Goal cell id.</param>
    /// <param name="cost">Cost of the found route under the movement model.</param>
    /// <param name="expanded">Number of nodes expanded (popped and processed).</param>
    bool FindPath(int start, int goal, out int cost, out int expanded);

    /// <summary>Drops all buffers so the instance can be collected.</summary>
    void Release();
}
