using Auxil.Pathfinding.Engine;

namespace Auxil.Pathfinding;

/// <summary>
/// Shortens a route by dropping waypoints that can be skipped. If a straight walk from A to C is
/// clear, the detour through B adds nothing. Smoothing never changes the route's cost: a straight
/// replacement is only used when every tile on it is walkable normal ground, which is exactly when
/// the straight walk costs the same as the steps it replaces.
/// </summary>
public static class PathSmoother
{
    /// <summary>Returns a shorter waypoint list for the same route.</summary>
    /// <param name="map">The map the route lives on.</param>
    /// <param name="path">The route, start to goal.</param>
    public static IReadOnlyList<PathPoint> Smooth(PathMap map, IReadOnlyList<PathPoint> path)
    {
        ArgumentNullException.ThrowIfNull(map);
        return Smooth((IPathWorld)map, path);
    }

    internal static IReadOnlyList<PathPoint> Smooth(IPathWorld world, IReadOnlyList<PathPoint> path)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(path);

        if (path.Count <= 2)
        {
            return path;
        }

        var smoothed = new List<PathPoint>(path.Count) { path[0] };
        int anchor = 0;
        while (anchor < path.Count - 1)
        {
            int next = anchor + 1;
            for (int candidate = path.Count - 1; candidate > anchor + 1; candidate--)
            {
                if (HasLineOfSight(world, path[anchor], path[candidate]))
                {
                    next = candidate;
                    break;
                }
            }

            smoothed.Add(path[next]);
            anchor = next;
        }

        return smoothed;
    }

    /// <summary>
    /// True when a straight walk between the two points is possible: every step is legal under the
    /// map's movement rules and every tile entered is normal ground.
    /// </summary>
    /// <param name="map">The map to walk on.</param>
    /// <param name="from">First point.</param>
    /// <param name="to">Second point.</param>
    public static bool HasLineOfSight(PathMap map, PathPoint from, PathPoint to)
    {
        ArgumentNullException.ThrowIfNull(map);
        return HasLineOfSight((IPathWorld)map, from, to);
    }

    private static bool HasLineOfSight(IPathWorld world, PathPoint from, PathPoint to)
    {
        if (!world.Contains(from) || !world.Contains(to))
        {
            return false;
        }

        if (from == to)
        {
            return true;
        }

        // A shortcut never crosses layers; stairs are the only way up or down.
        if (from.Layer != to.Layer)
        {
            return false;
        }

        Grid grid = world.Grid;
        int[] extraCosts = world.ExtraCosts;
        int layer = from.Layer;
        int x = from.X;
        int y = from.Y;
        int deltaX = Math.Abs(to.X - x);
        int deltaY = Math.Abs(to.Y - y);
        int stepX = Math.Sign(to.X - x);
        int stepY = Math.Sign(to.Y - y);
        int error = deltaX - deltaY;

        while (x != to.X || y != to.Y)
        {
            int doubledError = 2 * error;
            int nextX = x;
            int nextY = y;
            if (doubledError > -deltaY)
            {
                error -= deltaY;
                nextX += stepX;
            }

            if (doubledError < deltaX)
            {
                error += deltaX;
                nextY += stepY;
            }

            int direction = Grid.DirectionOf(nextX - x, nextY - y);
            if (direction < 0)
            {
                return false;
            }

            int currentId = grid.Index(x, y, layer);
            if (!grid.TryStep(currentId, x, y, layer, direction, out int neighborId))
            {
                return false;
            }

            if (extraCosts[neighborId] != 0)
            {
                return false;
            }

            x = nextX;
            y = nextY;
        }

        return true;
    }
}
