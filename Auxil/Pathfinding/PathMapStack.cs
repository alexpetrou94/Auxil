using Auxil.Pathfinding.Engine;

namespace Auxil.Pathfinding;

/// <summary>
/// Several 2D maps stacked into one world, for buildings with floors, caves under the surface or
/// bridges over ground. Add layers with <see cref="AddLayer"/> (all the same size) and connect
/// them tile by tile with <see cref="ConnectStairs"/>; a route can then move between layers only
/// through those connections.
///
/// <code>
/// var stack = new PathMapStack();
/// stack.AddLayer(groundFloor);
/// stack.AddLayer(basement);
/// stack.ConnectStairs(10, 4, lowerLayer: 0); // links (10, 4) on floor 0 and floor 1
///
/// using var finder = new Pathfinder(stack);
/// finder.TryFindPath(new PathPoint(0, 0, 0), new PathPoint(5, 5, 1), out PathResult route);
/// </code>
///
/// Layers are copied when added, so edit the stack (not the original maps) afterwards. Adding
/// layers or connecting stairs after creating a pathfinder is not supported; configure the stack
/// first, then search.
/// </summary>
public sealed class PathMapStack : IPathWorld
{
    private byte[] _cells = [];
    private int[] _extraCosts = [];
    private Grid? _grid;

    /// <summary>Number of layers added so far.</summary>
    public int LayerCount { get; private set; }

    /// <summary>Number of tiles horizontally; 0 until the first layer is added.</summary>
    public int Width { get; private set; }

    /// <summary>Number of tiles vertically; 0 until the first layer is added.</summary>
    public int Height { get; private set; }

    /// <summary>True when at least one tile on any layer has an extra cost.</summary>
    public bool HasTerrainCosts { get; private set; }

    /// <summary>
    /// Adds a map as the next layer, copying its tiles. The first layer fixes the size; every
    /// later layer must match it.
    /// </summary>
    /// <param name="map">The map to copy onto the top of the stack.</param>
    /// <returns>The index of the new layer, 0 for the first.</returns>
    public int AddLayer(PathMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        if (LayerCount == 0)
        {
            Width = map.Width;
            Height = map.Height;
        }
        else if (map.Width != Width || map.Height != Height)
        {
            throw new ArgumentException(
                $"The map is {map.Width}x{map.Height}, but the stack is {Width}x{Height}. All layers must match.",
                nameof(map));
        }

        int layerSize = Width * Height;
        var cells = new byte[_cells.Length + layerSize];
        var extraCosts = new int[_extraCosts.Length + layerSize];
        _cells.CopyTo(cells, 0);
        _extraCosts.CopyTo(extraCosts, 0);
        Array.Copy(map.Cells, 0, cells, _cells.Length, layerSize);
        Array.Copy(map.ExtraCosts, 0, extraCosts, _extraCosts.Length, layerSize);

        _cells = cells;
        _extraCosts = extraCosts;
        HasTerrainCosts |= map.HasTerrainCosts;

        int layerIndex = LayerCount;
        LayerCount = layerIndex + 1;
        _grid = new Grid(Width, Height, LayerCount, _cells);
        return layerIndex;
    }

