using System.Runtime.CompilerServices;

namespace Auxil.Pathfinding.Engine;

/// <summary>
/// Flat layered grid used by every path finder in this project.
///
/// Cell values: <see cref="Blocked"/>, <see cref="Open"/> or <see cref="Ramp"/>.
///
/// Movement model (the single definition of what a legal step is):
///   * 8-way movement in-plane; straight cost 10, diagonal cost 14.
///   * A diagonal step is legal when the target is walkable AND at least one of the two
///     orthogonal neighbours is walkable. This forbids squeezing between two blocked
///     corners and is the model the jump-point searches assume.
///   * A vertical step (direction 8 = layer + 1, direction 9 = layer - 1) costs 10 and is
///     legal only when both the source and the target cell are ramps.
///   * Out-of-bounds counts as blocked.
///
/// Use <see cref="TryStep"/> in search loops; it implements exactly the model above.
/// </summary>
internal sealed class Grid
{
    /// <summary>Cell value for an impassable tile.</summary>
    public const byte Blocked = 0;

    /// <summary>Cell value for a normal walkable tile.</summary>
    public const byte Open = 1;

    /// <summary>Cell value for a ramp, which allows vertical movement between layers.</summary>
    public const byte Ramp = 2;

    // Direction 0 = E, 1 = NE, 2 = N, 3 = NW, 4 = W, 5 = SW, 6 = S, 7 = SE.
    // Directions 8 and 9 are vertical (layer + 1 and layer - 1) and have no in-plane offset.
    public static readonly int[] OffX = [1, 1, 0, -1, -1, -1, 0, 1, 0, 0];
    public static readonly int[] OffY = [0, 1, 1, 1, 0, -1, -1, -1, 0, 0];
    public static readonly int[] OffZ = [0, 0, 0, 0, 0, 0, 0, 0, 1, -1];
    public static readonly int[] MoveCost = [10, 14, 10, 14, 10, 14, 10, 14, 10, 10];

    /// <summary>Number of in-plane directions (E..SE).</summary>
    public const int InPlaneDirectionCount = 8;

    /// <summary>Number of directions including vertical steps.</summary>
    public const int DirectionCount = 10;

    private static readonly sbyte[] DirectionByOffset = BuildDirectionByOffset();

    private static sbyte[] BuildDirectionByOffset()
    {
        var map = new sbyte[9];
        for (int i = 0; i < map.Length; i++) map[i] = -1;
        for (int direction = 0; direction < InPlaneDirectionCount; direction++)
            map[(OffY[direction] + 1) * 3 + (OffX[direction] + 1)] = (sbyte)direction;
        return map;
    }

    public Grid(int width, int height, int layers, byte[] cells)
    {
        if (width <= 0 || height <= 0 || layers <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (cells.Length != width * height * layers) throw new ArgumentException("Cell count mismatch.", nameof(cells));
        Width = width;
        Height = height;
        Layers = layers;
        LayerSize = width * height;
        Size = width * height * layers;
        Cells = cells;
    }

    public int Width { get; }
    public int Height { get; }
    public int Layers { get; }

    /// <summary>Number of cells in one layer.</summary>
    public int LayerSize { get; }

    /// <summary>Total number of cells across all layers.</summary>
    public int Size { get; }

    public byte[] Cells { get; }

    /// <summary>Flat index of the cell at (x, y, z).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Index(int x, int y, int z) => z * LayerSize + y * Width + x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int XOf(int id) => id % Width;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int YOf(int id) => (id % LayerSize) / Width;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ZOf(int id) => id / LayerSize;

    /// <summary>
    /// Returns the id of the cell reached from (x, y, z) in <paramref name="direction"/>.
    /// Performs no legality checks; use <see cref="TryStep"/> when the move may be illegal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int NeighborId(int currentId, int direction) =>
        direction < InPlaneDirectionCount
            ? currentId + OffY[direction] * Width + OffX[direction]
            : currentId + OffZ[direction] * LayerSize;

    /// <summary>
    /// Tests the movement model for one step from (x, y, z) in <paramref name="direction"/>.
    /// On success, <paramref name="neighborId"/> holds the target cell.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryStep(int currentId, int x, int y, int z, int direction, out int neighborId)
    {
        neighborId = -1;

        if (direction >= InPlaneDirectionCount)
        {
            if (Layers == 1 || Cells[currentId] != Ramp) return false;
            int neighborZ = z + OffZ[direction];
            if ((uint)neighborZ >= (uint)Layers) return false;
            int verticalId = currentId + OffZ[direction] * LayerSize;
            if (Cells[verticalId] != Ramp) return false;
            neighborId = verticalId;
            return true;
        }

        int neighborX = x + OffX[direction];
        int neighborY = y + OffY[direction];
        if ((uint)neighborX >= (uint)Width || (uint)neighborY >= (uint)Height) return false;

        int id = currentId + OffY[direction] * Width + OffX[direction];
        if (Cells[id] == Blocked) return false;

        // A diagonal step needs at least one walkable orthogonal neighbour.
        if ((direction & 1) != 0 &&
            Cells[currentId + OffX[direction]] == Blocked &&
            Cells[currentId + OffY[direction] * Width] == Blocked)
        {
            return false;
        }

        neighborId = id;
        return true;
    }

    /// <summary>Maps a unit offset (dx, dy) to a direction, or -1 when it is not one.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int DirectionOf(int dx, int dy) => DirectionByOffset[(dy + 1) * 3 + (dx + 1)];

    /// <summary>The direction that walks back along <paramref name="direction"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int OppositeDirection(int direction) =>
        direction < InPlaneDirectionCount ? (direction + 4) & 7 : (direction == 8 ? 9 : 8);

    /// <summary>
    /// The direction that moves from one cell to another, or -1 when they are not neighbours.
    /// Handles vertical moves between layers as well.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int DirectionBetween(int fromId, int toId)
    {
        int dx = XOf(toId) - XOf(fromId);
        int dy = YOf(toId) - YOf(fromId);
        if (dx != 0 || dy != 0)
        {
            return DirectionOf(dx, dy);
        }

        int dz = ZOf(toId) - ZOf(fromId);
        return dz > 0 ? 8 : dz < 0 ? 9 : -1;
    }

    /// <summary>Octile distance: the exact cost of a straight-then-diagonal walk between two offsets.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Octile(int dx, int dy)
    {
        int ax = dx < 0 ? -dx : dx;
        int ay = dy < 0 ? -dy : dy;
        return ax > ay ? 10 * ax + 4 * ay : 10 * ay + 4 * ax;
    }

    /// <summary>Admissible, consistent heuristic between two cells (octile plus 10 per layer).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Heuristic(int fromX, int fromY, int fromZ, int toX, int toY, int toZ)
    {
        int layerDistance = fromZ - toZ;
        if (layerDistance < 0) layerDistance = -layerDistance;
        return Octile(fromX - toX, fromY - toY) + 10 * layerDistance;
    }
}
