namespace Auxil.Pathfinding.Engine;

/// <summary>The state of a search that can be advanced in bounded steps.</summary>
internal enum SearchStepStatus
{
    /// <summary>Still searching.</summary>
    Running,

    /// <summary>The goal was reached.</summary>
    Found,

    /// <summary>The open list ran out; no route exists.</summary>
    NoPath,
}
