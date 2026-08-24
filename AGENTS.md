# Auxil Agent Configuration

## Working Principles

- Do not preserve backward compatibility (Always confirm). Remove obsolete paths instead of adding
  compatibility layers, fallbacks, or migrations.
- Choose the simplest implementation that fully meets the current requirements. Avoid speculative
  abstractions, configuration, and indirection.
- Grow the system in layers. Start from the smallest version that works end to end, and add each new
  capability on top of a product that already works. Never trade a working product for unfinished
  complexity.
- Keep components modular and concerns clearly separated.
- Prefer established, well-maintained libraries when they reduce overall complexity or improve
  reliability. Do not reimplement common functionality without a clear reason.
- Lean on the dependencies already in the project before writing your own implementation or adding
  packages. Do not assume a library lacks a capability without checking its documentation and types.
- Make architectural decisions for the long term. Do not accept a stopgap that only works for now and
  is meant to be replaced later.

## Repository Essentials

- Read all `.csproj` package references at the start of every conversation — dependencies change frequently.
- Build: `dotnet build` · Test: `dotnet test` · Coverage: `dotnet test --collect:"XPlat Code Coverage"`
- ReSharper gate: run `tools\inspect.ps1` (runs `dotnet jb inspectcode` with team settings from
  `Auxil.sln.DotSettings`) after code changes and keep the SUGGESTION-level report clean;
  use `// ReSharper disable once <RuleId>` for deliberate deviations.
- Tests catch bugs, not numbers — every test must assert behavior a regression could break; there are no coverage targets.
- Always use braces on all block constructs, even single-line bodies.
- Extension members (C# 14): blocks take no access modifier; sibling members are called with the
  explicit receiver; extension properties work on reference/interface receivers only (value-type
  receivers resolve to the method form — keep those as classic `this` methods); nullable receivers
  stay classic `this` methods.
