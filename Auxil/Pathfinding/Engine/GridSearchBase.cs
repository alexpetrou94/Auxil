using System.Runtime.CompilerServices;

namespace Auxil.Pathfinding.Engine;

/// <summary>
/// Shared state and helpers for grid searches that expand nodes with generation-stamped arrays.
/// Nothing is cleared between searches; a monotone counter marks which search wrote each array
/// slot, so every query starts instantly and no memory is allocated after initialization.
///
/// Subclasses own their open list and expansion strategy and implement <see cref="FindPath"/>.
/// This base provides:
///   * the grid layout fields and the four per-node arrays every search needs,
///   * generation handling (<see cref="StartSearch"/> and the stamp helpers),
///   * the goal coordinates of the current search and the admissible heuristic,
///   * parent-chain path reconstruction.
/// </summary>
internal abstract class GridSearchBase : IPathFinder
{
    protected Grid Grid = null!;
    protected byte[] Cells = null!;

    protected int Width;
    protected int Height;
    protected int LayerCount;
    protected int NodeCount;

    /// <summary>Best known cost from the search start to each node (the A* g score).</summary>
    protected int[] CostToReach = null!;

    /// <summary>Search generation that last wrote <see cref="CostToReach"/> for each node.</summary>
    protected int[] CostGeneration = null!;

    /// <summary>Search generation that expanded (closed) each node.</summary>
    protected int[] ClosedGeneration = null!;

    /// <summary>Expansion parent per node, used by <see cref="CollectPath"/>.</summary>
    protected int[] Parent = null!;

    /// <summary>Monotone search counter. Arrays are stamped with it instead of being cleared.</summary>
    private int _searchGeneration;

    /// <summary>Goal of the search currently in progress.</summary>
    protected int GoalId;
    protected int GoalX;
    protected int GoalY;
    protected int GoalZ;

    public abstract void Init(Grid grid);
    public abstract bool FindPath(int start, int goal, out int cost, out int expanded);
    public abstract void Release();

    /// <summary>Captures the grid layout and allocates the four per-node arrays.</summary>
    protected void InitializeSearchState(Grid grid)
    {
        Grid = grid;
        Cells = grid.Cells;
        Width = grid.Width;
        Height = grid.Height;
        LayerCount = grid.Layers;
        NodeCount = grid.Size;
        CostToReach = new int[NodeCount];
        CostGeneration = new int[NodeCount];
        ClosedGeneration = new int[NodeCount];
        Parent = new int[NodeCount];
        _searchGeneration = 0;
    }

    /// <summary>
    /// Starts a search: advances the generation (clearing stamps only on wrap-around), records
    /// the goal and seeds the start node. Returns the generation the search should stamp with.
    /// </summary>
    protected int StartSearch(int start, int goal)
    {
        GoalId = goal;
        GoalX = Grid.XOf(goal);
        GoalY = Grid.YOf(goal);
        GoalZ = Grid.ZOf(goal);

        _searchGeneration++;
        if (_searchGeneration == int.MaxValue)
        {
            ClearGenerationArrays();
            _searchGeneration = 1;
        }

        CostToReach[start] = 0;
        CostGeneration[start] = _searchGeneration;
        Parent[start] = -1;
        return _searchGeneration;
    }

    /// <summary>Clears every generation-stamped array. Only called on counter wrap-around.</summary>
    protected virtual void ClearGenerationArrays()
    {
        Array.Clear(CostGeneration);
        Array.Clear(ClosedGeneration);
    }

    /// <summary>Releases the per-node arrays. Subclasses release their own buffers as well.</summary>
    protected void ReleaseSearchState()
    {
        CostToReach = null!;
        CostGeneration = null!;
        ClosedGeneration = null!;
        Parent = null!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsClosed(int node, int generation) => ClosedGeneration[node] == generation;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void CloseNode(int node, int generation) => ClosedGeneration[node] = generation;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool HasCost(int node, int generation) => CostGeneration[node] == generation;

    /// <summary>Heuristic from a cell to the goal of the current search.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int Heuristic(int x, int y, int z) => Grid.Heuristic(x, y, z, GoalX, GoalY, GoalZ);

    /// <summary>Writes the parent chain of the last search into <paramref name="path"/> (start to goal).</summary>
    public void CollectPath(int goal, List<int> path)
    {
        path.Clear();
        for (int node = goal; node != -1; node = Parent[node]) path.Add(node);
        path.Reverse();
    }
}
