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

## Lab Environment

The lab is a disposable Docker container (`uphoric-lab`) on the host's Docker Desktop for
scratch work, assumption validation, and destructive experiments. Scratch files live inside
the container at `/lab` — never on the host.

- Helper: `tools\lab\lab.ps1`. Commands: `up` (create/start, idempotent), `exec <cmd...>`
  (run in `/lab`), `in <hostPath> <labPath>` (copy in), `in-stdin <labPath>` (write piped
  content), `out <labPath>` (copy out — writes only under the pre-approved temp dir and prints
  the host path), `status`, `destroy` (full wipe). Invoke with
  `powershell -NoProfile -ExecutionPolicy Bypass -File tools\lab\lab.ps1 <cmd> ...` — the
  machine's execution policy blocks plain script invocation.
- The project is mounted read-only at `/repo` — the repo is physically un-writable from the
  lab; never attempt to write there. Library: `/repo/Auxil/Auxil.csproj`,
  tests: `/repo/Auxil.Tests/`, solution: `/repo/Auxil.sln`.
- The NuGet cache persists in a named volume (`uphoric-lab-nuget`); everything else in the
  container is disposable. Recreate from scratch with `lab.ps1 destroy` then `lab.ps1 up`.
- Line endings: files written via `in-stdin` are UTF-8 from the host pipeline — prefer
  `in`/`docker cp` for large or binary files.
- The repo mount is read-only, so MSBuild cannot write `obj/` into it. Validation patterns:
  - Public-API behavior checks: build the library on the host (`dotnet build
    Auxil\Auxil.csproj`), copy `Auxil\bin\Debug\net10.0\` into the
    lab, and reference the DLL with `<Reference>` in a scratch project. Add the library's
    NuGet packages to the scratch project only when touching API surface that exposes
    third-party types (FastCloner, Nager.Country).
  - Full builds/test runs: snapshot the committed tree with
    `git -c core.autocrlf=false archive --format=tar HEAD Auxil Auxil.Tests
    | docker exec -i uphoric-lab tar -x -C /lab/work` (the `core.autocrlf=false` is required —
    plain `git archive` on this machine injects CRLF and breaks tests that assert exact
    output), then `lab.ps1 exec dotnet test /lab/work/Auxil.Tests`.

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
