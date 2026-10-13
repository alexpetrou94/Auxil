using Auxil.Pathfinding;

namespace Auxil.Tests.Pathfinding;

/// <summary>
/// Seeded randomized cross-check through the public API: every algorithm must agree with an
/// independent Dijkstra written from the movement rules (8-way, straight 10, diagonal 14, a
/// diagonal needs at least one walkable orthogonal neighbour). Complements the hand-picked
/// behaviour tests; the map generation is deterministic, so failures reproduce.
/// </summary>
public class RandomConsistencyTests
{
    [Theory]
    [InlineData(PathfinderAlgorithm.Default)]
    [InlineData(PathfinderAlgorithm.BitboardAStar)]
    [InlineData(PathfinderAlgorithm.JumpPointSearch)]
    [InlineData(PathfinderAlgorithm.JpsPlus)]
    public void EveryAlgorithm_MatchesAnIndependentDijkstra(PathfinderAlgorithm algorithm)
    {
        var random = new Random(20261010 + (int)algorithm);

        for (int iteration = 0; iteration < 8; iteration++)
        {
            int width = 6 + random.Next(14);
            int height = 6 + random.Next(14);
            var map = new PathMap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (random.NextDouble() < 0.25)
                    {
                        map.SetBlocked(x, y);
                    }
                }
            }

            var walkable = new List<PathPoint>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!map.IsBlocked(x, y))
                    {
                        walkable.Add(new PathPoint(x, y));
                    }
                }
            }

            if (walkable.Count < 2)
            {
                continue;
            }

            using var finder = new Pathfinder(map, algorithm);
            finder.Options.Smooth = false;

            for (int query = 0; query < 6; query++)
            {
                PathPoint start = walkable[random.Next(walkable.Count)];
                PathPoint goal = walkable[random.Next(walkable.Count)];
                if (start == goal)
                {
                    continue;
                }

                int reference = ReferenceCost(map, start, goal);
                bool found = finder.TryFindPath(start, goal, out PathResult result);

                found.ShouldBe(reference >= 0);
                if (found)
                {
                    result.Cost.ShouldBe(reference);
                }
            }
        }
    }

    /// <summary>Independent Dijkstra over the public map API, written from the movement rules.</summary>
    private static int ReferenceCost(PathMap map, PathPoint start, PathPoint goal)
    {
        int width = map.Width;
        int size = width * map.Height;
        var distance = new int[size];
        Array.Fill(distance, int.MaxValue);
        var queue = new PriorityQueue<int, int>();

        int startId = start.Y * width + start.X;
        int goalId = goal.Y * width + goal.X;
        distance[startId] = 0;
        queue.Enqueue(startId, 0);

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            int currentCost = distance[current];
            if (current == goalId)
            {
                return currentCost;
            }

            int x = current % width;
            int y = current / width;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    int neighborX = x + dx;
                    int neighborY = y + dy;
                    if (!map.Contains(neighborX, neighborY) || map.IsBlocked(neighborX, neighborY))
                    {
                        continue;
                    }

                    bool diagonal = dx != 0 && dy != 0;
                    if (diagonal && map.IsBlocked(x + dx, y) && map.IsBlocked(x, y + dy))
                    {
                        continue;
                    }

                    int neighbor = neighborY * width + neighborX;
                    int newCost = currentCost + (diagonal ? 14 : 10);
                    if (newCost < distance[neighbor])
                    {
                        distance[neighbor] = newCost;
                        queue.Enqueue(neighbor, newCost);
                    }
                }
            }
        }

        return -1;
    }
}
