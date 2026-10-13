using System.Runtime.CompilerServices;

namespace Auxil.Pathfinding.Engine;

/// <summary>
/// HPA* (Botea, Müller and Schaeffer) over fixed-size square clusters per layer.
///
/// Preprocessing partitions the map into clusters and finds "entrances": walkable cells with a
/// walkable neighbor in a different cluster, including vertical ramp moves between layers.
/// Adjacent entrance pairs across a cluster border become abstract edges. Costs between two
/// entrances of the same cluster come from a cluster-bounded Dijkstra, precomputed for all pairs
/// when the table fits a memory budget and otherwise truncated to each entrance's nearest K.
///
/// A query runs A* on the abstract graph (seeded from the start and terminated at the goal
/// cluster) and reports the abstract route cost. HPA* is not guaranteed optimal; the benchmark
/// reports its cost ratio instead of asserting it.
/// </summary>
internal sealed class HpaStar : IPathFinder
{
    private const int TruncatedNeighborsPerEntrance = 12;
    private const long IntraEdgeBudget = 24_000_000;

    private readonly int _clusterSize;
    private Grid _grid = null!;
    private byte[] _cells = null!;
    private int _width, _height, _layerSize, _layerCount, _nodeCount;
    private int _clusterColumns, _clusterRows;

    // Cluster layout.
    private int[] _clusterOfCell = null!;

    // Entrances.
    private int[] _entranceIndexByCell = null!;
    private int[] _entranceCells = null!;
    private int _entranceCount;

    // Cross-border edges between entrances of adjacent clusters (CSR).
    private int[] _crossEdgeStart = null!;
    private int[] _crossEdgeTarget = null!;
    private int[] _crossEdgeCost = null!;

    // Entrances per cluster (CSR).
    private int[] _clusterEntranceStart = null!;
    private int[] _clusterEntrances = null!;

    // Intra-cluster edges between entrances (CSR), either complete or truncated.
    private bool _usePrecomputedIntraEdges;
    private int[] _intraEdgeStart = null!;
    private int[] _intraEdgeTarget = null!;
    private int[] _intraEdgeCost = null!;
    private int[] _selectionCosts = null!;
    private int[] _selectionEntrances = null!;

    // Query scratch: cluster-bounded Dijkstra.
    private int[] _localCost = null!;
    private int[] _localGeneration = null!;
    private int[] _localClosedGeneration = null!;
    private int[] _localParent = null!;
    private int _localSearchGeneration;
    private DialBucketsLazy _localQueue;

    // Query scratch: abstract search.
    private int[] _abstractCost = null!;
    private int[] _abstractCostGeneration = null!;
    private int[] _abstractFScore = null!;
    private int[] _abstractOpenGeneration = null!;
    private int[] _abstractClosedGeneration = null!;
    private int[] _abstractParent = null!;
    private int _abstractSearchGeneration;
    private int _startCell, _startCluster, _goalCluster, _goalCell;
    private int _bestRouteCost;
    private int _bestEndEntrance;
    private bool _directSameCluster;
    private bool _hasResult;
    private long _abstractExpansions;
    private DialHeapIndexed _abstractQueue;

    // Route reconstruction scratch.
    private List<int> _localPathScratch = null!;
    private List<int> _abstractChainScratch = null!;

    public HpaStar(int clusterSize)
    {
        _clusterSize = clusterSize;
    }

    public void Init(Grid grid)
    {
        _grid = grid;
        _cells = grid.Cells;
        _width = grid.Width;
        _height = grid.Height;
        _layerSize = grid.LayerSize;
        _layerCount = grid.Layers;
        _nodeCount = grid.Size;
        _clusterColumns = (_width + _clusterSize - 1) / _clusterSize;
        _clusterRows = (_height + _clusterSize - 1) / _clusterSize;

        BuildClusterLayout();
        FindEntrances();
        BuildClusterEntranceLists();
        BuildCrossClusterEdges();
        AllocateQueryScratch();
        PrecomputeIntraEdges();
    }

