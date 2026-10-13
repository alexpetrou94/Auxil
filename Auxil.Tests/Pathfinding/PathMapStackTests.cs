using Auxil.Pathfinding;

namespace Auxil.Tests.Pathfinding;

/// <summary>
/// Black-box tests for stacked maps: layers, stairs, and routes that cross between floors.
/// No engine types, no internals.
/// </summary>
public class PathMapStackTests
{
    [Fact]
    public void RouteCrossesTheStairsBetweenLayers()
    {
        var stack = new PathMapStack();
        stack.AddLayer(new PathMap(5, 5)).ShouldBe(0);
        stack.AddLayer(new PathMap(5, 5)).ShouldBe(1);
        stack.ConnectStairs(2, 2, lowerLayer: 0);

        using var finder = new Pathfinder(stack);
        finder.Options.Smooth = false;

        finder.TryFindPath(new PathPoint(0, 0, 0), new PathPoint(4, 4, 1), out PathResult result).ShouldBeTrue();

        // Two diagonals on the ground, one step up the stairs, two diagonals on the upper floor.
        result.Cost.ShouldBe(66);
        result.Points[0].ShouldBe(new PathPoint(0, 0, 0));
        result.Points[^1].ShouldBe(new PathPoint(4, 4, 1));
        result.Points.ShouldContain(new PathPoint(2, 2, 0));
        result.Points.ShouldContain(new PathPoint(2, 2, 1));
    }

    [Fact]
    public void WithoutStairs_TheLayersAreSeparate()
    {
        var stack = new PathMapStack();
        stack.AddLayer(new PathMap(5, 5));
        stack.AddLayer(new PathMap(5, 5));

        using var finder = new Pathfinder(stack);

        finder.TryFindPath(new PathPoint(0, 0, 0), new PathPoint(4, 4, 1), out PathResult result).ShouldBeFalse();
        result.Status.ShouldBe(PathStatus.NoPath);
    }

    [Fact]
    public void SameLayerRoute_StaysOnThatLayer()
    {
        var stack = new PathMapStack();
        stack.AddLayer(new PathMap(5, 5));
        stack.AddLayer(new PathMap(5, 5));
        stack.ConnectStairs(2, 2, 0);

        using var finder = new Pathfinder(stack);
        finder.Options.Smooth = false;

        finder.TryFindPath(new PathPoint(0, 0, 0), new PathPoint(4, 4, 0), out PathResult result).ShouldBeTrue();

        result.Cost.ShouldBe(56);
        result.Points.ShouldAllBe(p => p.Layer == 0);
    }

    [Fact]
    public void BlockedStairs_BlockTheLayerChange()
    {
        var stack = new PathMapStack();
        stack.AddLayer(new PathMap(5, 5));
        stack.AddLayer(new PathMap(5, 5));
        stack.ConnectStairs(2, 2, 0);
        stack.SetBlocked(new PathPoint(2, 2, 1)); // the upper landing is blocked

        using var finder = new Pathfinder(stack);

        finder.TryFindPath(new PathPoint(0, 0, 0), new PathPoint(4, 4, 1), out PathResult result).ShouldBeFalse();
        result.Status.ShouldBe(PathStatus.NoPath);
    }

    [Fact]
    public void PointOnALayerThatDoesNotExist_IsInvalid()
    {
        var stack = new PathMapStack();
        stack.AddLayer(new PathMap(5, 5));

        using var finder = new Pathfinder(stack);

        finder.TryFindPath(new PathPoint(0, 0, 0), new PathPoint(4, 4, 1), out PathResult result).ShouldBeFalse();
        result.Status.ShouldBe(PathStatus.Invalid);
    }

    [Fact]
    public void TerrainCosts_CanMakeTheUpperLayerTheCheaperRoute()
    {
        var ground = new PathMap(5, 5);
        ground.SetExtraCost(2, 2, 500); // the direct ground diagonal is expensive mud
        var stack = new PathMapStack();
        stack.AddLayer(ground);
        stack.AddLayer(new PathMap(5, 5));
        stack.ConnectStairs(0, 0, 0);
        stack.ConnectStairs(4, 4, 0);

        using var finder = new Pathfinder(stack);
        finder.Options.Smooth = false;

        finder.TryFindPath(new PathPoint(0, 0, 0), new PathPoint(4, 4, 1), out PathResult result).ShouldBeTrue();

        // Up the stairs at (0, 0) and straight across the clean upper floor: 10 + 56.
        result.Cost.ShouldBe(66);
        result.Points.ShouldContain(new PathPoint(0, 0, 1));
        result.Points.ShouldNotContain(new PathPoint(2, 2, 0));
    }

