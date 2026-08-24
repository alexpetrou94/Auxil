<#
.SYNOPSIS
Runs JetBrains Inspect Code (ReSharper CLI) over Auxil.sln.

.DESCRIPTION
One-time setup per machine:
  1. dotnet tool restore
  2. First run will ask for a JetBrains license activation
     (same account as Rider; free for open source).

.EXAMPLE
tools\inspect.ps1
tools\inspect.ps1 -Severity INFO
tools\inspect.ps1 -Severity SUGGESTION -Format Xml -Output inspect-results.xml
#>
param(
    [ValidateSet("INFO", "HINT", "SUGGESTION", "WARNING", "ERROR")]
    [string] $Severity = "SUGGESTION",

    [ValidateSet("Xml", "Html", "Text", "Sarif")]
    [string] $Format = "Sarif",

    [string] $Output = "inspect-results.sarif"
)

$repoRoot = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $repoRoot "Auxil.sln"
$outputPath = if ([System.IO.Path]::IsPathRooted($Output))
{
    $Output
}
else
{
    Join-Path $repoRoot $Output
}

dotnet jb inspectcode $solution --output=$outputPath --format=$Format --severity=$Severity
exit $LASTEXITCODE
