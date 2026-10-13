using Auxil.Pathfinding;

namespace Auxil.Tests.Pathfinding;

public class PathMapTests
{
    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(4, 4, true)]
    [InlineData(-1, 0, false)]
    [InlineData(0, -1, false)]
    [InlineData(5, 0, false)]
    [InlineData(0, 5, false)]
    public void Contains_ReportsTheBounds(int x, int y, bool expected)
    {
        var map = new PathMap(5, 5);

        map.Contains(x, y).ShouldBe(expected);
    }

    [Fact]
    public void SetBlocked_AndUnblocked_RoundTrips()
    {
        var map = new PathMap(3, 3);

        map.IsBlocked(1, 1).ShouldBeFalse();
        map.SetBlocked(1, 1);
        map.IsBlocked(1, 1).ShouldBeTrue();
        map.SetBlocked(1, 1, blocked: false);
        map.IsBlocked(1, 1).ShouldBeFalse();
    }

    [Fact]
    public void SetBlocked_Rectangle_BlocksExactlyTheRectangle()
    {
        var map = new PathMap(5, 5);

        map.SetBlocked(1, 1, width: 3, height: 2);

        map.IsBlocked(1, 1).ShouldBeTrue();
        map.IsBlocked(3, 2).ShouldBeTrue();
        map.IsBlocked(0, 1).ShouldBeFalse();
        map.IsBlocked(4, 2).ShouldBeFalse();
        map.IsBlocked(1, 3).ShouldBeFalse();
    }

    [Fact]
    public void SetBlocked_OutsideTheMap_Throws()
    {
        var map = new PathMap(3, 3);

        Should.Throw<ArgumentOutOfRangeException>(() => map.SetBlocked(3, 0));
        Should.Throw<ArgumentOutOfRangeException>(() => map.SetBlocked(0, 3));
        Should.Throw<ArgumentOutOfRangeException>(() => map.SetBlocked(2, 2, width: 2, height: 1));
    }

    [Fact]
    public void SetExtraCost_Negative_Throws()
    {
        var map = new PathMap(3, 3);

        Should.Throw<ArgumentOutOfRangeException>(() => map.SetExtraCost(1, 1, -1));
        map.HasTerrainCosts.ShouldBeFalse();
        map.SetExtraCost(1, 1, 5);
        map.HasTerrainCosts.ShouldBeTrue();
        map.GetExtraCost(1, 1).ShouldBe(5);
    }

    [Fact]
    public void FromRows_BuildsTheBlockedPattern()
    {
        var map = PathMap.FromRows([
            "..#..",
            ".....",
            "#...#",
        ]);

        map.Width.ShouldBe(5);
        map.Height.ShouldBe(3);
        map.IsBlocked(2, 0).ShouldBeTrue();
        map.IsBlocked(0, 2).ShouldBeTrue();
        map.IsBlocked(4, 2).ShouldBeTrue();
        map.IsBlocked(0, 0).ShouldBeFalse();
    }

    [Fact]
    public void FromRows_RaggedRows_Throw()
    {
        Should.Throw<ArgumentException>(() => PathMap.FromRows(["...", ".."]));
        Should.Throw<ArgumentException>(() => PathMap.FromRows([]));
        Should.Throw<ArgumentException>(() => PathMap.FromRows([""]));
    }

    [Fact]
    public void PointOverloads_RequireLayerZero()
    {
        var map = new PathMap(3, 3);

        map.Contains(new PathPoint(1, 1)).ShouldBeTrue();
        map.Contains(new PathPoint(1, 1, 1)).ShouldBeFalse();
        Should.Throw<ArgumentOutOfRangeException>(() => map.IsBlocked(new PathPoint(1, 1, 1)));
    }
}
