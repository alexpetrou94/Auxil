using Auxil.Pathfinding.Engine;

namespace Auxil.Pathfinding;

/// <summary>
/// A flow field for moving many agents toward one shared goal, on a single map or a stack of maps.
/// Build it once with <see cref="SetGoal"/> (one O(map) sweep), then every agent can ask for its
/// next step or a whole route with no per-agent search:
///
/// <code>
/// using var field = new CrowdField(map);
/// field.SetGoal(new PathPoint(50, 50));
/// if (field.TryGetNextStep(agentPosition, out PathPoint next))
/// {
///     agentPosition = next;
/// }
/// </code>
///
/// The field understands the full movement model, including diagonal corner rules and stairs
/// between layers, and it reads the map live: edits are picked up by the next <see cref="SetGoal"/>.
/// Distances and route costs use base step costs only; extra terrain costs are ignored. Building a
/// new goal replaces the previous field. Use it when many agents share a goal; for one-off queries
/// use <see cref="Pathfinder"/>.
/// </summary>
public sealed class CrowdField : IDisposable
{
    private readonly IPathWorld _world;
    private readonly FlowField _field;
    private readonly List<int> _idBuffer = new(256);
    private bool _disposed;

    /// <summary>Creates a field over a single map. No goal is set yet.</summary>
    /// <param name="map">The map agents walk on.</param>
    public CrowdField(PathMap map)
        : this(WorldOf(map))
    {
        Map = map;
    }

    /// <summary>Creates a field over a stack of maps. No goal is set yet.</summary>
    /// <param name="stack">The stacked maps agents walk on.</param>
    public CrowdField(PathMapStack stack)
        : this(WorldOf(stack))
    {
        Stack = stack;
    }

    private CrowdField(IPathWorld world)
    {
        _world = world;
        _field = new FlowField();
        _field.Init(world.Grid);
    }

    /// <summary>The map agents walk on; null when the field was created from a stack.</summary>
    public PathMap? Map { get; }

    /// <summary>The stack agents walk on; null when the field was created from a single map.</summary>
    public PathMapStack? Stack { get; }

    /// <summary>The current goal, or the default point when none is set.</summary>
    public PathPoint Goal { get; private set; }

    /// <summary>True once a goal has been set.</summary>
    public bool HasGoal { get; private set; }

    /// <summary>
    /// Builds the field toward the given goal, replacing any previous goal. The goal must be a
    /// walkable tile inside the world; this is a programming error, not a search outcome, so it
    /// throws.
    /// </summary>
    /// <param name="goal">The shared destination for all agents.</param>
    public void SetGoal(PathPoint goal)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_world.Contains(goal))
        {
            throw new ArgumentOutOfRangeException(
                nameof(goal),
                $"({goal.X}, {goal.Y}, layer {goal.Layer}) is outside the {_world.Width}x{_world.Height} map.");
        }

        if (_world.IsBlocked(goal))
        {
            throw new ArgumentException("The goal tile is blocked; pick a walkable tile.", nameof(goal));
        }

        _field.ComputeField(_world.ToIndex(goal));
        Goal = goal;
        HasGoal = true;
    }

    /// <summary>
    /// Distance from the goal to the given point in the current field (10 per straight step,
    /// 14 per diagonal), or -1 when the point cannot reach the goal. Extra terrain costs are not
    /// part of field distances.
    /// </summary>
    /// <param name="point">The point to measure.</param>
    public int DistanceAt(PathPoint point)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        RequireGoal();

        if (!_world.Contains(point))
        {
            throw new ArgumentOutOfRangeException(
                nameof(point),
                $"({point.X}, {point.Y}, layer {point.Layer}) is outside the {_world.Width}x{_world.Height} map.");
        }

        return _field.DistanceAt(_world.ToIndex(point));
    }

    /// <summary>
    /// Returns the next tile an agent at <paramref name="from"/> should walk to. False when the tile
    /// cannot reach the goal or is already the goal. This is the call each agent makes as it moves.
    /// </summary>
    /// <param name="from">Where the agent stands.</param>
    /// <param name="next">The tile to move to.</param>
    public bool TryGetNextStep(PathPoint from, out PathPoint next)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        RequireGoal();
        next = default;

        if (!_world.Contains(from))
        {
            return false;
        }

        if (!_field.TryGetNextCell(_world.ToIndex(from), out int nextCell))
        {
            return false;
        }

        next = _world.ToPoint(nextCell);
        return true;
    }

    /// <summary>
    /// Walks the field from <paramref name="start"/> to the goal and returns the full route,
    /// following the same optimal distances as the field. Field-based routes do not expand tiles
    /// per query, so <see cref="PathResult.NodesExpanded"/> is 0.
    /// </summary>
    /// <param name="start">Where the route begins.</param>
    /// <param name="result">The outcome, including the route when one was found.</param>
    /// <returns>True when the start can reach the goal.</returns>
    public bool TryFindPath(PathPoint start, out PathResult result)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        RequireGoal();

        if (!_world.Contains(start))
        {
            result = PathResult.Failure(PathStatus.Invalid, Goal);
            return false;
        }

        if (_world.IsBlocked(start))
        {
            result = PathResult.Failure(PathStatus.StartBlocked, Goal);
            return false;
        }

        int currentId = _world.ToIndex(start);
        int goalId = _world.ToIndex(Goal);
        if (currentId == goalId)
        {
            result = PathResult.Success(Goal, [start], 0, 0);
            return true;
        }

        Grid grid = _world.Grid;
        _idBuffer.Clear();
        _idBuffer.Add(currentId);
        int cost = 0;
        int stepGuard = 4 * grid.Size + 16;

        while (true)
        {
            if (!_field.TryGetNextCell(currentId, out int nextId))
            {
                result = PathResult.Failure(PathStatus.NoPath, Goal);
                return false;
            }

            int direction = grid.DirectionBetween(currentId, nextId);
            cost += Grid.MoveCost[direction];
            _idBuffer.Add(nextId);
            currentId = nextId;

            if (currentId == goalId)
            {
                break;
            }

            if (_idBuffer.Count > stepGuard)
            {
                result = PathResult.Failure(PathStatus.NoPath, Goal);
                return false;
            }
        }

        var points = new PathPoint[_idBuffer.Count];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = _world.ToPoint(_idBuffer[i]);
        }

        result = PathResult.Success(Goal, points, cost, 0);
        return true;
    }

    /// <summary>Releases the field buffers.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _field.Release();
    }

    private void RequireGoal()
    {
        if (!HasGoal)
        {
            throw new InvalidOperationException("Call SetGoal before using the field.");
        }
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
