using Auxil.Pathfinding.Engine;

namespace Auxil.Pathfinding;

/// <summary>
/// Finds routes on a <see cref="PathMap"/> or a <see cref="PathMapStack"/>. This is the type most
/// callers need:
///
/// <code>
/// var map = new PathMap(100, 80);
/// map.SetBlocked(10, 10);
/// var finder = new Pathfinder(map);
/// if (finder.TryFindPath(new PathPoint(0, 0), new PathPoint(90, 70), out PathResult route))
/// {
///     foreach (PathPoint step in route.Points) { }
/// }
/// </code>
///
/// Construction does all the preparation the chosen algorithm needs (JPS+ precomputes its
/// distance tables here). One instance handles one query at a time and is not thread-safe; create
/// one per thread or use <see cref="PathfinderPool"/>.
/// </summary>
public sealed class Pathfinder : IDisposable
{
    private readonly IPathWorld _world;
    private readonly IPathFinder _engine;
    private readonly List<int> _idBuffer = new(1024);
    private readonly List<int> _goalBuffer = new(16);
    private MultiGoalAStar? _multiGoalEngine;
    private bool _disposed;

    /// <summary>Creates a pathfinder for a single map.</summary>
    /// <param name="map">The map to search. It may change afterwards; the next query sees the change.</param>
    /// <param name="algorithm">The search engine. The default works everywhere and handles terrain costs.</param>
    public Pathfinder(PathMap map, PathfinderAlgorithm algorithm = PathfinderAlgorithm.Default)
        : this(WorldOf(map), algorithm)
    {
        Map = map;
    }

    /// <summary>Creates a pathfinder for a stack of maps.</summary>
    /// <param name="stack">The stacked maps to search. Add all layers and stairs before this call.</param>
    /// <param name="algorithm">The search engine. The default works everywhere and handles terrain costs.</param>
    public Pathfinder(PathMapStack stack, PathfinderAlgorithm algorithm = PathfinderAlgorithm.Default)
        : this(WorldOf(stack), algorithm)
    {
        Stack = stack;
    }

    private Pathfinder(IPathWorld world, PathfinderAlgorithm algorithm)
    {
        _world = world;
        ValidateAlgorithm(world, algorithm);

        Algorithm = algorithm;
        _engine = CreateEngine(world, algorithm);
        _engine.Init(world.Grid);
    }

    /// <summary>The map this pathfinder searches; null when it was created from a stack.</summary>
    public PathMap? Map { get; }

    /// <summary>The stack this pathfinder searches; null when it was created from a single map.</summary>
    public PathMapStack? Stack { get; }

    /// <summary>The search engine in use.</summary>
    public PathfinderAlgorithm Algorithm { get; }

    /// <summary>Behaviour knobs; smoothing is on by default.</summary>
    public PathfinderOptions Options { get; } = new();

    internal IPathWorld World => _world;

    internal IPathFinder Engine => _engine;

    internal WeightedAStar? SteppableEngine => _engine as WeightedAStar;

    internal bool Disposed => _disposed;