    public void Release()
    {
        _clusterOfCell = null!;
        _entranceIndexByCell = null!; _entranceCells = null!;
        _crossEdgeStart = null!; _crossEdgeTarget = null!; _crossEdgeCost = null!;
        _clusterEntranceStart = null!; _clusterEntrances = null!;
        _intraEdgeStart = null!; _intraEdgeTarget = null!; _intraEdgeCost = null!;
        _selectionCosts = null!; _selectionEntrances = null!;
        _localCost = null!; _localGeneration = null!; _localClosedGeneration = null!; _localParent = null!;
        _abstractCost = null!; _abstractCostGeneration = null!; _abstractFScore = null!;
        _abstractOpenGeneration = null!; _abstractClosedGeneration = null!; _abstractParent = null!;
        _localPathScratch = null!; _abstractChainScratch = null!;
    }

    // ------------------------------------------------------------------ preprocessing

    private void BuildClusterLayout()
    {
        _clusterOfCell = new int[_nodeCount];
        for (int cell = 0; cell < _nodeCount; cell++)
        {
            int x = cell % _width;
            int y = (cell % _layerSize) / _width;
            int z = cell / _layerSize;
            _clusterOfCell[cell] = (z * _clusterRows + y / _clusterSize) * _clusterColumns + x / _clusterSize;
        }
    }

    private void FindEntrances()
    {
        _entranceIndexByCell = new int[_nodeCount];
        Array.Fill(_entranceIndexByCell, -1);

        var entrances = new List<int>();
        for (int cell = 0; cell < _nodeCount; cell++)
        {
            if (_cells[cell] == Grid.Blocked) continue;
            if (!HasCrossClusterNeighbor(cell)) continue;
            _entranceIndexByCell[cell] = entrances.Count;
            entrances.Add(cell);
        }

        _entranceCells = entrances.ToArray();
        _entranceCount = _entranceCells.Length;
    }

    private void BuildClusterEntranceLists()
    {
        int clusterCount = _clusterColumns * _clusterRows * _layerCount;
        var counts = new int[clusterCount];
        for (int entrance = 0; entrance < _entranceCount; entrance++)
            counts[_clusterOfCell[_entranceCells[entrance]]]++;

        _clusterEntranceStart = new int[clusterCount + 1];
        for (int cluster = 0; cluster < clusterCount; cluster++)
            _clusterEntranceStart[cluster + 1] = _clusterEntranceStart[cluster] + counts[cluster];

        _clusterEntrances = new int[_entranceCount];
        var cursors = new int[clusterCount];
        Array.Copy(_clusterEntranceStart, cursors, clusterCount);
        for (int entrance = 0; entrance < _entranceCount; entrance++)
        {
            int cluster = _clusterOfCell[_entranceCells[entrance]];
            _clusterEntrances[cursors[cluster]++] = entrance;
        }
    }

    private void BuildCrossClusterEdges()
    {
        var degrees = new int[_entranceCount];
        for (int entrance = 0; entrance < _entranceCount; entrance++)
            degrees[entrance] = CountCrossClusterNeighbors(_entranceCells[entrance]);

        _crossEdgeStart = new int[_entranceCount + 1];
        for (int entrance = 0; entrance < _entranceCount; entrance++)
            _crossEdgeStart[entrance + 1] = _crossEdgeStart[entrance] + degrees[entrance];

        int edgeCount = _crossEdgeStart[_entranceCount];
        _crossEdgeTarget = new int[edgeCount];
        _crossEdgeCost = new int[edgeCount];

        var cursors = new int[_entranceCount];
        Array.Copy(_crossEdgeStart, cursors, _entranceCount);
        for (int entrance = 0; entrance < _entranceCount; entrance++)
        {
            int cell = _entranceCells[entrance];
            int x = cell % _width;
            int y = (cell % _layerSize) / _width;
            int z = cell / _layerSize;
            int cluster = _clusterOfCell[cell];
            int directionCount = _layerCount > 1 ? Grid.DirectionCount : Grid.InPlaneDirectionCount;

            for (int direction = 0; direction < directionCount; direction++)
            {
                if (!_grid.TryStep(cell, x, y, z, direction, out int neighborId)) continue;
                if (_clusterOfCell[neighborId] == cluster) continue;
                _crossEdgeTarget[cursors[entrance]] = _entranceIndexByCell[neighborId];
                _crossEdgeCost[cursors[entrance]] = Grid.MoveCost[direction];
                cursors[entrance]++;
            }
        }
    }

