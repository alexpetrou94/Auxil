using Auxil.Pathfinding.Engine;

namespace Auxil.Pathfinding;

/// <summary>
/// A walkable map addressed as (X, Y). Every tile starts walkable; block tiles to build walls,
/// and give tiles an extra cost for slower terrain such as mud or forest. The map is the only
/// thing a <see cref="Pathfinder"/> needs.
/// </summary>
public sealed class PathMap : IPathWorld
{
    private readonly byte[] _cells;
    private readonly int[] _extraCosts;
    private bool _hasTerrainCosts;

    /// <summary>Creates a map of the given size with every tile walkable.</summary>
    /// <param name="width">Number of tiles horizontally; must be positive.</param>
    /// <param name="height">Number of tiles vertically; must be positive.</param>
    public PathMap(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);

        _cells = new byte[width * height];
        Array.Fill(_cells, Grid.Open);
        _extraCosts = new int[width * height];
        _hasTerrainCosts = false;
        Grid = new Grid(width, height, 1, _cells);
    }

    private Grid Grid { get; }

    internal int[] ExtraCosts => _extraCosts;

    internal byte[] Cells => _cells;

    Grid IPathWorld.Grid => Grid;

    int[] IPathWorld.ExtraCosts => _extraCosts;

    int IPathWorld.Width => Width;

    int IPathWorld.Height => Height;

    int IPathWorld.LayerCount => 1;

    int IPathWorld.ToIndex(PathPoint point) => ToIndex(point);

    PathPoint IPathWorld.ToPoint(int index) => ToPoint(index);

    /// <summary>Number of tiles horizontally.</summary>
    public int Width => Grid.Width;

    /// <summary>Number of tiles vertically.</summary>
    public int Height => Grid.Height;

    /// <summary>True when at least one tile has an extra cost.</summary>
    public bool HasTerrainCosts => _hasTerrainCosts;

    /// <summary>True when the coordinates lie inside the map.</summary>
    /// <param name="x">Horizontal position.</param>
    /// <param name="y">Vertical position.</param>
    public bool Contains(int x, int y) => (uint)x < (uint)Width && (uint)y < (uint)Height;

    /// <summary>True when the point lies inside the map and on layer 0.</summary>
    /// <param name="point">The point to test.</param>
    public bool Contains(PathPoint point) => point.Layer == 0 && Contains(point.X, point.Y);

    /// <summary>True when the tile cannot be walked on.</summary>
    /// <param name="x">Horizontal position.</param>
    /// <param name="y">Vertical position.</param>
    public bool IsBlocked(int x, int y)
    {
        ValidateCoordinates(x, y);
        return _cells[y * Width + x] == Grid.Blocked;
    }

    /// <summary>True when the point cannot be walked on.</summary>
    /// <param name="point">The point to test.</param>
    public bool IsBlocked(PathPoint point)
    {
        ValidateLayer(point);
        return IsBlocked(point.X, point.Y);
    }

    /// <summary>Blocks or unblocks one tile.</summary>
    /// <param name="x">Horizontal position.</param>
    /// <param name="y">Vertical position.</param>
    /// <param name="blocked">True to block the tile, false to make it walkable again.</param>
    public void SetBlocked(int x, int y, bool blocked = true)
    {
        ValidateCoordinates(x, y);
        _cells[y * Width + x] = blocked ? Grid.Blocked : Grid.Open;
    }

    /// <summary>Blocks or unblocks a rectangle of tiles.</summary>
    /// <param name="x">Left edge of the rectangle.</param>
    /// <param name="y">Top edge of the rectangle.</param>
    /// <param name="width">Rectangle width in tiles; must be positive.</param>
    /// <param name="height">Rectangle height in tiles; must be positive.</param>
    /// <param name="blocked">True to block the tiles, false to make them walkable.</param>
    public void SetBlocked(int x, int y, int width, int height, bool blocked = true)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);
        ValidateCoordinates(x, y);
        ValidateCoordinates(x + width - 1, y + height - 1);

        byte value = blocked ? Grid.Blocked : Grid.Open;
        for (int row = y; row < y + height; row++)
        {
            int rowStart = row * Width;
            for (int column = x; column < x + width; column++)
            {
                _cells[rowStart + column] = value;
            }
        }
    }

    /// <summary>Extra cost added when entering this tile; 0 means normal ground.</summary>
    /// <param name="x">Horizontal position.</param>
    /// <param name="y">Vertical position.</param>
    public int GetExtraCost(int x, int y)
    {
        ValidateCoordinates(x, y);
        return _extraCosts[y * Width + x];
    }

    /// <summary>
    /// Sets an extra cost added when entering this tile. Use it for slower terrain: 5 makes a tile
    /// half again as expensive as normal ground (a straight step costs 10, a diagonal 14).
    /// Only the default algorithm supports terrain costs.
    /// </summary>
    /// <param name="x">Horizontal position.</param>
    /// <param name="y">Vertical position.</param>
    /// <param name="extraCost">Extra cost per entry; must not be negative.</param>
    public void SetExtraCost(int x, int y, int extraCost)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(extraCost);
        ValidateCoordinates(x, y);
        _extraCosts[y * Width + x] = extraCost;
        _hasTerrainCosts = true;
    }

    /// <summary>
    /// Builds a map from text rows, one string per row, where '#' is a wall and every other
    /// character is walkable ground. Handy for tests, demos and small hand-authored maps.
    /// </summary>
    /// <param name="rows">The rows, all the same length.</param>
    public static PathMap FromRows(IEnumerable<string> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        string[] lines = rows.ToArray();
        if (lines.Length == 0)
        {
            throw new ArgumentException("At least one row is required.", nameof(rows));
        }

        int width = lines[0].Length;
        if (width == 0)
        {
            throw new ArgumentException("Rows must not be empty.", nameof(rows));
        }

        var map = new PathMap(width, lines.Length);
        for (int y = 0; y < lines.Length; y++)
        {
            if (lines[y].Length != width)
            {
                throw new ArgumentException("All rows must have the same length.", nameof(rows));
            }

            for (int x = 0; x < width; x++)
            {
                if (lines[y][x] == '#')
                {
                    map.SetBlocked(x, y);
                }
            }
        }

        return map;
    }

    private int ToIndex(PathPoint point) => point.Y * Width + point.X;

    private PathPoint ToPoint(int index) => new(index % Width, index / Width);

    private void ValidateCoordinates(int x, int y)
    {
        if (!Contains(x, y))
        {
            throw new ArgumentOutOfRangeException(nameof(x), $"({x}, {y}) is outside the {Width}x{Height} map.");
        }
    }

    private static void ValidateLayer(PathPoint point)
    {
        if (point.Layer != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(point), "This map has no layers; use a stacked map for layer 1 and above.");
        }
    }
}
