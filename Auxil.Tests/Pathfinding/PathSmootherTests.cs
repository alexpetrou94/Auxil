using Auxil.Pathfinding;

namespace Auxil.Tests.Pathfinding;

public class PathSmootherTests
{
    [Fact]
    public void LineOfSight_OnAnEmptyMap_IsClear()
    {
        var map = new PathMap(5, 5);

        PathSmoother.HasLineOfSight(map, new PathPoint(0, 0), new PathPoint(4, 4)).ShouldBeTrue();
    }

    [Fact]
    public void LineOfSight_ThroughAWall_IsBlocked()
    {
        var map = PathMap.FromRows([
            "..#..",
            "..#..",
            "..#..",
            "..#..",
            "..#..",
        ]);

        PathSmoother.HasLineOfSight(map, new PathPoint(0, 2), new PathPoint(4, 2)).ShouldBeFalse();
    }

    [Fact]
    public void LineOfSight_AcrossACostedTile_IsBlocked()
    {
        var map = new PathMap(3, 3);
        map.SetExtraCost(1, 1, 5);

        // The tile is walkable, but smoothing must not ignore what it costs.
        PathSmoother.HasLineOfSight(map, new PathPoint(0, 0), new PathPoint(2, 2)).ShouldBeFalse();
    }

    [Fact]
    public void LineOfSight_ThroughATwoBlockedCorner_IsBlocked()
    {
        var map = PathMap.FromRows([
            ".#",
            "#.",
        ]);

        // The diagonal step needs at least one free orthogonal neighbour.
        PathSmoother.HasLineOfSight(map, new PathPoint(0, 0), new PathPoint(1, 1)).ShouldBeFalse();
    }

    [Fact]
    public void Smooth_LeavesShortRoutesAlone()
    {
        var map = new PathMap(5, 5);
        PathPoint[] path = [new(0, 0), new(4, 4)];

        PathSmoother.Smooth(map, path).Count.ShouldBe(2);
    }

    [Fact]
    public void Smooth_RemovesPointsItCanSkip()
    {
        var map = new PathMap(9, 1);
        PathPoint[] path =
        [
            new(0, 0), new(1, 0), new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0), new(7, 0), new(8, 0),
        ];

        IReadOnlyList<PathPoint> smoothed = PathSmoother.Smooth(map, path);

        smoothed.Count.ShouldBe(2);
        smoothed[0].ShouldBe(new PathPoint(0, 0));
        smoothed[1].ShouldBe(new PathPoint(8, 0));
    }

    [Fact]
    public void LineOfSight_OutsideTheMap_IsBlocked()
    {
        var map = new PathMap(5, 5);

        PathSmoother.HasLineOfSight(map, new PathPoint(0, 0), new PathPoint(9, 9)).ShouldBeFalse();
        PathSmoother.HasLineOfSight(map, new PathPoint(2, 2), new PathPoint(2, 2)).ShouldBeTrue();
    }
}
