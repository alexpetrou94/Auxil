using System.Collections.Concurrent;

namespace Auxil.Pathfinding;

/// <summary>
/// Runs many path queries in parallel on one map or stack of maps, for servers and background
/// work. Each concurrent query gets its own <see cref="Pathfinder"/>; idle instances are reused,
/// so the expensive setup (such as JPS+ precomputation) happens at most once per concurrent query.
///
/// <code>
/// using var pool = new PathfinderPool(map);
/// PathResult route = await pool.FindPathAsync(start, goal);
/// </code>
/// </summary>
public sealed class PathfinderPool : IDisposable
{
    private readonly IPathWorld _world;
    private readonly PathfinderAlgorithm _algorithm;
    private readonly ConcurrentBag<Pathfinder> _idle = new();
    private readonly SemaphoreSlim _slots;
    private bool _disposed;

    /// <summary>Creates a pool over a single map.</summary>
    /// <param name="map">The map every query runs on.</param>
    /// <param name="algorithm">The search engine for every query.</param>
    /// <param name="maxConcurrency">Queries allowed at once; 0 or less picks the processor count.</param>
    public PathfinderPool(PathMap map, PathfinderAlgorithm algorithm = PathfinderAlgorithm.Default, int maxConcurrency = 0)
        : this(WorldOf(map), algorithm, maxConcurrency)
    {
        Map = map;
    }

    /// <summary>Creates a pool over a stack of maps.</summary>
    /// <param name="stack">The stacked maps every query runs on.</param>
    /// <param name="algorithm">The search engine for every query.</param>
    /// <param name="maxConcurrency">Queries allowed at once; 0 or less picks the processor count.</param>
    public PathfinderPool(PathMapStack stack, PathfinderAlgorithm algorithm = PathfinderAlgorithm.Default, int maxConcurrency = 0)
        : this(WorldOf(stack), algorithm, maxConcurrency)
    {
        Stack = stack;
    }

    private PathfinderPool(IPathWorld world, PathfinderAlgorithm algorithm, int maxConcurrency)
    {
        _world = world;
        _algorithm = algorithm;
        int limit = maxConcurrency > 0 ? maxConcurrency : Environment.ProcessorCount;
        _slots = new SemaphoreSlim(limit, limit);
    }

    /// <summary>The map every query runs on; null when the pool was created from a stack.</summary>
    public PathMap? Map { get; }

    /// <summary>The stack every query runs on; null when the pool was created from a single map.</summary>
    public PathMapStack? Stack { get; }

    /// <summary>The search engine every query uses.</summary>
    public PathfinderAlgorithm Algorithm => _algorithm;

    /// <summary>
    /// Finds a route on a pool worker. Queries beyond the concurrency limit wait for a free slot.
    /// Cancelling only stops a query that has not started yet; a running search finishes first.
    /// </summary>
    /// <param name="start">Where the route begins.</param>
    /// <param name="goal">Where the route ends.</param>
    /// <param name="cancellationToken">Stops a query that is still waiting for a free slot.</param>
    public async Task<PathResult> FindPathAsync(PathPoint start, PathPoint goal, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _slots.WaitAsync(cancellationToken).ConfigureAwait(false);

        Pathfinder? worker = null;
        try
        {
            if (!_idle.TryTake(out worker))
            {
                worker = CreateWorker();
            }

            Pathfinder current = worker;
            return await Task.Run(() => current.FindPath(start, goal), cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (worker is not null)
            {
                if (_disposed)
                {
                    worker.Dispose();
                }
                else
                {
                    _idle.Add(worker);
                }
            }

            if (!_disposed)
            {
                _slots.Release();
            }
        }
    }

    /// <summary>Disposes the idle workers. Queries already running finish on their own.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        while (_idle.TryTake(out Pathfinder? worker))
        {
            worker.Dispose();
        }

        _slots.Dispose();
    }

    private Pathfinder CreateWorker()
    {
        return _world is PathMapStack stack
            ? new Pathfinder(stack, _algorithm)
            : new Pathfinder((PathMap)_world, _algorithm);
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
