# Auxil

Auxil is a .NET utility library for shared application code. It collects the
small pieces that are useful across projects, including extension methods,
common result and version types, text and JSON helpers, process execution,
file watching, and code generation.

The library targets .NET 10 and uses C# 14. Its API is made up of ordinary
types and extension members, so projects can use the parts they need without
adopting a framework or a particular application structure.

## Example

```csharp
using Auxil.Extensions;

string title = "hello world".ToPascalCase();
int wordCount = "hello world".CountWords();
```

## Installation

Auxil is available from GitHub Packages. Configure the package source with a
Personal Access Token that has the `read:packages` scope:

```powershell
dotnet nuget add source "https://nuget.pkg.github.com/alexpetrou94/index.json" `
  --name "GitHub-Auxil" `
  --username YOUR_GITHUB_USERNAME `
  --password YOUR_PAT_TOKEN `
  --store-password-in-clear-text
```

Then install the package:

```powershell
dotnet add package Auxil --version 0.1.0 --source GitHub-Auxil
```

Keep the token in your user-level NuGet configuration or an environment
secret. Do not commit it to a project.

## Development

Restore, build, and run the test suite with:

```bash
dotnet test Auxil.sln
```

## License

[MIT License](https://github.com/alexpetrou94/Auxil/blob/production/LICENSE)