    private void AllocateQueryScratch()
    {
        _localCost = new int[_nodeCount];
        _localGeneration = new int[_nodeCount];
        _localClosedGeneration = new int[_nodeCount];
        _localParent = new int[_nodeCount];
        _localSearchGeneration = 0;

        _abstractCost = new int[_entranceCount];
        _abstractCostGeneration = new int[_entranceCount];
        _abstractFScore = new int[_entranceCount];
        _abstractOpenGeneration = new int[_entranceCount];
        _abstractClosedGeneration = new int[_entranceCount];
        _abstractParent = new int[_entranceCount];
        _abstractSearchGeneration = 0;

        _localPathScratch = new List<int>(1024);
        _abstractChainScratch = new List<int>(64);

        _localQueue = new DialBucketsLazy(1024);
        _localQueue.Init(_grid);
        _abstractQueue = new DialHeapIndexed(Math.Max(64, _entranceCount), 1);
        _abstractQueue.Init(_grid);
    }

    /// <summary>
    /// Precomputes entrance-to-entrance costs inside every cluster. When the complete table fits
    /// the memory budget, all pairs are stored; otherwise each entrance keeps its nearest K
    /// entrances, which keeps HPA* usable on very large sparse maps.
    /// </summary>
    private void PrecomputeIntraEdges()
    {
        int clusterCount = _clusterColumns * _clusterRows * _layerCount;
        long pairCount = 0;
        for (int cluster = 0; cluster < clusterCount; cluster++)
        {
            long entrances = _clusterEntranceStart[cluster + 1] - _clusterEntranceStart[cluster];
            pairCount += entrances * (entrances - 1);
        }

        if (pairCount <= IntraEdgeBudget)
        {
            PrecomputeAllIntraEdges(clusterCount);
            return;
        }

        PrecomputeTruncatedIntraEdges(clusterCount);
    }

    private void PrecomputeAllIntraEdges(int clusterCount)
    {
        _usePrecomputedIntraEdges = true;
        _intraEdgeStart = new int[_entranceCount + 1];
        int cursor = 0;
        for (int entrance = 0; entrance < _entranceCount; entrance++)
        {
            int cluster = _clusterOfCell[_entranceCells[entrance]];
            _intraEdgeStart[entrance] = cursor;
            cursor += _clusterEntranceStart[cluster + 1] - _clusterEntranceStart[cluster] - 1;
        }
        _intraEdgeStart[_entranceCount] = cursor;
        _intraEdgeTarget = new int[cursor];
        _intraEdgeCost = new int[cursor];

        // Costs are symmetric, so one Dijkstra per entrance fills both directions of each pair.
        var writeCursors = new int[_entranceCount];
        Array.Copy(_intraEdgeStart, writeCursors, _entranceCount);

        for (int cluster = 0; cluster < clusterCount; cluster++)
        {
            int first = _clusterEntranceStart[cluster];
            int last = _clusterEntranceStart[cluster + 1];
            if (last - first <= 1) continue;

            for (int i = first; i < last; i++)
            {
                int entrance = _clusterEntrances[i];
                RunLocalDijkstra(_entranceCells[entrance], cluster, out int generation);
                for (int j = i + 1; j < last; j++)
                {
                    int other = _clusterEntrances[j];
                    int cost = LocalCostAt(_entranceCells[other], generation);
                    if (cost == int.MaxValue) continue;
                    _intraEdgeTarget[writeCursors[entrance]] = other;
                    _intraEdgeCost[writeCursors[entrance]] = cost;
                    writeCursors[entrance]++;
                    _intraEdgeTarget[writeCursors[other]] = entrance;
                    _intraEdgeCost[writeCursors[other]] = cost;
                    writeCursors[other]++;
                }
            }
        }

        // Unreachable pairs leave slots empty; mark them with an infinite sentinel.
        for (int entrance = 0; entrance < _entranceCount; entrance++)
        {
            for (int slot = writeCursors[entrance]; slot < _intraEdgeStart[entrance + 1]; slot++)
            {
                _intraEdgeTarget[slot] = -1;
                _intraEdgeCost[slot] = int.MaxValue / 4;
            }
        }
    }

