using Auxil.Pathfinding;

namespace Auxil.Tests.Pathfinding;

/// <summary>
/// Black-box tests for running many queries in parallel on one map.
/// No engine types, no internals.
/// </summary>
public class PathfinderPoolTests
{
    [Fact]
    public async Task ParallelQueries_MatchSequentialResults()
    {
        var map = PathMap.FromRows([
            "..........",
            "..###.....",
            "..#.......",
            "..#..###..",
            ".....#....",
            "###..#....",
            ".....#....",
            "..........",
        ]);

        (PathPoint Start, PathPoint Goal)[] queries =
        [
            (new(0, 0), new(9, 7)),
            (new(9, 0), new(0, 7)),
            (new(0, 7), new(9, 7)),
            (new(4, 0), new(4, 7)),
            (new(9, 7), new(0, 0)),
        ];

        var expected = new List<PathResult>();
        using (var finder = new Pathfinder(map))
        {
            finder.Options.Smooth = false;
            foreach ((PathPoint start, PathPoint goal) in queries)
            {
                expected.Add(finder.FindPath(start, goal));
            }
        }

        using var pool = new PathfinderPool(map, maxConcurrency: 4);
        Task<PathResult>[] tasks = queries.Select(q => pool.FindPathAsync(q.Start, q.Goal)).ToArray();
        PathResult[] results = await Task.WhenAll(tasks);

        for (int i = 0; i < queries.Length; i++)
        {
            results[i].Status.ShouldBe(expected[i].Status);
            results[i].Cost.ShouldBe(expected[i].Cost);
        }
    }

    [Fact]
    public void Pool_ExposesItsMapAndAlgorithm()
    {
        var map = new PathMap(5, 5);
        using var pool = new PathfinderPool(map);

        pool.Map.ShouldBeSameAs(map);
        pool.Stack.ShouldBeNull();
        pool.Algorithm.ShouldBe(PathfinderAlgorithm.Default);
    }

    [Fact]
    public async Task ManyQueries_AllSucceed()
    {
        using var pool = new PathfinderPool(new PathMap(100, 100), maxConcurrency: 8);

        Task<PathResult>[] tasks = new Task<PathResult>[64];
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = pool.FindPathAsync(new PathPoint(0, i), new PathPoint(99, 99 - i));
        }

        PathResult[] results = await Task.WhenAll(tasks);
        results.ShouldAllBe(r => r.Found);
    }

    [Fact]
    public async Task CancelledBeforeStart_Throws()
    {
        using var pool = new PathfinderPool(new PathMap(5, 5), maxConcurrency: 1);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(
            () => pool.FindPathAsync(new PathPoint(0, 0), new PathPoint(4, 4), cancellation.Token));
    }

    [Fact]
    public async Task DisposedPool_Throws()
    {
        var pool = new PathfinderPool(new PathMap(5, 5));
        pool.Dispose();

        await Should.ThrowAsync<ObjectDisposedException>(
            () => pool.FindPathAsync(new PathPoint(0, 0), new PathPoint(4, 4)));
    }

    [Fact]
    public async Task Pool_WithAUniformAlgorithm_Works()
    {
        var map = PathMap.FromRows([
            "..#..",
            "..#..",
            "..#..",
            "..#..",
            ".....",
        ]);
        using var pool = new PathfinderPool(map, PathfinderAlgorithm.JpsPlus, maxConcurrency: 2);

        PathResult result = await pool.FindPathAsync(new PathPoint(0, 0), new PathPoint(4, 0));

        result.Found.ShouldBeTrue();
        result.Cost.ShouldBe(96);
    }
}
