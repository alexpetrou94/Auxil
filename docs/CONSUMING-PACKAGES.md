# Consuming Auxil from GitHub Packages

Auxil is published to the owner's GitHub Packages feed.

## Package Reference

```xml
<PackageReference Include="Auxil" Version="0.1.0" />
```

Or install it from the command line:

```powershell
dotnet add package Auxil --version 0.1.0 --source GitHub-Auxil
```

## GitHub Packages Source

```powershell
dotnet nuget add source "https://nuget.pkg.github.com/alexpetrou94/index.json" `
  --name "GitHub-Auxil" `
  --username YOUR_GITHUB_USERNAME `
  --password YOUR_PAT_TOKEN `
  --store-password-in-clear-text
```

Use a Personal Access Token (classic) with only the `read:packages` scope for
consuming the package. Keep credentials in the user NuGet configuration or an
environment-specific secret; never commit them to this repository.