    private void PrecomputeTruncatedIntraEdges(int clusterCount)
    {
        long edgeCount = (long)_entranceCount * TruncatedNeighborsPerEntrance;
        if (edgeCount > int.MaxValue - 16)
        {
            _usePrecomputedIntraEdges = false; // fall back to a Dijkstra per expansion
            return;
        }

        _usePrecomputedIntraEdges = true;
        _intraEdgeStart = new int[_entranceCount + 1];
        for (int entrance = 0; entrance <= _entranceCount; entrance++)
            _intraEdgeStart[entrance] = entrance * TruncatedNeighborsPerEntrance;
        _intraEdgeTarget = new int[(int)edgeCount];
        _intraEdgeCost = new int[(int)edgeCount];
        Array.Fill(_intraEdgeTarget, -1);
        Array.Fill(_intraEdgeCost, int.MaxValue / 4);
        _selectionCosts = new int[TruncatedNeighborsPerEntrance + 1];
        _selectionEntrances = new int[TruncatedNeighborsPerEntrance + 1];

        for (int cluster = 0; cluster < clusterCount; cluster++)
        {
            int first = _clusterEntranceStart[cluster];
            int last = _clusterEntranceStart[cluster + 1];
            if (last - first <= 1) continue;

            for (int i = first; i < last; i++)
            {
                int entrance = _clusterEntrances[i];
                RunLocalDijkstra(_entranceCells[entrance], cluster, out int generation);

                // Keep the nearest K entrances (insertion selection, K is tiny).
                int count = 0;
                for (int j = first; j < last; j++)
                {
                    if (i == j) continue;
                    int other = _clusterEntrances[j];
                    int cost = LocalCostAt(_entranceCells[other], generation);
                    if (cost == int.MaxValue) continue;

                    if (count < TruncatedNeighborsPerEntrance)
                    {
                        int position = count++;
                        while (position > 0 && _selectionCosts[position - 1] > cost)
                        {
                            _selectionCosts[position] = _selectionCosts[position - 1];
                            _selectionEntrances[position] = _selectionEntrances[position - 1];
                            position--;
                        }
                        _selectionCosts[position] = cost;
                        _selectionEntrances[position] = other;
                    }
                    else if (cost < _selectionCosts[TruncatedNeighborsPerEntrance - 1])
                    {
                        int position = TruncatedNeighborsPerEntrance - 1;
                        while (position > 0 && _selectionCosts[position - 1] > cost)
                        {
                            _selectionCosts[position] = _selectionCosts[position - 1];
                            _selectionEntrances[position] = _selectionEntrances[position - 1];
                            position--;
                        }
                        _selectionCosts[position] = cost;
                        _selectionEntrances[position] = other;
                    }
                }

                int write = _intraEdgeStart[entrance];
                for (int n = 0; n < count; n++)
                {
                    _intraEdgeTarget[write] = _selectionEntrances[n];
                    _intraEdgeCost[write] = _selectionCosts[n];
                    write++;
                }
            }
        }
    }

