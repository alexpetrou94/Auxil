using System.Runtime.CompilerServices;

namespace Auxil.Pathfinding.Engine;

/// <summary>
/// A* that searches toward the nearest of several goals at once: the heuristic is the smallest
/// distance to any goal, and the search stops at the first goal popped. Supports the same terrain
/// costs as <see cref="WeightedAStar"/>. The heuristic costs time proportional to the goal count
/// on every relaxation, so keep the goal list modest (a handful, not thousands).
/// </summary>
internal sealed class MultiGoalAStar : GridSearchBase
{
    private readonly int[]? _extraCosts;
    private readonly int[] _singleGoal = new int[1];
    private DialBucketsLazy _openList;
    private int[] _sortedGoals = [];
    private int[] _goalX = [];
    private int[] _goalY = [];
    private int[] _goalZ = [];
    private int _goalCount;

    /// <param name="extraCosts">Extra cost per cell, or null for uniform cost.</param>
    public MultiGoalAStar(int[]? extraCosts = null)
    {
        _extraCosts = extraCosts;
    }

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

    /// <summary>Single-goal overload required by <see cref="IPathFinder"/>.</summary>
    public override bool FindPath(int start, int goal, out int cost, out int expanded)
    {
        _singleGoal[0] = goal;
        return FindPath(start, _singleGoal, out _, out cost, out expanded);
    }

    /// <summary>
    /// Finds the cheapest route from start to any of the goals.
    /// </summary>
    /// <param name="start">Start cell id.</param>
    /// <param name="goals">Candidate goal cell ids; must not be empty.</param>
    /// <param name="reachedGoal">The goal that was reached.</param>
    /// <param name="cost">Cost of the route.</param>
    /// <param name="expanded">Number of tiles expanded.</param>
    public bool FindPath(int start, IReadOnlyList<int> goals, out int reachedGoal, out int cost, out int expanded)
    {
        reachedGoal = -1;
        cost = 0;
        expanded = 0;
        if (goals.Count == 0)
        {
            return false;
        }

        PrepareGoals(goals);
        if (IsGoal(start))
        {
            StartSearch(start, start);
            reachedGoal = start;
            return true;
        }

        int generation = StartSearch(start, _sortedGoals[0]);
        _openList.Clear();
        _openList.Push(HeuristicToNearestGoal(start), start);

        int[] costToReach = CostToReach;
        int[] closed = ClosedGeneration;
        int expandedCount = 0;

        while (_openList.Count > 0)
        {
            int currentId = _openList.Pop(out _);
            if (closed[currentId] == generation)
            {
                continue;
            }

            closed[currentId] = generation;
            expandedCount++;

            if (IsGoal(currentId))
            {
                reachedGoal = currentId;
                cost = costToReach[currentId];
                expanded = expandedCount;
                return true;
            }

            ExpandNode(currentId, generation);
        }

        expanded = expandedCount;
        return false;
    }

    private void PrepareGoals(IReadOnlyList<int> goals)
    {
        int count = goals.Count;
        if (_sortedGoals.Length < count)
        {
            _sortedGoals = new int[count];
            _goalX = new int[count];
            _goalY = new int[count];
            _goalZ = new int[count];
        }

        for (int i = 0; i < count; i++)
        {
            int goal = goals[i];
            _sortedGoals[i] = goal;
            _goalX[i] = Grid.XOf(goal);
            _goalY[i] = Grid.YOf(goal);
            _goalZ[i] = Grid.ZOf(goal);
        }

        Array.Sort(_sortedGoals, 0, count);
        _goalCount = count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsGoal(int cell) => Array.BinarySearch(_sortedGoals, 0, _goalCount, cell) >= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int HeuristicToNearestGoal(int cell)
    {
        int x = Grid.XOf(cell);
        int y = Grid.YOf(cell);
        int z = Grid.ZOf(cell);
        return HeuristicToNearestGoal(x, y, z);
    }

    private int HeuristicToNearestGoal(int x, int y, int z)
    {
        int best = int.MaxValue;
        for (int i = 0; i < _goalCount; i++)
        {
            int layerDistance = z - _goalZ[i];
            if (layerDistance < 0)
            {
                layerDistance = -layerDistance;
            }

            int estimate = Grid.Octile(x - _goalX[i], y - _goalY[i]) + 10 * layerDistance;
            if (estimate < best)
            {
                best = estimate;
            }
        }

        return best;
    }

    private void ExpandNode(int currentId, int generation)
    {
        int[] costToReach = CostToReach;
        int[] costGeneration = CostGeneration;
        int[] closed = ClosedGeneration;
        int[] parent = Parent;
        int[]? extraCosts = _extraCosts;

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
            _openList.Push(newCost + HeuristicToNearestGoal(neighborX, neighborY, neighborZ), neighborId);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ExtraCostOf(int[]? extraCosts, int cell) => extraCosts is null ? 0 : extraCosts[cell];
}
