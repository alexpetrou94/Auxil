using System.Runtime.CompilerServices;

namespace Auxil.Pathfinding.Engine;

/// <summary>
/// JPS+ (Rabin; Harabor and Grastien) for uniform-cost, single-layer grids.
///
/// The whole jump-point graph is precomputed into one distance table per direction, stored as a
/// <see cref="short"/> per cell:
///   * positive: distance to the next jump point in that direction,
///   * negative: distance to the wall in that direction,
///   * zero: no information (blocked cell or immediate wall).
///
/// Preprocessing sweeps are linear in the map size. At runtime the search never scans rows or
/// columns; it reads the table, relaxes the landing jump point, and synthesizes "target jump
/// points" when a jump crosses the goal's row or column. An optional intermediate-pruning mode
/// (<c>intermediatePruning</c>) expands diagonal jump points in place instead of queueing them.
/// </summary>
internal sealed class JpsPlusSearch : GridSearchBase
{
    private const byte NoParentDirection = 255;

    // Direction indices into the table and the primary-jump-point flags.
    private const int East = 0;
    private const int North = 2;
    private const int West = 4;
    private const int South = 6;
    private const byte EastFlag = 1;
    private const byte NorthFlag = 2;
    private const byte WestFlag = 4;
    private const byte SouthFlag = 8;

    private readonly bool _intermediatePruning;

    /// <summary>Per-direction jump and wall distances: <c>[direction * NodeCount + cell]</c>.</summary>
    private short[] _distanceTable = null!;

    private byte[] _parentDirection = null!;
    private int[] _fScore = null!;
    private int[] _openGeneration = null!;
    private DialHeapIndexed _openList;

    public JpsPlusSearch(bool intermediatePruning = false)
    {
        _intermediatePruning = intermediatePruning;
    }

    public override void Init(Grid grid)
    {
        if (grid.Layers != 1) throw new InvalidOperationException("JPS+ is defined for single-layer grids.");
        InitializeSearchState(grid);
        _distanceTable = new short[Grid.DirectionCount * NodeCount];
        _parentDirection = new byte[NodeCount];
        _fScore = new int[NodeCount];
        _openGeneration = new int[NodeCount];
        _openList.Init(grid);
        Preprocess();
    }

    public override void Release()
    {
        ReleaseSearchState();
        _distanceTable = null!;
        _parentDirection = null!;
        _fScore = null!;
        _openGeneration = null!;
        _openList = default;
    }

    protected override void ClearGenerationArrays()
    {
        base.ClearGenerationArrays();
        Array.Clear(_parentDirection);
        Array.Clear(_openGeneration);
    }

    // ------------------------------------------------------------------ preprocessing

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int TableIndex(int direction, int cell) => direction * NodeCount + cell;

    private void Preprocess()
    {
        byte[] primaryJumpPoints = BuildPrimaryJumpPoints();
        var straightTables = BuildStraightTables(primaryJumpPoints);
        BuildDiagonalTables(primaryJumpPoints, straightTables);
    }