    private bool HasCrossClusterNeighbor(int cell)
    {
        int x = cell % _width;
        int y = (cell % _layerSize) / _width;
        int z = cell / _layerSize;
        int cluster = _clusterOfCell[cell];
        int directionCount = _layerCount > 1 ? Grid.DirectionCount : Grid.InPlaneDirectionCount;
        for (int direction = 0; direction < directionCount; direction++)
        {
            if (_grid.TryStep(cell, x, y, z, direction, out int neighborId) && _clusterOfCell[neighborId] != cluster) return true;
        }
        return false;
    }

    private int CountCrossClusterNeighbors(int cell)
    {
        int x = cell % _width;
        int y = (cell % _layerSize) / _width;
        int z = cell / _layerSize;
        int cluster = _clusterOfCell[cell];
        int count = 0;
        int directionCount = _layerCount > 1 ? Grid.DirectionCount : Grid.InPlaneDirectionCount;
        for (int direction = 0; direction < directionCount; direction++)
        {
            if (_grid.TryStep(cell, x, y, z, direction, out int neighborId) && _clusterOfCell[neighborId] != cluster) count++;
        }
        return count;
    }

    // ------------------------------------------------------------------ query

    public bool FindPath(int start, int goal, out int cost, out int expanded)
    {
        cost = 0;
        expanded = 0;
        _abstractExpansions = 0;
        _startCell = start;
        _directSameCluster = false;
        _bestEndEntrance = -1;
        _hasResult = false;
        if (start == goal)
        {
            _hasResult = true;
            return true;
        }

        _startCluster = _clusterOfCell[start];
        _goalCluster = _clusterOfCell[goal];
        _goalCell = goal;
        _bestRouteCost = int.MaxValue;

        int generation = ++_abstractSearchGeneration;
        if (generation == int.MaxValue)
        {
            Array.Clear(_abstractCostGeneration);
            Array.Clear(_abstractOpenGeneration);
            Array.Clear(_abstractClosedGeneration);
            generation = _abstractSearchGeneration = 1;
        }
        _abstractQueue.Clear();

        // Distances from the goal inside its own cluster, used to finish at the goal cluster.
        RunLocalDijkstra(goal, _goalCluster, out int goalLocalGeneration);

        if (_startCluster == _goalCluster)
        {
            int directCost = LocalCostAt(start, goalLocalGeneration);
            if (directCost != int.MaxValue)
            {
                _directSameCluster = true;
                _bestRouteCost = directCost;
                _hasResult = true;
                cost = directCost;
                return true;
            }
        }

        // Seed the abstract search with the entrances of the start cluster.
        RunLocalDijkstra(start, _startCluster, out int startLocalGeneration);
        for (int i = _clusterEntranceStart[_startCluster]; i < _clusterEntranceStart[_startCluster + 1]; i++)
        {
            int entrance = _clusterEntrances[i];
            int entranceCost = LocalCostAt(_entranceCells[entrance], startLocalGeneration);
            if (entranceCost != int.MaxValue) RelaxEntrance(entrance, entranceCost, generation, -1);
        }

        while (_abstractQueue.Count > 0)
        {
            int entrance = _abstractQueue.Pop(out int fScore);
            if (fScore >= _bestRouteCost) break; // dial pops in non-decreasing order
            if (_abstractClosedGeneration[entrance] == generation) continue;
            _abstractClosedGeneration[entrance] = generation;
            _abstractExpansions++;

            int entranceCost = _abstractCost[entrance];
            int cell = _entranceCells[entrance];
            int cluster = _clusterOfCell[cell];

            if (cluster == _goalCluster)
            {
                int costToGoal = LocalCostAt(cell, goalLocalGeneration);
                if (costToGoal != int.MaxValue)
                {
                    int candidate = entranceCost + costToGoal;
                    if (candidate < _bestRouteCost)
                    {
                        _bestRouteCost = candidate;
                        _bestEndEntrance = entrance;
                    }
                }
            }

            for (int slot = _crossEdgeStart[entrance]; slot < _crossEdgeStart[entrance + 1]; slot++)
                RelaxEntrance(_crossEdgeTarget[slot], entranceCost + _crossEdgeCost[slot], generation, entrance);

            if (_usePrecomputedIntraEdges)
            {
                for (int slot = _intraEdgeStart[entrance]; slot < _intraEdgeStart[entrance + 1]; slot++)
                {
                    int edgeCost = _intraEdgeCost[slot];
                    if (edgeCost >= int.MaxValue / 4) continue;
                    RelaxEntrance(_intraEdgeTarget[slot], entranceCost + edgeCost, generation, entrance);
                }
            }
            else
            {
                // Lazy fallback: one cluster-bounded Dijkstra per expansion.
                RunLocalDijkstra(cell, cluster, out int localGeneration);
                for (int i = _clusterEntranceStart[cluster]; i < _clusterEntranceStart[cluster + 1]; i++)
                {
                    int other = _clusterEntrances[i];
                    if (other == entrance) continue;
                    int stepCost = LocalCostAt(_entranceCells[other], localGeneration);
                    if (stepCost == int.MaxValue) continue;
                    RelaxEntrance(other, entranceCost + stepCost, generation, entrance);
                }
            }
        }

        expanded = (int)Math.Min(_abstractExpansions, int.MaxValue);
        if (_bestRouteCost == int.MaxValue) return false;
        _hasResult = true;
        cost = _bestRouteCost;
        return true;
    }

