namespace Auxil.Pathfinding;

/// <summary>
/// A tile on a map, addressed as (X, Y). X grows to the right, Y grows downwards. <see cref="Layer"/>
/// is always 0 unless the map is a stack of maps.
/// </summary>
/// <param name="X">Horizontal position.</param>
/// <param name="Y">Vertical position.</param>
/// <param name="Layer">Layer index for stacked maps; 0 for normal maps.</param>
public readonly record struct PathPoint(int X, int Y, int Layer = 0);
