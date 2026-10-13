using Auxil.Pathfinding.Engine;

namespace Auxil.Pathfinding;

/// <summary>
/// A search that can be advanced a little at a time, so a long query can be spread over several
/// calls instead of blocking one. Create one with <see cref="Pathfinder.BeginSearch"/>:
///
/// <code>
/// PathSearch search = finder.BeginSearch(start, goal);
/// while (search.Step(maxNodes: 200) is null)
/// {
///     // do other work between steps
/// }
/// PathResult result = search.GetResult();
/// </code>
///
/// Only the default algorithm advances in steps; the other algorithms run to completion on the
/// first <see cref="Step"/> call. One <see cref="Pathfinder"/> handles one search at a time, so do
/// not interleave a stepped search with other queries on the same instance.
/// </summary>
public sealed class PathSearch
{
    private readonly Pathfinder _finder;
    private readonly PathPoint _start;
    private readonly PathPoint _goal;
    private readonly WeightedAStar? _steppable;
    private readonly bool _immediateGoal;
    private readonly int _startId;
    private readonly int _goalId;
    private int _cost;
    private int _expanded;
    private PathStatus _status;
    private bool _complete;

    internal PathSearch(Pathfinder finder, PathPoint start, PathPoint goal)
    {
        _finder = finder;
        _start = start;
        _goal = goal;

        if (!finder.World.Contains(start) || !finder.World.Contains(goal))
        {
            Complete(PathStatus.Invalid, 0);
            return;
        }

        if (finder.World.IsBlocked(start))
        {
            Complete(PathStatus.StartBlocked, 0);
            return;
        }

        if (finder.World.IsBlocked(goal))
        {
            Complete(PathStatus.GoalBlocked, 0);
            return;
        }

        _startId = finder.World.ToIndex(start);
        _goalId = finder.World.ToIndex(goal);
        if (_startId == _goalId)
        {
            _immediateGoal = true;
            Complete(PathStatus.Found, 0);
            return;
        }

        _steppable = finder.SteppableEngine;
        _steppable?.BeginSearch(_startId, _goalId);
    }

    /// <summary>True once the search has stopped, successfully or not.</summary>
    public bool IsComplete => _complete;

    /// <summary>True when the search completed and found a route.</summary>
    public bool Found => _complete && _status == PathStatus.Found;

    /// <summary>How many tiles the search expanded so far.</summary>
    public int NodesExpanded => _expanded;

    /// <summary>
    /// Runs up to <paramref name="maxNodes"/> expansions and reports where the search stands:
    /// null while it is still running, otherwise the final <see cref="PathStatus"/>. Calling it
    /// again after completion returns the same status.
    /// </summary>
    /// <param name="maxNodes">Expansion budget for this step; values below 1 are treated as 1.</param>
    public PathStatus? Step(int maxNodes)
    {
        ObjectDisposedException.ThrowIf(_finder.Disposed, _finder);

        if (_complete)
        {
            return _status;
        }

        if (_steppable is not null)
        {
            SearchStepStatus stepStatus = _steppable.Step(maxNodes);
            _expanded = _steppable.NodesExpanded;
            if (stepStatus == SearchStepStatus.Found)
            {
                Complete(PathStatus.Found, _steppable.ResultCost);
            }
            else if (stepStatus == SearchStepStatus.NoPath)
            {
                Complete(PathStatus.NoPath, 0);
            }

            return _complete ? _status : null;
        }

        // Algorithms without stepped search run to completion on the first step.
        bool found = _finder.Engine.FindPath(_startId, _goalId, out int cost, out int expanded);
        _expanded = expanded;
        Complete(found ? PathStatus.Found : PathStatus.NoPath, found ? cost : 0);
        return _status;
    }

    /// <summary>
    /// The outcome of the finished search. Throws when the search is still running, so call it
    /// only after <see cref="Step"/> has returned a status.
    /// </summary>
    public PathResult GetResult()
    {
        ObjectDisposedException.ThrowIf(_finder.Disposed, _finder);

        if (!_complete)
        {
            throw new InvalidOperationException("The search is still running; call Step until it returns a status.");
        }

        if (_status != PathStatus.Found)
        {
            return PathResult.Failure(_status, _goal, _expanded);
        }

        if (_immediateGoal)
        {
            return PathResult.Success(_goal, [_start], 0, 0);
        }

        return _finder.BuildResult(_goal, _goalId, _cost, _expanded, PathStatus.Found);
    }

    private void Complete(PathStatus status, int cost)
    {
        _status = status;
        _cost = cost;
        _complete = true;
    }
}
