using System.Runtime.CompilerServices;

namespace Auxil.Pathfinding.Engine;

/// <summary>
/// Jump Point Search (Harabor and Grastien) for uniform-cost, single-layer grids.
///
/// Instead of expanding every neighbour, JPS scans in straight and diagonal lines and only stops
/// at "jump points": cells with a forced neighbour (a wall configuration where an optimal route
/// must turn) or the goal. Successors are the pruned neighbours of the current jump point, each
/// jumped to the next jump point in its direction.
///
/// Only the diagonal scan calls the straight scan; straight scans never recurse, so jump depth is
/// bounded and no recursion is used. The movement model is the one in <see cref="Grid"/>.
/// </summary>
internal sealed class JumpPointSearch : GridSearchBase
{
    private const byte NoParentDirection = 255;

    private byte[] _parentDirection = null!;
    private BinaryHeapLazy _openList = new(1024);

    public override void Init(Grid grid)
    {
        if (grid.Layers != 1) throw new InvalidOperationException("JPS is defined for single-layer grids.");
        InitializeSearchState(grid);
        _parentDirection = new byte[NodeCount];
        _openList.Init(grid);
    }

    public override void Release()
    {
        ReleaseSearchState();
        _parentDirection = null!;
        _openList = default;
    }

    protected override void ClearGenerationArrays()
    {
        base.ClearGenerationArrays();
        Array.Clear(_parentDirection);
    }