    /// <summary>
    /// Marks, for each straight direction, the cells that are primary jump points: cells whose
    /// wall configuration forces a turn. A single byte per cell carries the four flags.
    /// </summary>
    private byte[] BuildPrimaryJumpPoints()
    {
        var primary = new byte[NodeCount];
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int cell = y * Width + x;
                if (Cells[cell] == Grid.Blocked) continue;
                byte flags = 0;

                // East travel: a walkable cell diagonally ahead with a blocked cell beside it.
                if (x + 1 < Width)
                {
                    if (y + 1 < Height && Cells[cell + Width + 1] != Grid.Blocked && Cells[cell + Width] == Grid.Blocked) flags |= EastFlag;
                    if (y - 1 >= 0 && Cells[cell - Width + 1] != Grid.Blocked && Cells[cell - Width] == Grid.Blocked) flags |= EastFlag;
                }

                // West travel.
                if (x - 1 >= 0)
                {
                    if (y + 1 < Height && Cells[cell + Width - 1] != Grid.Blocked && Cells[cell + Width] == Grid.Blocked) flags |= WestFlag;
                    if (y - 1 >= 0 && Cells[cell - Width - 1] != Grid.Blocked && Cells[cell - Width] == Grid.Blocked) flags |= WestFlag;
                }

                // North travel (dy = +1).
                if (y + 1 < Height)
                {
                    if (x + 1 < Width && Cells[cell + Width + 1] != Grid.Blocked && Cells[cell + 1] == Grid.Blocked) flags |= NorthFlag;
                    if (x - 1 >= 0 && Cells[cell + Width - 1] != Grid.Blocked && Cells[cell - 1] == Grid.Blocked) flags |= NorthFlag;
                }

                // South travel (dy = -1).
                if (y - 1 >= 0)
                {
                    if (x + 1 < Width && Cells[cell - Width + 1] != Grid.Blocked && Cells[cell + 1] == Grid.Blocked) flags |= SouthFlag;
                    if (x - 1 >= 0 && Cells[cell - Width - 1] != Grid.Blocked && Cells[cell - 1] == Grid.Blocked) flags |= SouthFlag;
                }

                primary[cell] = flags;
            }
        }
        return primary;
    }

    /// <summary>
    /// Fills the four straight tables: for every cell, the distance to the next primary jump
    /// point in that direction, or to the wall when there is none.
    /// </summary>
    private (short[] East, short[] North, short[] West, short[] South) BuildStraightTables(byte[] primary)
    {
        var eastTable = new short[NodeCount];
        var northTable = new short[NodeCount];
        var westTable = new short[NodeCount];
        var southTable = new short[NodeCount];
        var wallDistance = new short[NodeCount];

        // East: sweep right to left.
        for (int y = 0; y < Height; y++)
        {
            for (int x = Width - 1; x >= 0; x--)
            {
                int cell = y * Width + x;
                if (Cells[cell] == Grid.Blocked) { eastTable[cell] = 0; wallDistance[cell] = 0; continue; }
                bool canStep = x + 1 < Width && Cells[cell + 1] != Grid.Blocked;
                int jumpDistance = canStep && (primary[cell + 1] & EastFlag) != 0 ? 1
                    : canStep && eastTable[cell + 1] > 0 ? eastTable[cell + 1] + 1 : 0;
                int wallSteps = canStep ? wallDistance[cell + 1] + 1 : 0;
                eastTable[cell] = (short)jumpDistance;
                wallDistance[cell] = (short)wallSteps;
                _distanceTable[TableIndex(East, cell)] = (short)(jumpDistance > 0 ? jumpDistance : -wallSteps);
            }
        }

        // West: sweep left to right.
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int cell = y * Width + x;
                if (Cells[cell] == Grid.Blocked) { westTable[cell] = 0; wallDistance[cell] = 0; continue; }
                bool canStep = x > 0 && Cells[cell - 1] != Grid.Blocked;
                int jumpDistance = canStep && (primary[cell - 1] & WestFlag) != 0 ? 1
                    : canStep && westTable[cell - 1] > 0 ? westTable[cell - 1] + 1 : 0;
                int wallSteps = canStep ? wallDistance[cell - 1] + 1 : 0;
                westTable[cell] = (short)jumpDistance;
                wallDistance[cell] = (short)wallSteps;
                _distanceTable[TableIndex(West, cell)] = (short)(jumpDistance > 0 ? jumpDistance : -wallSteps);
            }
        }

        // North (dy = +1): sweep top to bottom.
        for (int y = Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < Width; x++)
            {
                int cell = y * Width + x;
                if (Cells[cell] == Grid.Blocked) { northTable[cell] = 0; wallDistance[cell] = 0; continue; }
                bool canStep = y + 1 < Height && Cells[cell + Width] != Grid.Blocked;
                int jumpDistance = canStep && (primary[cell + Width] & NorthFlag) != 0 ? 1
                    : canStep && northTable[cell + Width] > 0 ? northTable[cell + Width] + 1 : 0;
                int wallSteps = canStep ? wallDistance[cell + Width] + 1 : 0;
                northTable[cell] = (short)jumpDistance;
                wallDistance[cell] = (short)wallSteps;
                _distanceTable[TableIndex(North, cell)] = (short)(jumpDistance > 0 ? jumpDistance : -wallSteps);
            }
        }

        // South (dy = -1): sweep bottom to top.
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int cell = y * Width + x;
                if (Cells[cell] == Grid.Blocked) { southTable[cell] = 0; wallDistance[cell] = 0; continue; }
                bool canStep = y > 0 && Cells[cell - Width] != Grid.Blocked;
                int jumpDistance = canStep && (primary[cell - Width] & SouthFlag) != 0 ? 1
                    : canStep && southTable[cell - Width] > 0 ? southTable[cell - Width] + 1 : 0;
                int wallSteps = canStep ? wallDistance[cell - Width] + 1 : 0;
                southTable[cell] = (short)jumpDistance;
                wallDistance[cell] = (short)wallSteps;
                _distanceTable[TableIndex(South, cell)] = (short)(jumpDistance > 0 ? jumpDistance : -wallSteps);
            }
        }

        return (eastTable, northTable, westTable, southTable);
    }

    /// <summary>
    /// Fills the four diagonal tables: distance to the next diagonal jump point (or the wall).
    /// A diagonal jump point is a cell whose diagonal move has a forced neighbour, or from which
    /// a straight jump exists in either component direction.
    /// </summary>
    private void BuildDiagonalTables(byte[] primary, (short[] East, short[] North, short[] West, short[] South) straight)
    {
        var diagonalJump = new short[NodeCount];
        var wallDistance = new short[NodeCount];

        // North-east (dx = +1, dy = +1): sweep top to bottom.
        for (int y = Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < Width; x++)
            {
                int cell = y * Width + x;
                if (Cells[cell] == Grid.Blocked || x + 1 >= Width || y + 1 >= Height) { diagonalJump[cell] = 0; wallDistance[cell] = 0; continue; }
                if (!CanStepNorthEast(cell)) { diagonalJump[cell] = 0; wallDistance[cell] = 0; continue; }
                int next = cell + Width + 1;
                bool isJumpPoint = IsDiagonalJumpPoint(next, 1, 1, straight.East, EastFlag, straight.North, NorthFlag, primary);
                int jumpDistance = isJumpPoint ? 1 : diagonalJump[next] > 0 ? diagonalJump[next] + 1 : 0;
                int wallSteps = wallDistance[next] + 1;
                diagonalJump[cell] = (short)jumpDistance;
                wallDistance[cell] = (short)wallSteps;
                _distanceTable[TableIndex(1, cell)] = (short)(jumpDistance > 0 ? jumpDistance : -wallSteps);
            }
        }

        // North-west (dx = -1, dy = +1): sweep top to bottom.
        for (int y = Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < Width; x++)
            {
                int cell = y * Width + x;
                if (Cells[cell] == Grid.Blocked || x - 1 < 0 || y + 1 >= Height) { diagonalJump[cell] = 0; wallDistance[cell] = 0; continue; }
                if (!CanStepNorthWest(cell)) { diagonalJump[cell] = 0; wallDistance[cell] = 0; continue; }
                int next = cell + Width - 1;
                bool isJumpPoint = IsDiagonalJumpPoint(next, -1, 1, straight.West, WestFlag, straight.North, NorthFlag, primary);
                int jumpDistance = isJumpPoint ? 1 : diagonalJump[next] > 0 ? diagonalJump[next] + 1 : 0;
                int wallSteps = wallDistance[next] + 1;
                diagonalJump[cell] = (short)jumpDistance;
                wallDistance[cell] = (short)wallSteps;
                _distanceTable[TableIndex(3, cell)] = (short)(jumpDistance > 0 ? jumpDistance : -wallSteps);
            }
        }

        // South-west (dx = -1, dy = -1): sweep bottom to top.
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int cell = y * Width + x;
                if (Cells[cell] == Grid.Blocked || x - 1 < 0 || y - 1 < 0) { diagonalJump[cell] = 0; wallDistance[cell] = 0; continue; }
                if (!CanStepSouthWest(cell)) { diagonalJump[cell] = 0; wallDistance[cell] = 0; continue; }
                int next = cell - Width - 1;
                bool isJumpPoint = IsDiagonalJumpPoint(next, -1, -1, straight.West, WestFlag, straight.South, SouthFlag, primary);
                int jumpDistance = isJumpPoint ? 1 : diagonalJump[next] > 0 ? diagonalJump[next] + 1 : 0;
                int wallSteps = wallDistance[next] + 1;
                diagonalJump[cell] = (short)jumpDistance;
                wallDistance[cell] = (short)wallSteps;
                _distanceTable[TableIndex(5, cell)] = (short)(jumpDistance > 0 ? jumpDistance : -wallSteps);
            }
        }

        // South-east (dx = +1, dy = -1): sweep bottom to top.
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int cell = y * Width + x;
                if (Cells[cell] == Grid.Blocked || x + 1 >= Width || y - 1 < 0) { diagonalJump[cell] = 0; wallDistance[cell] = 0; continue; }
                if (!CanStepSouthEast(cell)) { diagonalJump[cell] = 0; wallDistance[cell] = 0; continue; }
                int next = cell - Width + 1;
                bool isJumpPoint = IsDiagonalJumpPoint(next, 1, -1, straight.East, EastFlag, straight.South, SouthFlag, primary);
                int jumpDistance = isJumpPoint ? 1 : diagonalJump[next] > 0 ? diagonalJump[next] + 1 : 0;
                int wallSteps = wallDistance[next] + 1;
                diagonalJump[cell] = (short)jumpDistance;
                wallDistance[cell] = (short)wallSteps;
                _distanceTable[TableIndex(7, cell)] = (short)(jumpDistance > 0 ? jumpDistance : -wallSteps);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CanStepNorthEast(int cell) =>
        Cells[cell + Width + 1] != Grid.Blocked && (Cells[cell + 1] != Grid.Blocked || Cells[cell + Width] != Grid.Blocked);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CanStepNorthWest(int cell) =>
        Cells[cell + Width - 1] != Grid.Blocked && (Cells[cell - 1] != Grid.Blocked || Cells[cell + Width] != Grid.Blocked);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CanStepSouthWest(int cell) =>
        Cells[cell - Width - 1] != Grid.Blocked && (Cells[cell - 1] != Grid.Blocked || Cells[cell - Width] != Grid.Blocked);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CanStepSouthEast(int cell) =>
        Cells[cell - Width + 1] != Grid.Blocked && (Cells[cell + 1] != Grid.Blocked || Cells[cell - Width] != Grid.Blocked);

    /// <summary>Static diagonal jump point test at a cell for travel direction (dx, dy).</summary>
    private bool IsDiagonalJumpPoint(int cell, int dx, int dy, short[] straightA, byte primaryFlagA, short[] straightB, byte primaryFlagB, byte[] primary)
    {
        int x = cell % Width;
        int y = cell / Width;

        // Forced diagonal neighbours.
        int neighborX = x - dx;
        int neighborY = y + dy;
        if ((uint)neighborX < (uint)Width && (uint)neighborY < (uint)Height)
        {
            if (Cells[neighborY * Width + neighborX] != Grid.Blocked && Cells[y * Width + neighborX] == Grid.Blocked) return true;
        }
        neighborX = x + dx;
        neighborY = y - dy;
        if ((uint)neighborX < (uint)Width && (uint)neighborY < (uint)Height)
        {
            if (Cells[neighborY * Width + neighborX] != Grid.Blocked && Cells[neighborY * Width + x] == Grid.Blocked) return true;
        }

        // A straight jump must exist in either component direction. The straight scan checks the
        // component neighbour cell itself before stepping, so a primary jump point there counts.
        if (dx > 0)
        {
            if (x + 1 < Width)
            {
                int componentCell = cell + 1;
                if ((primary[componentCell] & primaryFlagA) != 0 || straightA[componentCell] > 0) return true;
            }
        }
        else
        {
            if (x > 0)
            {
                int componentCell = cell - 1;
                if ((primary[componentCell] & primaryFlagA) != 0 || straightA[componentCell] > 0) return true;
            }
        }
        if (dy > 0)
        {
            if (y + 1 < Height)
            {
                int componentCell = cell + Width;
                if ((primary[componentCell] & primaryFlagB) != 0 || straightB[componentCell] > 0) return true;
            }
        }
        else
        {
            if (y > 0)
            {
                int componentCell = cell - Width;
                if ((primary[componentCell] & primaryFlagB) != 0 || straightB[componentCell] > 0) return true;
            }
        }

        return false;
    }

    // ------------------------------------------------------------------ runtime search

    public override bool FindPath(int start, int goal, out int cost, out int expanded)
    {
        cost = 0;
        expanded = 0;
        if (start == goal) return true;

        int generation = StartSearch(start, goal);
        _parentDirection[start] = NoParentDirection;

        _openList.Clear();
        int startF = Heuristic(Grid.XOf(start), Grid.YOf(start), 0);
        _openGeneration[start] = generation;
        _fScore[start] = startF;
        _openList.Insert(start, startF);

        int expandedCount = 0;
        while (_openList.Count > 0)
        {
            int currentId = _openList.Pop(out _);
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
            for (int direction = 0; direction < Grid.InPlaneDirectionCount; direction++)
            {
                int dx = Grid.OffX[direction];
                int dy = Grid.OffY[direction];
                if (_intermediatePruning && (direction & 1) != 0)
                    TryNaturalDiagonal(currentId, x, y, currentCost, generation, dx, dy, direction);
                else
                    TryJumpDirection(currentId, x, y, currentCost, generation, dx, dy, direction);
            }
            return;
        }

        int parentDx = Grid.OffX[parentDirection];
        int parentDy = Grid.OffY[parentDirection];

        if (parentDx != 0 && parentDy != 0)
        {
            if (IsWalkable(x, y + parentDy)) TryJumpDirection(currentId, x, y, currentCost, generation, 0, parentDy, Grid.DirectionOf(0, parentDy));
            if (IsWalkable(x + parentDx, y)) TryJumpDirection(currentId, x, y, currentCost, generation, parentDx, 0, Grid.DirectionOf(parentDx, 0));
            if (IsWalkable(x, y + parentDy) || IsWalkable(x + parentDx, y))
            {
                if (_intermediatePruning)
                    TryNaturalDiagonal(currentId, x, y, currentCost, generation, parentDx, parentDy, parentDirection);
                else
                    TryJumpDirection(currentId, x, y, currentCost, generation, parentDx, parentDy, parentDirection);
            }
            if (!IsWalkable(x - parentDx, y) && IsWalkable(x - parentDx, y + parentDy))
                TryJumpDirection(currentId, x, y, currentCost, generation, -parentDx, parentDy, Grid.DirectionOf(-parentDx, parentDy));
            if (!IsWalkable(x, y - parentDy) && IsWalkable(x + parentDx, y - parentDy))
                TryJumpDirection(currentId, x, y, currentCost, generation, parentDx, -parentDy, Grid.DirectionOf(parentDx, -parentDy));
        }
        else if (parentDx != 0)
        {
            if (IsWalkable(x + parentDx, y)) TryJumpDirection(currentId, x, y, currentCost, generation, parentDx, 0, parentDirection);
            if (!IsWalkable(x, y + 1) && IsWalkable(x + parentDx, y + 1))
                TryJumpDirection(currentId, x, y, currentCost, generation, parentDx, 1, Grid.DirectionOf(parentDx, 1));
            if (!IsWalkable(x, y - 1) && IsWalkable(x + parentDx, y - 1))
                TryJumpDirection(currentId, x, y, currentCost, generation, parentDx, -1, Grid.DirectionOf(parentDx, -1));
        }
        else
        {
            if (IsWalkable(x, y + parentDy)) TryJumpDirection(currentId, x, y, currentCost, generation, 0, parentDy, parentDirection);
            if (!IsWalkable(x + 1, y) && IsWalkable(x + 1, y + parentDy))
                TryJumpDirection(currentId, x, y, currentCost, generation, 1, parentDy, Grid.DirectionOf(1, parentDy));
            if (!IsWalkable(x - 1, y) && IsWalkable(x - 1, y + parentDy))
                TryJumpDirection(currentId, x, y, currentCost, generation, -1, parentDy, Grid.DirectionOf(-1, parentDy));
        }
    }

    /// <summary>Reads the table for one step, relaxes goal crossings and the static landing jump point.</summary>
    private void TryJumpDirection(int parentId, int x, int y, int costSoFar, int generation, int dx, int dy, int direction)
    {
        int neighborX = x + dx;
        int neighborY = y + dy;
        if (!IsWalkable(neighborX, neighborY)) return;

        bool diagonal = dx != 0 && dy != 0;
        if (diagonal && !IsWalkable(x + dx, y) && !IsWalkable(x, y + dy)) return;

        int tableValue = _distanceTable[TableIndex(direction, y * Width + x)];
        int run = tableValue > 0 ? tableValue : -tableValue;
        if (run == 0) return;

        if (diagonal) RelaxDiagonalGoalCrossings(parentId, x, y, costSoFar, run, direction, generation);
        else RelaxStraightGoalCrossing(parentId, x, y, costSoFar, run, dx, dy, direction, generation);

        if (tableValue > 0)
        {
            int jumpX = x + tableValue * dx;
            int jumpY = y + tableValue * dy;
            RelaxJumpPoint(jumpY * Width + jumpX, jumpX, jumpY, costSoFar + tableValue * (diagonal ? 14 : 10), parentId, direction, generation);
        }
    }

    /// <summary>
    /// Intermediate-pruning variant: follows the diagonal chain of jump points in place, expanding
    /// each intermediate's canonical successors directly and never queueing the intermediates.
    /// </summary>
    private void TryNaturalDiagonal(int parentId, int x, int y, int costSoFar, int generation, int dx, int dy, int direction)
    {
        int currentX = x, currentY = y, currentCost = costSoFar, currentParent = parentId;
        while (true)
        {
            int tableValue = _distanceTable[TableIndex(direction, currentY * Width + currentX)];
            int run = tableValue > 0 ? tableValue : -tableValue;
            if (run == 0) return;

            RelaxDiagonalGoalCrossings(currentParent, currentX, currentY, currentCost, run, direction, generation);
            if (tableValue <= 0) return;

            int intermediateX = currentX + tableValue * dx;
            int intermediateY = currentY + tableValue * dy;
            int intermediateId = intermediateY * Width + intermediateX;
            int intermediateCost = currentCost + tableValue * 14;

            // The intermediate's canonical continuations, as if it were expanded with a diagonal parent.
            if (IsWalkable(intermediateX, intermediateY + dy))
                TryJumpDirection(intermediateId, intermediateX, intermediateY, intermediateCost, generation, 0, dy, Grid.DirectionOf(0, dy));
            if (IsWalkable(intermediateX + dx, intermediateY))
                TryJumpDirection(intermediateId, intermediateX, intermediateY, intermediateCost, generation, dx, 0, Grid.DirectionOf(dx, 0));
            if (!IsWalkable(intermediateX - dx, intermediateY) && IsWalkable(intermediateX - dx, intermediateY + dy))
                TryJumpDirection(intermediateId, intermediateX, intermediateY, intermediateCost, generation, -dx, dy, Grid.DirectionOf(-dx, dy));
            if (!IsWalkable(intermediateX, intermediateY - dy) && IsWalkable(intermediateX + dx, intermediateY - dy))
                TryJumpDirection(intermediateId, intermediateX, intermediateY, intermediateCost, generation, dx, -dy, Grid.DirectionOf(dx, -dy));

            currentX = intermediateX;
            currentY = intermediateY;
            currentCost = intermediateCost;
            currentParent = intermediateId;
        }
    }

    /// <summary>
    /// Goal shortcuts for a diagonal segment: the goal on the ray, or a turn where the segment
    /// crosses the goal's row or column. Mirrors the target-jump-point rule from Game AI Pro 2.
    /// </summary>
    private void RelaxDiagonalGoalCrossings(int parentId, int x, int y, int costSoFar, int run, int direction, int generation)
    {
        int dx = Grid.OffX[direction];
        int dy = Grid.OffY[direction];
        int columnSteps = (GoalX - x) * dx;
        int rowSteps = (GoalY - y) * dy;

        // (a) The goal lies exactly on the ray.
        if (columnSteps == rowSteps && columnSteps >= 1 && columnSteps <= run)
            RelaxJumpPoint(GoalId, GoalX, GoalY, costSoFar + columnSteps * 14, parentId, direction, generation);

        // (b) The segment crosses the goal's row; a straight jump from the turn cell may reach it.
        if (rowSteps >= 1 && rowSteps <= run)
        {
            int turnX = x + rowSteps * dx;
            int straightSteps = (GoalX - turnX) * dx;
            if (straightSteps >= 1 && IsWalkable(turnX + dx, GoalY))
            {
                int scanStartX = turnX + dx;
                int scanDistance = (GoalX - scanStartX) * dx;
                int reach = TableRun(Grid.DirectionOf(dx, 0), GoalY * Width + scanStartX);
                if (scanDistance >= 0 && scanDistance <= reach)
                    RelaxJumpPoint(GoalId, GoalX, GoalY, costSoFar + rowSteps * 14 + straightSteps * 10, parentId, direction, generation);
            }
        }

        // (c) The segment crosses the goal's column; symmetric to (b).
        if (columnSteps >= 1 && columnSteps <= run)
        {
            int turnY = y + columnSteps * dy;
            int straightSteps = (GoalY - turnY) * dy;
            if (straightSteps >= 1 && IsWalkable(GoalX, turnY + dy))
            {
                int scanStartY = turnY + dy;
                int scanDistance = (GoalY - scanStartY) * dy;
                int reach = TableRun(Grid.DirectionOf(0, dy), scanStartY * Width + GoalX);
                if (scanDistance >= 0 && scanDistance <= reach)
                    RelaxJumpPoint(GoalId, GoalX, GoalY, costSoFar + columnSteps * 14 + straightSteps * 10, parentId, direction, generation);
            }
        }
    }

    /// <summary>Goal shortcut for a straight segment: the goal lies ahead on the same row or column.</summary>
    private void RelaxStraightGoalCrossing(int parentId, int x, int y, int costSoFar, int run, int dx, int dy, int direction, int generation)
    {
        if (dx != 0)
        {
            if (GoalY != y) return;
            int steps = (GoalX - x) * dx;
            if (steps >= 1 && steps <= run)
                RelaxJumpPoint(GoalId, GoalX, GoalY, costSoFar + steps * 10, parentId, direction, generation);
        }
        else
        {
            if (GoalX != x) return;
            int steps = (GoalY - y) * dy;
            if (steps >= 1 && steps <= run)
                RelaxJumpPoint(GoalId, GoalX, GoalY, costSoFar + steps * 10, parentId, direction, generation);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int TableRun(int direction, int cell)
    {
        int value = _distanceTable[TableIndex(direction, cell)];
        return value > 0 ? value : -value;
    }

    private void RelaxJumpPoint(int cell, int x, int y, int cost, int parentId, int direction, int generation)
    {
        if (IsClosed(cell, generation)) return;
        if (HasCost(cell, generation) && cost >= CostToReach[cell]) return;

        CostToReach[cell] = cost;
        CostGeneration[cell] = generation;
        Parent[cell] = parentId;
        _parentDirection[cell] = (byte)direction;

        int fScore = cost + Grid.Octile(x - GoalX, y - GoalY);
        if (_openGeneration[cell] != generation)
        {
            _openGeneration[cell] = generation;
            _fScore[cell] = fScore;
            _openList.Insert(cell, fScore);
        }
        else if (fScore < _fScore[cell])
        {
            _fScore[cell] = fScore;
            _openList.Decrease(cell, fScore);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsWalkable(int x, int y) =>
        (uint)x < (uint)Width && (uint)y < (uint)Height && Cells[y * Width + x] != Grid.Blocked;
}