    /// <summary>
    /// Connects a tile on <paramref name="lowerLayer"/> with the tile directly above it, so routes
    /// can move between those two layers there. Both tiles must be walkable and become stairs.
    /// </summary>
    /// <param name="x">Horizontal position of the stair tile.</param>
    /// <param name="y">Vertical position of the stair tile.</param>
    /// <param name="lowerLayer">The layer below; the stair also spans the layer above it.</param>
    public void ConnectStairs(int x, int y, int lowerLayer)
    {
        if (lowerLayer < 0 || lowerLayer >= LayerCount - 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lowerLayer),
                $"Layer {lowerLayer} has no layer above it; the stack has {LayerCount} layer(s).");
        }

        int lower = IndexOf(x, y, lowerLayer);
        int upper = IndexOf(x, y, lowerLayer + 1);
        if (_cells[lower] == Grid.Blocked || _cells[upper] == Grid.Blocked)
        {
            throw new InvalidOperationException("Both tiles must be walkable before they can be connected; unblock them first.");
        }

        _cells[lower] = Grid.Ramp;
        _cells[upper] = Grid.Ramp;
    }

    /// <summary>True when the point lies inside the stack.</summary>
    /// <param name="point">The point to test.</param>
    public bool Contains(PathPoint point) => Contains(point.X, point.Y, point.Layer);

    /// <summary>True when the coordinates lie inside the stack.</summary>
    /// <param name="x">Horizontal position.</param>
    /// <param name="y">Vertical position.</param>
    /// <param name="layer">Layer index.</param>
    public bool Contains(int x, int y, int layer) =>
        (uint)x < (uint)Width && (uint)y < (uint)Height && (uint)layer < (uint)LayerCount;

    /// <summary>True when the tile cannot be walked on.</summary>
    /// <param name="point">The point to test.</param>
    public bool IsBlocked(PathPoint point) => IsBlocked(point.X, point.Y, point.Layer);

    /// <summary>True when the tile cannot be walked on.</summary>
    /// <param name="x">Horizontal position.</param>
    /// <param name="y">Vertical position.</param>
    /// <param name="layer">Layer index.</param>
    public bool IsBlocked(int x, int y, int layer) => _cells[IndexOf(x, y, layer)] == Grid.Blocked;

    /// <summary>Blocks or unblocks one tile on one layer.</summary>
    /// <param name="point">The tile to change.</param>
    /// <param name="blocked">True to block the tile, false to make it walkable again.</param>
    public void SetBlocked(PathPoint point, bool blocked = true) => SetBlocked(point.X, point.Y, point.Layer, blocked);

    /// <summary>Blocks or unblocks one tile on one layer.</summary>
    /// <param name="x">Horizontal position.</param>
    /// <param name="y">Vertical position.</param>
    /// <param name="layer">Layer index.</param>
    /// <param name="blocked">True to block the tile, false to make it walkable again.</param>
    public void SetBlocked(int x, int y, int layer, bool blocked = true) =>
        _cells[IndexOf(x, y, layer)] = blocked ? Grid.Blocked : Grid.Open;

    /// <summary>Extra cost added when entering this tile; 0 means normal ground.</summary>
    /// <param name="point">The tile to read.</param>
    public int GetExtraCost(PathPoint point) => GetExtraCost(point.X, point.Y, point.Layer);

    /// <summary>Extra cost added when entering this tile; 0 means normal ground.</summary>
    /// <param name="x">Horizontal position.</param>
    /// <param name="y">Vertical position.</param>
    /// <param name="layer">Layer index.</param>
    public int GetExtraCost(int x, int y, int layer) => _extraCosts[IndexOf(x, y, layer)];

    /// <summary>
    /// Sets an extra cost added when entering this tile, for slower terrain. Only the default
    /// algorithm supports terrain costs.
    /// </summary>
    /// <param name="point">The tile to change.</param>
    /// <param name="extraCost">Extra cost per entry; must not be negative.</param>
    public void SetExtraCost(PathPoint point, int extraCost) => SetExtraCost(point.X, point.Y, point.Layer, extraCost);

    /// <summary>
    /// Sets an extra cost added when entering this tile, for slower terrain. Only the default
    /// algorithm supports terrain costs.
    /// </summary>
    /// <param name="x">Horizontal position.</param>
    /// <param name="y">Vertical position.</param>
    /// <param name="layer">Layer index.</param>
    /// <param name="extraCost">Extra cost per entry; must not be negative.</param>
    public void SetExtraCost(int x, int y, int layer, int extraCost)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(extraCost);
        _extraCosts[IndexOf(x, y, layer)] = extraCost;
        HasTerrainCosts = true;
    }

    Grid IPathWorld.Grid => _grid ?? throw new InvalidOperationException("The stack has no layers; call AddLayer first.");

    int[] IPathWorld.ExtraCosts => _extraCosts;

    int IPathWorld.Width => Width;

    int IPathWorld.Height => Height;

    int IPathWorld.LayerCount => LayerCount;

    int IPathWorld.ToIndex(PathPoint point) => IndexOf(point.X, point.Y, point.Layer);

    PathPoint IPathWorld.ToPoint(int index)
    {
        int layerSize = Width * Height;
        return new PathPoint(index % Width, index % layerSize / Width, index / layerSize);
    }

    private int IndexOf(int x, int y, int layer)
    {
        if (!Contains(x, y, layer))
        {
            throw new ArgumentOutOfRangeException(
                nameof(x),
                $"({x}, {y}, layer {layer}) is outside the {Width}x{Height} stack of {LayerCount} layer(s).");
        }

        return layer * Width * Height + y * Width + x;
    }
}