    public override bool FindPath(int start, int goal, out int cost, out int expanded)
    {
        cost = 0;
        expanded = 0;
        if (start == goal) return true;

        int generation = StartSearch(start, goal);
        _parentDirection[start] = NoParentDirection;

        _openList.Clear();
        _openList.Push(Heuristic(Grid.XOf(start), Grid.YOf(start), 0), start);

        int expandedCount = 0;
        while (_openList.Count > 0)
        {
            int currentId = _openList.Pop();
            if (IsClosed(currentId, generation)) continue;
            CloseNode(currentId, generation);
            expandedCount++;

            if (currentId == GoalId)
            {
                cost = CostToReach[currentId];
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
        int x = Grid.XOf(currentId);
        int y = Grid.YOf(currentId);
        int currentCost = CostToReach[currentId];
        int parentDirection = _parentDirection[currentId];

        if (parentDirection == NoParentDirection)
        {
            // Start node: every legal move is a candidate.
            for (int direction = 0; direction < Grid.InPlaneDirectionCount; direction++)
                TryJumpDirection(currentId, x, y, currentCost, generation, Grid.OffX[direction], Grid.OffY[direction], direction);
            return;
        }

        int dx = Grid.OffX[parentDirection];
        int dy = Grid.OffY[parentDirection];

        if (dx != 0 && dy != 0) ExpandFromDiagonalParent(currentId, x, y, currentCost, generation, dx, dy, parentDirection);
        else if (dx != 0) ExpandFromHorizontalParent(currentId, x, y, currentCost, generation, dx, parentDirection);
        else ExpandFromVerticalParent(currentId, x, y, currentCost, generation, dy, parentDirection);
    }

    /// <summary>Arrived diagonally: continue, take the two components, and any forced side turn.</summary>
    private void ExpandFromDiagonalParent(int currentId, int x, int y, int costSoFar, int generation, int dx, int dy, int parentDirection)
    {
        if (IsWalkable(x, y + dy)) TryJumpDirection(currentId, x, y, costSoFar, generation, 0, dy, Grid.DirectionOf(0, dy));
        if (IsWalkable(x + dx, y)) TryJumpDirection(currentId, x, y, costSoFar, generation, dx, 0, Grid.DirectionOf(dx, 0));
        if (IsWalkable(x, y + dy) || IsWalkable(x + dx, y)) TryJumpDirection(currentId, x, y, costSoFar, generation, dx, dy, parentDirection);
        if (!IsWalkable(x - dx, y) && IsWalkable(x - dx, y + dy)) TryJumpDirection(currentId, x, y, costSoFar, generation, -dx, dy, Grid.DirectionOf(-dx, dy));
        if (!IsWalkable(x, y - dy) && IsWalkable(x + dx, y - dy)) TryJumpDirection(currentId, x, y, costSoFar, generation, dx, -dy, Grid.DirectionOf(dx, -dy));
    }

    /// <summary>Arrived horizontally: continue straight and take forced sidesteps around walls.</summary>
    private void ExpandFromHorizontalParent(int currentId, int x, int y, int costSoFar, int generation, int dx, int parentDirection)
    {
        if (IsWalkable(x + dx, y)) TryJumpDirection(currentId, x, y, costSoFar, generation, dx, 0, parentDirection);
        if (!IsWalkable(x, y + 1) && IsWalkable(x + dx, y + 1)) TryJumpDirection(currentId, x, y, costSoFar, generation, dx, 1, Grid.DirectionOf(dx, 1));
        if (!IsWalkable(x, y - 1) && IsWalkable(x + dx, y - 1)) TryJumpDirection(currentId, x, y, costSoFar, generation, dx, -1, Grid.DirectionOf(dx, -1));
    }

    /// <summary>Arrived vertically: continue straight and take forced sidesteps around walls.</summary>
    private void ExpandFromVerticalParent(int currentId, int x, int y, int costSoFar, int generation, int dy, int parentDirection)
    {
        if (IsWalkable(x, y + dy)) TryJumpDirection(currentId, x, y, costSoFar, generation, 0, dy, parentDirection);
        if (!IsWalkable(x + 1, y) && IsWalkable(x + 1, y + dy)) TryJumpDirection(currentId, x, y, costSoFar, generation, 1, dy, Grid.DirectionOf(1, dy));
        if (!IsWalkable(x - 1, y) && IsWalkable(x - 1, y + dy)) TryJumpDirection(currentId, x, y, costSoFar, generation, -1, dy, Grid.DirectionOf(-1, dy));
    }

    /// <summary>
    /// Checks the first step of a candidate direction, scans for the next jump point and, when one
    /// is found, relaxes it into the open list.
    /// </summary>
    private void TryJumpDirection(int parentId, int x, int y, int costSoFar, int generation, int dx, int dy, int direction)
    {
        int neighborX = x + dx;
        int neighborY = y + dy;
        if (!IsWalkable(neighborX, neighborY)) return;

        // A diagonal first step needs at least one walkable orthogonal neighbour.
        if (dx != 0 && dy != 0 && !IsWalkable(x + dx, y) && !IsWalkable(x, y + dy)) return;

        bool found;
        int jumpX, jumpY;
        if (dy == 0) found = JumpStraight(neighborX, neighborY, dx, 0, out jumpX, out jumpY);
        else if (dx == 0) found = JumpStraight(neighborX, neighborY, 0, dy, out jumpX, out jumpY);
        else found = JumpDiagonal(neighborX, neighborY, dx, dy, out jumpX, out jumpY);
        if (!found) return;

        int jumpId = jumpY * Width + jumpX;
        if (IsClosed(jumpId, generation)) return;

        int newCost = costSoFar + Grid.Octile(jumpX - x, jumpY - y);
        if (HasCost(jumpId, generation) && newCost >= CostToReach[jumpId]) return;

        CostToReach[jumpId] = newCost;
        CostGeneration[jumpId] = generation;
        Parent[jumpId] = parentId;
        _parentDirection[jumpId] = (byte)direction;
        _openList.Push(newCost + Heuristic(jumpX, jumpY, 0), jumpId);
    }

    /// <summary>
    /// Scans straight until a forced neighbour, the goal or a wall. A forced neighbour exists when
    /// a wall beside the line opens into a walkable diagonal cell behind it.
    /// </summary>
    private bool JumpStraight(int startX, int startY, int dx, int dy, out int jumpX, out int jumpY)
    {
        int x = startX;
        int y = startY;
        while (true)
        {
            if (!IsWalkable(x, y)) { jumpX = 0; jumpY = 0; return false; }
            if (x == GoalX && y == GoalY) { jumpX = x; jumpY = y; return true; }

            if (dx != 0)
            {
                if (IsWalkable(x + dx, y + 1) && !IsWalkable(x, y + 1)) { jumpX = x; jumpY = y; return true; }
                if (IsWalkable(x + dx, y - 1) && !IsWalkable(x, y - 1)) { jumpX = x; jumpY = y; return true; }
            }
            else
            {
                if (IsWalkable(x + 1, y + dy) && !IsWalkable(x + 1, y)) { jumpX = x; jumpY = y; return true; }
                if (IsWalkable(x - 1, y + dy) && !IsWalkable(x - 1, y)) { jumpX = x; jumpY = y; return true; }
            }

            x += dx;
            y += dy;
        }
    }

    /// <summary>
    /// Scans diagonally until a forced neighbour, the goal, a jump point found by either straight
    /// component scan, or a wall.
    /// </summary>
    private bool JumpDiagonal(int startX, int startY, int dx, int dy, out int jumpX, out int jumpY)
    {
        int x = startX;
        int y = startY;
        while (true)
        {
            if (!IsWalkable(x, y)) { jumpX = 0; jumpY = 0; return false; }
            if (x == GoalX && y == GoalY) { jumpX = x; jumpY = y; return true; }

            // Forced neighbours for a diagonal move.
            if (IsWalkable(x - dx, y + dy) && !IsWalkable(x - dx, y)) { jumpX = x; jumpY = y; return true; }
            if (IsWalkable(x + dx, y - dy) && !IsWalkable(x, y - dy)) { jumpX = x; jumpY = y; return true; }

            // A jump point in either component direction also makes this cell a jump point.
            if (JumpStraight(x + dx, y, dx, 0, out _, out _)) { jumpX = x; jumpY = y; return true; }
            if (JumpStraight(x, y + dy, 0, dy, out _, out _)) { jumpX = x; jumpY = y; return true; }

            // Continue diagonally, enforcing the movement model.
            int nextX = x + dx;
            int nextY = y + dy;
            if (!IsWalkable(nextX, nextY)) { jumpX = 0; jumpY = 0; return false; }
            if (!IsWalkable(nextX, y) && !IsWalkable(x, nextY)) { jumpX = 0; jumpY = 0; return false; }
            x = nextX;
            y = nextY;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsWalkable(int x, int y) =>
        (uint)x < (uint)Width && (uint)y < (uint)Height && Cells[y * Width + x] != Grid.Blocked;
}
