using System.Runtime.CompilerServices;

namespace Auxil.Pathfinding.Engine;

/// <summary>
/// Flow field for crowds: one Dijkstra search from a goal paints an integration cost on every
/// cell plus a flow direction pointing one step closer to the goal. Building the field is O(map);
/// each agent then walks the field in O(path length) with no queue operations at all.
///
/// The field supports stacked maps: vertical steps between ramps are part of the movement model,
/// so agents walk through ramps like any other direction.
/// </summary>
internal sealed class FlowField
{
    private const byte NoFlow = 255;

    private Grid _grid = null!;
    private int _width, _layerSize, _layerCount, _nodeCount;

    private int[] _integrationCost = null!;
    private int[] _integrationGeneration = null!;
    private int[] _closedGeneration = null!;
    private byte[] _flowDirection = null!;
    private int[] _flowGeneration = null!;
    private int _generation;
    private DialBucketsLazy _queue;

    public void Init(Grid grid)
    {
        _grid = grid;
        _width = grid.Width;
        _layerSize = grid.LayerSize;
        _layerCount = grid.Layers;
        _nodeCount = grid.Size;

        _integrationCost = new int[_nodeCount];
        _integrationGeneration = new int[_nodeCount];
        _closedGeneration = new int[_nodeCount];
        _flowDirection = new byte[_nodeCount];
        _flowGeneration = new int[_nodeCount];
        _generation = 0;
        _queue.Init(grid);
    }

    public void Release()
    {
        _integrationCost = null!;
        _integrationGeneration = null!;
        _closedGeneration = null!;
        _flowDirection = null!;
        _flowGeneration = null!;
    }

    /// <summary>Builds the integration costs and flow directions toward <paramref name="goal"/>.</summary>
    public void ComputeField(int goal)
    {
        int generation = ++_generation;
        if (generation == int.MaxValue)
        {
            Array.Clear(_integrationGeneration);
            Array.Clear(_closedGeneration);
            Array.Clear(_flowGeneration);
            generation = _generation = 1;
        }

        _integrationCost[goal] = 0;
        _integrationGeneration[goal] = generation;
        _flowGeneration[goal] = generation;
        _flowDirection[goal] = NoFlow;
        _queue.Clear();
        _queue.Push(0, goal);

        int directionCount = _layerCount > 1 ? Grid.DirectionCount : Grid.InPlaneDirectionCount;
        int[] integrationCost = _integrationCost;
        int[] integrationGeneration = _integrationGeneration;
        int[] closedGeneration = _closedGeneration;
        byte[] flowDirection = _flowDirection;
        int[] flowGeneration = _flowGeneration;

        while (_queue.Count > 0)
        {
            int current = _queue.Pop(out int currentCost);
            if (closedGeneration[current] == generation) continue;
            closedGeneration[current] = generation;

            int x = current % _width;
            int y = (current % _layerSize) / _width;
            int z = current / _layerSize;

            for (int direction = 0; direction < directionCount; direction++)
            {
                if (!_grid.TryStep(current, x, y, z, direction, out int neighborId)) continue;
                if (closedGeneration[neighborId] == generation) continue;

                int newCost = currentCost + Grid.MoveCost[direction];
                if (integrationGeneration[neighborId] != generation || newCost < integrationCost[neighborId])
                {
                    integrationCost[neighborId] = newCost;
                    integrationGeneration[neighborId] = generation;
                    flowDirection[neighborId] = (byte)Grid.OppositeDirection(direction);
                    flowGeneration[neighborId] = generation;
                    _queue.Push(newCost, neighborId);
                }
            }
        }
    }

    /// <summary>Distance from the goal for a cell in the current field, or -1 when unreachable.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int DistanceAt(int cell) => _integrationGeneration[cell] == _generation ? _integrationCost[cell] : -1;

    /// <summary>
    /// Returns the next cell on the way to the goal for an agent standing on <paramref name="cell"/>,
    /// or false when the cell is not covered by the current field or is the goal itself.
    /// </summary>
    public bool TryGetNextCell(int cell, out int nextCell)
    {
        nextCell = -1;
        if (_flowGeneration[cell] != _generation)
        {
            return false;
        }

        int direction = _flowDirection[cell];
        if (direction == NoFlow)
        {
            return false;
        }

        nextCell = _grid.NeighborId(cell, direction);
        return true;
    }
}
