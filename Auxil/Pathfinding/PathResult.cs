namespace Auxil.Pathfinding;

/// <summary>
/// The outcome of a path request: why it ended, the route when one was found, and how much work
/// the search did. <see cref="Cost"/> counts 10 per straight step and 14 per diagonal step; extra
/// terrain costs add on top. Smoothing never changes <see cref="Cost"/>; it only removes waypoints.
/// </summary>
public sealed class PathResult
{
    private static readonly PathPoint[] NoPoints = [];

    private PathResult(PathStatus status, PathPoint goal, IReadOnlyList<PathPoint> points, int cost, int nodesExpanded)
    {
        Status = status;
        Goal = goal;
        Points = points;
        Cost = cost;
        NodesExpanded = nodesExpanded;
    }

    /// <summary>Why the request ended.</summary>
    public PathStatus Status { get; }

    /// <summary>True when a route was found.</summary>
    public bool Found => Status == PathStatus.Found;

    /// <summary>
    /// The goal the route leads to. For a nearest-target search this is the target that was
    /// chosen, which is also the last point of the route.
    /// </summary>
    public PathPoint Goal { get; }

    /// <summary>The route from start to goal, inclusive. Empty when no route was found.</summary>
    public IReadOnlyList<PathPoint> Points { get; }

    /// <summary>Number of points on the route; 0 when no route was found.</summary>
    public int Steps => Points.Count;

    /// <summary>Cost of the route, or 0 when no route was found.</summary>
    public int Cost { get; }

    /// <summary>How many tiles the search expanded while looking.</summary>
    public int NodesExpanded { get; }

    /// <summary>Creates a successful result.</summary>
    internal static PathResult Success(PathPoint goal, IReadOnlyList<PathPoint> points, int cost, int nodesExpanded) =>
        new(PathStatus.Found, goal, points, cost, nodesExpanded);

    /// <summary>Creates a failed result with the given reason.</summary>
    internal static PathResult Failure(PathStatus status, PathPoint goal, int nodesExpanded = 0) =>
        new(status, goal, NoPoints, 0, nodesExpanded);

    /// <summary>A short human-readable summary, useful in logs.</summary>
    public override string ToString() =>
        Found
            ? $"{Status}: {Steps} steps, cost {Cost}, {NodesExpanded} tiles examined"
            : $"{Status} ({NodesExpanded} tiles examined)";
}