    [Fact]
    public void StackMetadata_TracksLayersAndTerrain()
    {
        var ground = new PathMap(4, 3);
        ground.SetExtraCost(0, 0, 5);
        var stack = new PathMapStack();
        stack.AddLayer(ground);
        stack.AddLayer(new PathMap(4, 3));

        stack.LayerCount.ShouldBe(2);
        stack.Width.ShouldBe(4);
        stack.Height.ShouldBe(3);
        stack.HasTerrainCosts.ShouldBeTrue();
        stack.GetExtraCost(new PathPoint(0, 0, 0)).ShouldBe(5);
        stack.GetExtraCost(0, 0, 1).ShouldBe(0);
        stack.Contains(new PathPoint(3, 2, 1)).ShouldBeTrue();
        stack.Contains(4, 2, 1).ShouldBeFalse();
        stack.Contains(new PathPoint(0, 0, 2)).ShouldBeFalse();
        stack.IsBlocked(0, 0, 0).ShouldBeFalse();

        stack.SetBlocked(1, 1, 1);
        stack.IsBlocked(1, 1, 1).ShouldBeTrue();
        stack.SetBlocked(1, 1, 1, blocked: false);
        stack.IsBlocked(1, 1, 1).ShouldBeFalse();

        stack.SetExtraCost(new PathPoint(2, 2, 1), 7);
        stack.GetExtraCost(2, 2, 1).ShouldBe(7);
        stack.SetExtraCost(2, 2, 1, 9);
        stack.GetExtraCost(new PathPoint(2, 2, 1)).ShouldBe(9);
    }

    [Fact]
    public void LayersOfDifferentSizes_AreRejected()
    {
        var stack = new PathMapStack();
        stack.AddLayer(new PathMap(5, 5));

        Should.Throw<ArgumentException>(() => stack.AddLayer(new PathMap(6, 5)));
    }

    [Fact]
    public void ConnectStairs_OnBlockedTiles_Throws()
    {
        var stack = new PathMapStack();
        stack.AddLayer(new PathMap(5, 5));
        stack.AddLayer(new PathMap(5, 5));
        stack.SetBlocked(new PathPoint(2, 2, 0));

        Should.Throw<InvalidOperationException>(() => stack.ConnectStairs(2, 2, 0));
    }

    [Fact]
    public void ConnectStairs_BeyondTheTopLayer_Throws()
    {
        var stack = new PathMapStack();
        stack.AddLayer(new PathMap(5, 5));
        stack.AddLayer(new PathMap(5, 5));

        Should.Throw<ArgumentOutOfRangeException>(() => stack.ConnectStairs(2, 2, lowerLayer: 1));
    }

    [Fact]
    public void EmptyStack_CannotBeSearched()
    {
        Should.Throw<InvalidOperationException>(() => new Pathfinder(new PathMapStack()));
    }

    [Fact]
    public void JpsPlus_RejectsStackedMaps()
    {
        var stack = new PathMapStack();
        stack.AddLayer(new PathMap(5, 5));
        stack.AddLayer(new PathMap(5, 5));

        Should.Throw<InvalidOperationException>(() => new Pathfinder(stack, PathfinderAlgorithm.JpsPlus));
    }

    [Fact]
    public void Bitboard_WorksOnStackedMaps()
    {
        var stack = new PathMapStack();
        stack.AddLayer(new PathMap(5, 5));
        stack.AddLayer(new PathMap(5, 5));
        stack.ConnectStairs(2, 2, 0);

        using var finder = new Pathfinder(stack, PathfinderAlgorithm.BitboardAStar);
        finder.Options.Smooth = false;

        finder.TryFindPath(new PathPoint(0, 0, 0), new PathPoint(4, 4, 1), out PathResult result).ShouldBeTrue();

        result.Cost.ShouldBe(66);
    }

