namespace Auxil.Pathfinding;

/// <summary>Why a path request ended the way it did.</summary>
public enum PathStatus
{
    /// <summary>A route was found.</summary>
    Found,

    /// <summary>No route exists between the two points.</summary>
    NoPath,

    /// <summary>The start tile is blocked.</summary>
    StartBlocked,

    /// <summary>The goal tile is blocked.</summary>
    GoalBlocked,

    /// <summary>The start or goal lies outside the map.</summary>
    Invalid,
}