    /// <summary>
    /// Reconstructs the route of the last successful <see cref="FindPath"/> into
    /// <paramref name="path"/> as cell ids from start to goal. The route is rebuilt from the
    /// abstract chain plus cluster-bounded walks, so calling this repeats a few small local
    /// searches.
    /// </summary>
    internal void CollectPath(List<int> path)
    {
        ArgumentNullException.ThrowIfNull(path);
        path.Clear();
        if (!_hasResult)
        {
            return;
        }

        if (_startCell == _goalCell)
        {
            path.Add(_startCell);
            return;
        }

        if (_directSameCluster)
        {
            RunLocalDijkstra(_startCell, _startCluster, out _);
            path.Add(_startCell);
            AppendLocalPath(path, _startCell, _goalCell);
            return;
        }

        if (_bestEndEntrance < 0 || _bestRouteCost == int.MaxValue)
        {
            return;
        }

        // Walk the abstract chain back to a start-cluster seed.
        _abstractChainScratch.Clear();
        for (int entrance = _bestEndEntrance; entrance != -1; entrance = _abstractParent[entrance])
        {
            _abstractChainScratch.Add(entrance);
        }

        _abstractChainScratch.Reverse();

        path.Add(_startCell);
        RunLocalDijkstra(_startCell, _startCluster, out _);
        AppendLocalPath(path, _startCell, _entranceCells[_abstractChainScratch[0]]);

        for (int i = 0; i + 1 < _abstractChainScratch.Count; i++)
        {
            int from = _entranceCells[_abstractChainScratch[i]];
            int to = _entranceCells[_abstractChainScratch[i + 1]];
            if (_clusterOfCell[from] == _clusterOfCell[to])
            {
                RunLocalDijkstra(from, _clusterOfCell[from], out _);
                AppendLocalPath(path, from, to);
            }
            else
            {
                path.Add(to); // cross-border edges are single steps
            }
        }

        RunLocalDijkstra(_goalCell, _goalCluster, out _);
        AppendGoalRootedPath(path, _entranceCells[_abstractChainScratch[^1]]);
    }