    /// <summary>
    /// Finds a route from start to goal. Never throws for bad input: out-of-bounds points and
    /// blocked tiles come back as a <see cref="PathResult"/> whose <see cref="PathResult.Status"/>
    /// explains what happened.
    /// </summary>
    /// <param name="start">Where the route begins.</param>
    /// <param name="goal">Where the route ends.</param>
    /// <param name="result">The outcome, including the route when one was found.</param>
    /// <returns>True when a route was found.</returns>
    public bool TryFindPath(PathPoint start, PathPoint goal, out PathResult result)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_world.Contains(start) || !_world.Contains(goal))
        {
            result = PathResult.Failure(PathStatus.Invalid, goal);
            return false;
        }

        if (_world.IsBlocked(start))
        {
            result = PathResult.Failure(PathStatus.StartBlocked, goal);
            return false;
        }

        if (_world.IsBlocked(goal))
        {
            result = PathResult.Failure(PathStatus.GoalBlocked, goal);
            return false;
        }

        int startId = _world.ToIndex(start);
        int goalId = _world.ToIndex(goal);
        if (startId == goalId)
        {
            result = PathResult.Success(goal, [start], 0, 0);
            return true;
        }

        bool found = _engine.FindPath(startId, goalId, out int cost, out int expanded);
        result = BuildResult(goal, goalId, cost, expanded, found ? PathStatus.Found : PathStatus.NoPath);
        return found;
    }

    /// <summary>Convenience overload that returns the result directly.</summary>
    /// <param name="start">Where the route begins.</param>
    /// <param name="goal">Where the route ends.</param>
    public PathResult FindPath(PathPoint start, PathPoint goal)
    {
        TryFindPath(start, goal, out PathResult result);
        return result;
    }

    /// <summary>
    /// Finds the cheapest route from start to any of the given targets, for questions like "which
    /// target is closest?". Targets that are outside the map or blocked are skipped. The chosen
    /// target is the last point of the route and is also reported as <see cref="PathResult.Goal"/>.
    /// This always uses the general-purpose engine, which handles terrain costs.
    /// </summary>
    /// <param name="start">Where the route begins.</param>
    /// <param name="goals">The candidate targets.</param>
    /// <param name="result">The outcome, including the route when one was found.</param>
    /// <returns>True when a route to one of the targets was found.</returns>
    public bool TryFindPathToNearest(PathPoint start, IEnumerable<PathPoint> goals, out PathResult result)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(goals);

        if (!_world.Contains(start))
        {
            result = PathResult.Failure(PathStatus.Invalid, start);
            return false;
        }

        if (_world.IsBlocked(start))
        {
            result = PathResult.Failure(PathStatus.StartBlocked, start);
            return false;
        }

        _goalBuffer.Clear();
        foreach (PathPoint goal in goals)
        {
            if (_world.Contains(goal) && !_world.IsBlocked(goal))
            {
                _goalBuffer.Add(_world.ToIndex(goal));
            }
        }

        if (_goalBuffer.Count == 0)
        {
            result = PathResult.Failure(PathStatus.Invalid, start);
            return false;
        }

        MultiGoalAStar engine = EnsureMultiGoalEngine();
        bool found = engine.FindPath(_world.ToIndex(start), _goalBuffer, out int reachedGoalId, out int cost, out int expanded);
        if (!found)
        {
            result = PathResult.Failure(PathStatus.NoPath, start, expanded);
            return false;
        }

        PathPoint reachedGoal = _world.ToPoint(reachedGoalId);
        engine.CollectPath(reachedGoalId, _idBuffer);
        IReadOnlyList<PathPoint> points = ConvertPath(_idBuffer);
        if (Options.Smooth)
        {
            points = PathSmoother.Smooth(_world, points);
        }

        result = PathResult.Success(reachedGoal, points, cost, expanded);
        return true;
    }

    /// <summary>
    /// Starts a search that can be advanced a bounded number of expansions at a time, so a long
    /// query can be spread over several calls instead of blocking one:
    ///
    /// <code>
    /// PathSearch search = finder.BeginSearch(start, goal);
    /// while (search.Step(maxNodes: 200) is null)
    /// {
    ///     // do other work between steps
    /// }
    /// PathResult result = search.GetResult();
    /// </code>
    ///
    /// Starting a new search, or calling <see cref="TryFindPath"/> while a search is running,
    /// abandons the previous one. Only the default algorithm advances in steps; the other
    /// algorithms complete on the first call.
    /// </summary>
    /// <param name="start">Where the route begins.</param>
    /// <param name="goal">Where the route ends.</param>
    public PathSearch BeginSearch(PathPoint start, PathPoint goal)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return new PathSearch(this, start, goal);
    }

    /// <summary>Releases the search buffers; the instance cannot be used afterwards.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _engine.Release();
        _multiGoalEngine?.Release();
        _multiGoalEngine = null;
    }

    internal PathResult BuildResult(PathPoint goal, int goalId, int cost, int expanded, PathStatus status)
    {
        if (status != PathStatus.Found)
        {
            return PathResult.Failure(status, goal, expanded);
        }

        if (_engine is GridSearchBase gridEngine)
        {
            gridEngine.CollectPath(goalId, _idBuffer);
        }
        else if (_engine is HpaStar hpaEngine)
        {
            hpaEngine.CollectPath(_idBuffer);
        }
        else
        {
            throw new InvalidOperationException("Unsupported search engine.");
        }

        IReadOnlyList<PathPoint> points = ConvertPath(_idBuffer);
        if (Options.Smooth)
        {
            points = PathSmoother.Smooth(_world, points);
        }

        return PathResult.Success(goal, points, cost, expanded);
    }

    private MultiGoalAStar EnsureMultiGoalEngine()
    {
        if (_multiGoalEngine is null)
        {
            _multiGoalEngine = new MultiGoalAStar(_world.ExtraCosts);
            _multiGoalEngine.Init(_world.Grid);
        }

        return _multiGoalEngine;
    }

    private IReadOnlyList<PathPoint> ConvertPath(List<int> ids)
    {
        var points = new PathPoint[ids.Count];
        for (int i = 0; i < ids.Count; i++)
        {
            points[i] = _world.ToPoint(ids[i]);
        }

        return points;
    }

    private static void ValidateAlgorithm(IPathWorld world, PathfinderAlgorithm algorithm)
    {
        bool uniformTerrainOnly = algorithm is PathfinderAlgorithm.BitboardAStar
            or PathfinderAlgorithm.JumpPointSearch
            or PathfinderAlgorithm.JpsPlus
            or PathfinderAlgorithm.HpaStar;

        if (uniformTerrainOnly && world.HasTerrainCosts)
        {
            throw new InvalidOperationException(
                $"{algorithm} needs uniform terrain, but the map has tiles with extra cost. " +
                $"Use {nameof(PathfinderAlgorithm.Default)}, or remove the costs.");
        }

        bool singleLayerOnly = algorithm is PathfinderAlgorithm.JumpPointSearch
            or PathfinderAlgorithm.JpsPlus;

        if (singleLayerOnly && world.LayerCount > 1)
        {
            throw new InvalidOperationException(
                $"{algorithm} works on single-layer maps only. " +
                $"Use {nameof(PathfinderAlgorithm.Default)} or {nameof(PathfinderAlgorithm.BitboardAStar)} for stacked maps.");
        }
    }

    private static IPathFinder CreateEngine(IPathWorld world, PathfinderAlgorithm algorithm)
    {
        return algorithm switch
        {
            PathfinderAlgorithm.Default => new WeightedAStar(world.ExtraCosts),
            PathfinderAlgorithm.BitboardAStar => new BitboardAStar(),
            PathfinderAlgorithm.JumpPointSearch => new JumpPointSearch(),
            PathfinderAlgorithm.JpsPlus => new JpsPlusSearch(),
            PathfinderAlgorithm.HpaStar => new HpaStar(16),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, "Unknown algorithm."),
        };
    }

    private static IPathWorld WorldOf(PathMap map)
    {
        ArgumentNullException.ThrowIfNull(map);
        return map;
    }

    private static IPathWorld WorldOf(PathMapStack stack)
    {
        ArgumentNullException.ThrowIfNull(stack);
        return stack;
    }
}
