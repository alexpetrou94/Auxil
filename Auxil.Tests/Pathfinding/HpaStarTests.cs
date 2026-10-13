using Auxil.Pathfinding;

namespace Auxil.Tests.Pathfinding;

/// <summary>
/// Black-box tests for the hierarchical A* engine exposed through the public API.
/// No engine types, no internals.
/// </summary>
public class HpaStarTests
{
    [Fact]
    public void FindsARouteAcrossAnOpenMap()
    {
        using var finder = new Pathfinder(new PathMap(40, 40), PathfinderAlgorithm.HpaStar);
        finder.Options.Smooth = false;

        finder.TryFindPath(new PathPoint(0, 0), new PathPoint(39, 39), out PathResult result).ShouldBeTrue();

        result.Status.ShouldBe(PathStatus.Found);
        result.Points[0].ShouldBe(new PathPoint(0, 0));
        result.Points[^1].ShouldBe(new PathPoint(39, 39));
        result.Cost.ShouldBeGreaterThanOrEqualTo(39 * 14); // never below the straight-line estimate

        // Every consecutive pair of points is one tile apart.
        for (int i = 0; i + 1 < result.Points.Count; i++)
        {
            int dx = Math.Abs(result.Points[i + 1].X - result.Points[i].X);
            int dy = Math.Abs(result.Points[i + 1].Y - result.Points[i].Y);
            Math.Max(dx, dy).ShouldBe(1);
        }
    }

    [Fact]
    public void RoutesAroundAWall()
    {
        var map = PathMap.FromRows([
            ".....#.....",
            ".....#.....",
            ".....#.....",
            ".....#.....",
            ".....#.....",
            ".....#.....",
            ".....#.....",
            ".....#.....",
            ".....#.....",
            "...........",
        ]);
        using var finder = new Pathfinder(map, PathfinderAlgorithm.HpaStar);
        finder.Options.Smooth = false;

        finder.TryFindPath(new PathPoint(0, 0), new PathPoint(10, 0), out PathResult result).ShouldBeTrue();

        result.Cost.ShouldBeGreaterThan(10 * 10); // must detour to the bottom or top
        result.Points.ShouldAllBe(p => !map.IsBlocked(p));
    }

    [Fact]
    public void FullyWalledOff_ReturnsNoPath()
    {
        var map = PathMap.FromRows([
            "..#..",
            "..#..",
            "..#..",
            "..#..",
            "..#..",
        ]);
        using var finder = new Pathfinder(map, PathfinderAlgorithm.HpaStar);

        finder.TryFindPath(new PathPoint(0, 0), new PathPoint(4, 4), out PathResult result).ShouldBeFalse();

        result.Status.ShouldBe(PathStatus.NoPath);
    }

    [Fact]
    public void BlockedStart_ReturnsStartBlocked()
    {
        var map = new PathMap(10, 10);
        map.SetBlocked(0, 0);
        using var finder = new Pathfinder(map, PathfinderAlgorithm.HpaStar);

        finder.TryFindPath(new PathPoint(0, 0), new PathPoint(9, 9), out PathResult result).ShouldBeFalse();

        result.Status.ShouldBe(PathStatus.StartBlocked);
    }

    [Fact]
    public void TerrainCosts_AreRejected()
    {
        var map = new PathMap(10, 10);
        map.SetExtraCost(5, 5, 10);

        Should.Throw<InvalidOperationException>(() => new Pathfinder(map, PathfinderAlgorithm.HpaStar));
    }

    [Fact]
    public void StackedMaps_CrossTheStairs()
    {
        var stack = new PathMapStack();
        stack.AddLayer(new PathMap(30, 30));
        stack.AddLayer(new PathMap(30, 30));
        stack.ConnectStairs(15, 15, 0);

        using var finder = new Pathfinder(stack, PathfinderAlgorithm.HpaStar);
        finder.Options.Smooth = false;

        finder.TryFindPath(new PathPoint(0, 0, 0), new PathPoint(29, 29, 1), out PathResult result).ShouldBeTrue();

        result.Cost.ShouldBeGreaterThanOrEqualTo(15 * 14 + 10 + 14 * 14); // straight-line estimate through the stairs
        result.Points.ShouldContain(new PathPoint(15, 15, 0));
        result.Points.ShouldContain(new PathPoint(15, 15, 1));
        result.Points[0].ShouldBe(new PathPoint(0, 0, 0));
        result.Points[^1].ShouldBe(new PathPoint(29, 29, 1));
    }
}
