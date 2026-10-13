# Pathfinding

Grid pathfinding over 2D maps. A map is a grid of tiles (`PathMap`) or a stack of grids connected
by stairs (`PathMapStack`). Movement is 8-way: straight steps cost 10, diagonal steps cost 14,
and a diagonal needs at least one walkable orthogonal neighbour. Tiles can carry an extra cost
for slow terrain. Layers change only through stairs.

## How it works

`Pathfinder` searches from a start point to a goal and returns a `PathResult` (status, route,
cost). Give it a map, then reuse it:

```csharp
var map = new PathMap(100, 80);
map.SetBlocked(40, 0, 1, 60);
using var finder = new Pathfinder(map);
if (finder.TryFindPath(new PathPoint(0, 0), new PathPoint(90, 70), out PathResult route))
{
    foreach (PathPoint step in route.Points) { }
}
```

Bad input never throws: out-of-bounds and blocked points come back as a status (`Found`,
`NoPath`, `StartBlocked`, `GoalBlocked`, `Invalid`). Construction does the preprocessing, so
build one finder and reuse it. A finder handles one query at a time (not thread-safe); use
`PathfinderPool` for parallel queries.

## Choosing an algorithm

| `PathfinderAlgorithm` | Terrain costs | Stacked maps | Safe on maps edited after construction | Preprocessing |
|---|---|---|---|---|
| `Default` | yes | yes | yes | none |
| `BitboardAStar` | no | yes | **no** | light |
| `JumpPointSearch` | no | no | yes | none |
| `JpsPlus` | no | no | **no** | heavy |
| `HpaStar` | no | yes | **no** | heavy |

`CrowdField` is a separate type (flow field), not a `PathfinderAlgorithm`.

### Default (`PathfinderAlgorithm.Default`)

A* with a bucket-list open list and an octile heuristic.
Use it for everything by default: terrain costs, stacked maps, maps that change between queries,
searches split across calls (`BeginSearch` + `Step`), nearest-target queries. Specialized
engines only pay off on static, uniform-cost maps.

### BitboardAStar

A* whose legal moves per tile are a precomputed 16-bit mask, so expansion skips neighbour tests.
Use it on static, uniform-cost maps that are cluttered or stacked when Default is not fast enough.
Don't use it with terrain costs (constructor rejects) or on maps edited after construction.

### JumpPointSearch

A* that jumps along straight lines and stops only at forced turns, so it expands far fewer tiles.
Use it on uniform-cost, single-layer maps with open space and walls — static or dynamic.
Don't use it with terrain costs or stacked maps (constructor rejects), and prefer `JpsPlus` on
very large maps where repeated scans cost more than the saved expansions.

### JpsPlus

JPS with jump and wall distances precomputed per tile and direction; the query never scans.
Use it on large, static, uniform-cost, single-layer maps — the fastest single-query choice there.
Don't use it with terrain costs, stacked maps, or maps that change; building the table in the
constructor is wasted if the finder is not reused.

### HpaStar

Splits the map into clusters, precomputes a small graph of cluster entrances, and searches that
graph instead of the whole map. The route is fast but not guaranteed cheapest.
Use it on very large, static, uniform-cost maps where near-optimal is fine.
Don't use it with terrain costs (constructor rejects), maps that change, or small maps where the
overhead does not pay off.

### CrowdField (flow field)

One Dijkstra sweep from the goal paints a distance and a next-step direction on every tile; any
number of agents then read it as they move, with optimal routes under uniform costs.
Use it for many agents sharing one goal (or a few goals); goals can change — call `SetGoal` again
to rebuild. It reads the map live, so edits between goals are seen.
Don't use it for one-off queries (`Pathfinder` is simpler), for one goal per agent, or with
terrain costs — it silently ignores extra costs.

## Dynamic or static maps?

- **Dynamic** (obstacles appear or disappear at runtime): use `Default` or `JumpPointSearch`
  (uniform, single-layer) and reuse one finder — both read the live map. `CrowdField` is safe
  too; call `SetGoal` again after edits. Avoid `BitboardAStar`, `JpsPlus` and `HpaStar`: they
  keep using numbers computed at construction and can return routes through tiles that are
  blocked now.
- **Static** (built once, searched many times): `JpsPlus` for large uniform single-layer maps,
  `HpaStar` for very large maps where near-optimal is fine, `Default` everywhere else.

## Rules of thumb

- Only `Default` supports `SetExtraCost` terrain. Uniform-only engines throw
  `InvalidOperationException` at construction if the map has extra costs; JPS and JPS+ also throw
  on stacked maps.
- One `Pathfinder` runs one query at a time; use one per thread or `PathfinderPool`.
- `Options.Smooth` is on by default: it removes waypoints but never changes `PathResult.Cost`.
- Stepped search (`BeginSearch` + `Step`) advances in steps only on `Default`; other engines
  complete on the first step.
- `TryFindPathToNearest` always uses the general-purpose engine.
- Points are `(X, Y)` with Y growing down; `Layer` only matters for `PathMapStack`.
