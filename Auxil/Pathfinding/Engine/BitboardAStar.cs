using System.Numerics;

namespace Auxil.Pathfinding.Engine;

/// <summary>
/// A* that stores every cell's legal moves as a 10-bit mask, built once from the same movement
/// model the other searches use. Expansion then iterates only the set bits, which removes the
/// repeated walkability and corner-rule tests from the inner loop. The mask costs two bytes per
/// cell and pays for itself on cluttered and stacked maps.
/// </summary>
internal sealed class BitboardAStar : GridSearchBase
{
    private ushort[] _moveMasks = null!;
    private DialBucketsLazy _openList;

    public override void Init(Grid grid)
    {
        InitializeSearchState(grid);
        BuildMoveMasks();
        _openList.Init(grid);
    }

    public override void Release()
    {
        ReleaseSearchState();
        _moveMasks = null!;
        _openList = default;
    }

    /// <summary>Precomputes, for every walkable cell, which of the 10 directions are legal steps.</summary>
    private void BuildMoveMasks()
    {
        _moveMasks = new ushort[NodeCount];
        int directionCount = LayerCount > 1 ? Grid.DirectionCount : Grid.InPlaneDirectionCount;

        for (int cell = 0; cell < NodeCount; cell++)
        {
            if (Cells[cell] == Grid.Blocked) continue;

            int x = Grid.XOf(cell);
            int y = Grid.YOf(cell);
            int z = Grid.ZOf(cell);
            int mask = 0;
            for (int direction = 0; direction < directionCount; direction++)
            {
                if (Grid.TryStep(cell, x, y, z, direction, out _)) mask |= 1 << direction;
            }
            _moveMasks[cell] = (ushort)mask;
        }
    }

    public override bool FindPath(int start, int goal, out int cost, out int expanded)
    {
        cost = 0;
        expanded = 0;
        if (start == goal) return true;

        int generation = StartSearch(start, goal);

        int[] costToReach = CostToReach;
        int[] closed = ClosedGeneration;

        _openList.Clear();
        _openList.Push(Heuristic(Grid.XOf(start), Grid.YOf(start), Grid.ZOf(start)), start);

        int expandedCount = 0;
        while (_openList.Count > 0)
        {
            int currentId = _openList.Pop(out _);
            if (closed[currentId] == generation) continue;
            closed[currentId] = generation;
            expandedCount++;

            if (currentId == GoalId)
            {
                cost = costToReach[currentId];
                expanded = expandedCount;
                return true;
            }

            ExpandNode(currentId, generation);
        }

        expanded = expandedCount;
        return false;
    }

    private void ExpandNode(int currentId, int generation)
    {
        int[] costToReach = CostToReach;
        int[] costGeneration = CostGeneration;
        int[] closed = ClosedGeneration;
        int[] parent = Parent;
        int goalX = GoalX;
        int goalY = GoalY;
        int goalZ = GoalZ;

        int x = Grid.XOf(currentId);
        int y = Grid.YOf(currentId);
        int z = Grid.ZOf(currentId);
        int currentCost = costToReach[currentId];

        uint mask = _moveMasks[currentId];
        while (mask != 0)
        {
            int direction = BitOperations.TrailingZeroCount(mask);
            mask &= mask - 1;

            int neighborId = Grid.NeighborId(currentId, direction);
            if (closed[neighborId] == generation) continue;

            int newCost = currentCost + Grid.MoveCost[direction];
            if (costGeneration[neighborId] == generation && newCost >= costToReach[neighborId]) continue;

            costToReach[neighborId] = newCost;
            costGeneration[neighborId] = generation;
            parent[neighborId] = currentId;

            int neighborX = x + Grid.OffX[direction];
            int neighborY = y + Grid.OffY[direction];
            int neighborZ = z + Grid.OffZ[direction];
            _openList.Push(newCost + Grid.Octile(neighborX - goalX, neighborY - goalY) + 10 * Math.Abs(neighborZ - goalZ), neighborId);
        }
    }
}