    /// <summary>Appends the local path from <paramref name="from"/> to <paramref name="to"/> (both included).</summary>
    private void AppendLocalPath(List<int> path, int from, int to)
    {
        _localPathScratch.Clear();
        for (int cell = to; cell != -1; cell = _localParent[cell])
        {
            _localPathScratch.Add(cell);
            if (cell == from)
            {
                break;
            }
        }

        // The path already ends with `from`; emit everything after it, up to `to`.
        for (int i = _localPathScratch.Count - 2; i >= 0; i--)
        {
            path.Add(_localPathScratch[i]);
        }
    }

    /// <summary>Appends the tail of a goal-rooted local search: everything after <paramref name="from"/>, up to the goal.</summary>
    private void AppendGoalRootedPath(List<int> path, int from)
    {
        for (int cell = _localParent[from]; cell != -1; cell = _localParent[cell])
        {
            path.Add(cell);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int LocalCostAt(int cell, int generation) =>
        _localGeneration[cell] == generation ? _localCost[cell] : int.MaxValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RelaxEntrance(int entrance, int cost, int generation, int parent)
    {
        if (_abstractClosedGeneration[entrance] == generation) return;
        if (_abstractCostGeneration[entrance] == generation && cost >= _abstractCost[entrance]) return;

        _abstractCost[entrance] = cost;
        _abstractCostGeneration[entrance] = generation;
        _abstractParent[entrance] = parent;

        int cell = _entranceCells[entrance];
        int fScore = cost + _grid.Heuristic(cell % _width, (cell % _layerSize) / _width, cell / _layerSize, _goalCell % _width, (_goalCell % _layerSize) / _width, _goalCell / _layerSize);
        if (_abstractOpenGeneration[entrance] != generation)
        {
            _abstractOpenGeneration[entrance] = generation;
            _abstractFScore[entrance] = fScore;
            _abstractQueue.Insert(entrance, fScore);
        }
        else if (fScore < _abstractFScore[entrance])
        {
            _abstractFScore[entrance] = fScore;
            _abstractQueue.Decrease(entrance, fScore);
        }
    }

    /// <summary>
    /// Cluster-bounded Dijkstra from <paramref name="source"/>. Fills the local cost arrays for
    /// the cluster and returns the generation they were stamped with. Never leaves the cluster.
    /// </summary>
    private void RunLocalDijkstra(int source, int cluster, out int generation)
    {
        generation = ++_localSearchGeneration;
        if (generation == int.MaxValue)
        {
            Array.Clear(_localGeneration);
            Array.Clear(_localClosedGeneration);
            generation = _localSearchGeneration = 1;
        }

        _localCost[source] = 0;
        _localGeneration[source] = generation;
        _localParent[source] = -1;
        _localQueue.Clear();
        _localQueue.Push(0, source);

        int[] localCost = _localCost;
        int[] localGeneration = _localGeneration;
        int[] localClosed = _localClosedGeneration;

        while (_localQueue.Count > 0)
        {
            int current = _localQueue.Pop(out _);
            if (localClosed[current] == generation) continue;
            localClosed[current] = generation;
            int currentCost = localCost[current];

            int x = current % _width;
            int y = (current % _layerSize) / _width;
            int z = current / _layerSize;
            int directionCount = _layerCount > 1 ? Grid.DirectionCount : Grid.InPlaneDirectionCount;

            for (int direction = 0; direction < directionCount; direction++)
            {
                if (!_grid.TryStep(current, x, y, z, direction, out int neighborId)) continue;
                if (_clusterOfCell[neighborId] != cluster) continue;
                int newCost = currentCost + Grid.MoveCost[direction];
                if (localGeneration[neighborId] != generation || newCost < localCost[neighborId])
                {
                    localCost[neighborId] = newCost;
                    localGeneration[neighborId] = generation;
                    _localParent[neighborId] = current;
                    _localQueue.Push(newCost, neighborId);
                }
            }
        }
    }
}
