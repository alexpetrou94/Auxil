using System.Runtime.CompilerServices;

namespace Auxil.Pathfinding.Engine;

/// <summary>
/// A* that adds an optional per-cell extra cost while entering a tile, used for terrain such as
/// roads, mud or forest. With no cost array the search is plain uniform-cost A*.
///
/// The heuristic stays the plain octile estimate, which remains admissible and consistent because
/// extra costs are never negative. The search can run to completion with <see cref="FindPath"/>,
/// or be advanced in bounded steps with <see cref="BeginSearch"/> and <see cref="Step"/>.
/// </summary>
internal sealed class WeightedAStar : GridSearchBase
{
    private readonly int[]? _extraCosts;
    private DialBucketsLazy _openList;
    private int _searchGeneration;
    private int _expandedCount;
    private int _resultCost;
    private SearchStepStatus _stepStatus;
    private bool _hasActiveSearch;

    /// <param name="extraCosts">Extra cost per cell, or null for uniform cost.</param>
    public WeightedAStar(int[]? extraCosts = null)
    {
        _extraCosts = extraCosts;
    }

    /// <summary>Number of tiles expanded by the current search.</summary>
    public int NodesExpanded => _expandedCount;

    /// <summary>Cost of the found route; valid once the state is <see cref="SearchStepStatus.Found"/>.</summary>
    public int ResultCost => _resultCost;

    public override void Init(Grid grid)
    {
        InitializeSearchState(grid);
        _openList.Init(grid);
    }

    public override void Release()
    {
        ReleaseSearchState();
        _openList = default;
    }

    public override bool FindPath(int start, int goal, out int cost, out int expanded)
    {
        BeginSearch(start, goal);
        SearchStepStatus status = Step(int.MaxValue);
        cost = _resultCost;
        expanded = _expandedCount;
        return status == SearchStepStatus.Found;
    }

    /// <summary>Resets the search and prepares it for the given start and goal.</summary>
    /// <param name="start">Start cell id.</param>
    /// <param name="goal">Goal cell id.</param>
    public void BeginSearch(int start, int goal)
    {
        _hasActiveSearch = true;
        _expandedCount = 0;
        _resultCost = 0;

        if (start == goal)
        {
            _searchGeneration = StartSearch(start, goal);
            _stepStatus = SearchStepStatus.Found;
            return;
        }

        _searchGeneration = StartSearch(start, goal);
        _openList.Clear();
        _openList.Push(Heuristic(Grid.XOf(start), Grid.YOf(start), Grid.ZOf(start)), start);
        _stepStatus = SearchStepStatus.Running;
    }

    /// <summary>Runs up to the given number of expansions and reports the current state.</summary>
    /// <param name="maxExpansions">Expansion budget for this step; at least one.</param>
    public SearchStepStatus Step(int maxExpansions)
    {
        if (!_hasActiveSearch)
        {
            throw new InvalidOperationException("Call BeginSearch before Step.");
        }

        if (_stepStatus != SearchStepStatus.Running)
        {
            return _stepStatus;
        }

        int budget = maxExpansions < 1 ? 1 : maxExpansions;
        int generation = _searchGeneration;
        int[] costToReach = CostToReach;
        int[] closed = ClosedGeneration;

        for (int expansion = 0; expansion < budget; expansion++)
        {
            if (_openList.Count == 0)
            {
                _stepStatus = SearchStepStatus.NoPath;
                return _stepStatus;
            }

            int currentId = _openList.Pop(out _);
            if (closed[currentId] == generation)
            {
                expansion--; // stale duplicate; does not consume the budget
                continue;
            }

            closed[currentId] = generation;
            _expandedCount++;

            if (currentId == GoalId)
            {
                _resultCost = costToReach[currentId];
                _stepStatus = SearchStepStatus.Found;
                return _stepStatus;
            }

            ExpandNode(currentId, generation);
        }

        return _stepStatus;
    }

    private void ExpandNode(int currentId, int generation)
    {
        int[] costToReach = CostToReach;
        int[] costGeneration = CostGeneration;
        int[] closed = ClosedGeneration;
        int[] parent = Parent;
        int[]? extraCosts = _extraCosts;
        int goalX = GoalX;
        int goalY = GoalY;
        int goalZ = GoalZ;

        int x = Grid.XOf(currentId);
        int y = Grid.YOf(currentId);
        int z = Grid.ZOf(currentId);
        int currentCost = costToReach[currentId];
        int directionCount = LayerCount > 1 ? Grid.DirectionCount : Grid.InPlaneDirectionCount;

        for (int direction = 0; direction < directionCount; direction++)
        {
            if (!Grid.TryStep(currentId, x, y, z, direction, out int neighborId))
            {
                continue;
            }

            if (closed[neighborId] == generation)
            {
                continue;
            }

            int newCost = currentCost + Grid.MoveCost[direction] + ExtraCostOf(extraCosts, neighborId);
            if (costGeneration[neighborId] == generation && newCost >= costToReach[neighborId])
            {
                continue;
            }

            costToReach[neighborId] = newCost;
            costGeneration[neighborId] = generation;
            parent[neighborId] = currentId;

            int neighborX = x + Grid.OffX[direction];
            int neighborY = y + Grid.OffY[direction];
            int neighborZ = z + Grid.OffZ[direction];
            _openList.Push(newCost + Grid.Octile(neighborX - goalX, neighborY - goalY) + 10 * Math.Abs(neighborZ - goalZ), neighborId);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ExtraCostOf(int[]? extraCosts, int cell) => extraCosts is null ? 0 : extraCosts[cell];
}
