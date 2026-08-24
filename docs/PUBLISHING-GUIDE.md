# Publishing Auxil

Auxil is published to GitHub Packages from the `alexpetrou94/Auxil` repository.
The workflow builds and tests the solution for pushes to `production` and pull
requests targeting `production`. A push to `production` also publishes a
temporary CI package. Version tags publish stable packages.

## CI builds

Every push to `production` publishes a temporary package using the format
`Major.Year.DayOfYear.RunNumber`. For example, a CI package might be
`0.2026.237.12`.

Temporary CI versions are intended for validation and are not stable releases.
When a stable version tag is published, all four-part numeric CI versions are
removed from GitHub Packages. Stable versions published from tags are kept.

## Release a version

1. Update `<Version>` in `Auxil/Auxil.csproj`.
2. Add the release notes to `CHANGELOG.md`.
3. Run `dotnet test Auxil.sln` locally.
4. Create and push a tag with the `v` prefix.

```bash
git tag -a v0.1.0 -m "Release Auxil 0.1.0"
git push origin v0.1.0
```

The workflow uses the tag without the `v` prefix as the package version. For
example, `v0.1.0` publishes `Auxil` version `0.1.0`, then removes the previous
temporary CI versions.

## Package feed

```text
https://nuget.pkg.github.com/alexpetrou94/index.json
```

Publishing uses the workflow's `GITHUB_TOKEN`. Consumers need a Personal
Access Token (classic) with the `read:packages` scope.