    [Fact]
    public void SteppedSearch_WorksOnStackedMaps()
    {
        var stack = new PathMapStack();
        stack.AddLayer(new PathMap(5, 5));
        stack.AddLayer(new PathMap(5, 5));
        stack.ConnectStairs(2, 2, 0);

        using var finder = new Pathfinder(stack);
        finder.Options.Smooth = false;

        PathSearch search = finder.BeginSearch(new PathPoint(0, 0, 0), new PathPoint(4, 4, 1));
        while (search.Step(3) is null)
        {
        }

        search.GetResult().Cost.ShouldBe(66);
    }

    [Fact]
    public void CrowdField_WorksOnStackedMaps()
    {
        var stack = new PathMapStack();
        stack.AddLayer(new PathMap(5, 5));
        stack.AddLayer(new PathMap(5, 5));
        stack.ConnectStairs(2, 2, 0);

        using var field = new CrowdField(stack);
        field.SetGoal(new PathPoint(4, 4, 1));
        field.Stack.ShouldBeSameAs(stack);
        field.Map.ShouldBeNull();

        field.TryFindPath(new PathPoint(0, 0, 0), out PathResult route).ShouldBeTrue();
        route.Cost.ShouldBe(66);
        route.Points.ShouldContain(new PathPoint(2, 2, 0));
        route.Points.ShouldContain(new PathPoint(2, 2, 1));

        field.TryGetNextStep(new PathPoint(2, 2, 0), out PathPoint next).ShouldBeTrue();
        next.ShouldBe(new PathPoint(2, 2, 1)); // at the lower landing, the stairs are the next step
    }

    [Fact]
    public async Task PathfinderPool_WorksOnStackedMaps()
    {
        var stack = new PathMapStack();
        stack.AddLayer(new PathMap(5, 5));
        stack.AddLayer(new PathMap(5, 5));
        stack.ConnectStairs(2, 2, 0);

        using var pool = new PathfinderPool(stack);
        PathResult result = await pool.FindPathAsync(new PathPoint(0, 0, 0), new PathPoint(4, 4, 1));

        result.Found.ShouldBeTrue();
        result.Cost.ShouldBe(66);
    }

    [Fact]
    public void TerrainCostSetOnTheStack_ChangesTheChosenRoute()
    {
        var stack = new PathMapStack();
        stack.AddLayer(new PathMap(5, 1));
        stack.AddLayer(new PathMap(5, 1));
        stack.ConnectStairs(0, 0, 0);
        stack.ConnectStairs(4, 0, 0);

        using var finder = new Pathfinder(stack);
        finder.Options.Smooth = false;

        // Mud appears on the ground layer after the finder exists; the next query avoids it.
        stack.SetExtraCost(1, 0, 0, 100);
        stack.SetExtraCost(2, 0, 0, 100);
        stack.SetExtraCost(3, 0, 0, 100);

        finder.TryFindPath(new PathPoint(0, 0, 0), new PathPoint(4, 0, 1), out PathResult result).ShouldBeTrue();

        result.Cost.ShouldBe(50); // up at (0, 0), then straight across the clean upper layer
        result.Points.ShouldNotContain(new PathPoint(2, 0, 0));
    }

    [Fact]
    public void NearestTarget_SearchesAcrossLayers()
    {
        var stack = new PathMapStack();
        stack.AddLayer(new PathMap(5, 5));
        stack.AddLayer(new PathMap(5, 5));
        stack.ConnectStairs(2, 2, 0);

        using var finder = new Pathfinder(stack);
        finder.Options.Smooth = false;

        // (4, 4, 0) is closer in tiles, but (2, 2, 1) is cheaper because the stairs are at (2, 2).
        PathPoint[] goals = [new(4, 4, 0), new(2, 2, 1)];

        finder.TryFindPathToNearest(new PathPoint(0, 0, 0), goals, out PathResult result).ShouldBeTrue();

        result.Goal.ShouldBe(new PathPoint(2, 2, 1));
        result.Cost.ShouldBe(38); // two diagonals, then up the stairs
    }
}
