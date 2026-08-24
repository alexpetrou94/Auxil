$ErrorActionPreference = 'Stop'

$ContainerName = 'uphoric-lab'
$ImageName = 'uphoric-lab'
$VolumeName = 'uphoric-lab-nuget'
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent (Split-Path -Parent $ScriptDir)
$TempRoot = Join-Path $env:LOCALAPPDATA 'Temp\opencode'

function Test-DockerOk {
    param([Parameter(Mandatory = $true)][string[]]$DockerArgs)
    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'SilentlyContinue'
    & docker @DockerArgs 2>$null | Out-Null
    $ok = ($LASTEXITCODE -eq 0)
    $ErrorActionPreference = $previous
    return $ok
}

function Invoke-LabEnsure {
    if (-not (Test-DockerOk @('image', 'inspect', $ImageName))) {
        & docker build -t $ImageName $ScriptDir
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to build lab image"
        }
    }

    if (-not (Test-DockerOk @('inspect', $ContainerName))) {
        & docker run -d --name $ContainerName --restart unless-stopped --memory 2g `
            -v "${VolumeName}:/root/.nuget/packages" `
            -v "${ProjectRoot}:/repo:ro" `
            -w /lab `
            $ImageName sleep infinity
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to create lab container"
        }
        Start-Sleep -Seconds 2
        if (-not (Test-DockerOk @('inspect', $ContainerName))) {
            throw "Lab container not created"
        }
    }

    $running = & docker ps --filter "name=^/${ContainerName}$" --format '{{.Names}}'
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to query lab container"
    }
    if (-not $running) {
        & docker start $ContainerName
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to start lab container"
        }
    }
}

function Get-LabPath {
    param([string]$Path)
    $normalized = $Path -replace '\\', '/'
    $normalized = $normalized.TrimEnd('/')
    if ($normalized -eq '') {
        throw 'Lab path is empty'
    }
    if (-not $normalized.StartsWith('/')) {
        $normalized = "/$normalized"
    }
    return $normalized
}

$Command = $args[0]
$Rest = @($args[1..($args.Count - 1)])

switch ($Command) {
    'up' {
        Invoke-LabEnsure
        Write-Host "Lab is up (container: $ContainerName, project mounted read-only at /repo)"
    }
    'exec' {
        if ($Rest.Count -eq 0) {
            throw 'exec requires a command'
        }
        Invoke-LabEnsure
        & docker exec -i -w /lab $ContainerName @Rest
        exit $LASTEXITCODE
    }
    'in' {
        if ($Rest.Count -ne 2) {
            throw 'in requires <hostPath> <labPath>'
        }
        $hostPath = $Rest[0]
        $labPath = Get-LabPath $Rest[1]
        Invoke-LabEnsure
        $parent = $labPath.Substring(0, $labPath.LastIndexOf('/'))
        if ($parent -ne '') {
            & docker exec $ContainerName mkdir -p -- $parent
            if ($LASTEXITCODE -ne 0) {
                throw 'Failed to create destination directory'
            }
        }
        & docker cp $hostPath "${ContainerName}:${labPath}"
        if ($LASTEXITCODE -ne 0) {
            throw 'Failed to copy into lab'
        }
    }
    'in-stdin' {
        if ($Rest.Count -ne 1) {
            throw 'in-stdin requires <labPath>'
        }
        $labPath = Get-LabPath $Rest[0]
        Invoke-LabEnsure
        $parent = $labPath.Substring(0, $labPath.LastIndexOf('/'))
        if ($parent -ne '') {
            & docker exec $ContainerName mkdir -p -- $parent
            if ($LASTEXITCODE -ne 0) {
                throw 'Failed to create destination directory'
            }
        }
        $content = [Console]::In.ReadToEnd()
        $base64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($content))
        $bashCommand = "echo $base64 | base64 -d > '$labPath'"
        & docker exec $ContainerName bash -lc $bashCommand
        if ($LASTEXITCODE -ne 0) {
            throw 'Failed to write into lab'
        }
    }
    'out' {
        if ($Rest.Count -ne 1) {
            throw 'out requires <labPath>'
        }
        $labPath = Get-LabPath $Rest[0]
        if (-not (Test-Path -LiteralPath $TempRoot)) {
            New-Item -ItemType Directory -Path $TempRoot | Out-Null
        }
        Invoke-LabEnsure
        & docker cp "${ContainerName}:${labPath}" $TempRoot
        if ($LASTEXITCODE -ne 0) {
            throw 'Failed to copy out of lab'
        }
        $name = Split-Path $labPath -Leaf
        Write-Host (Join-Path $TempRoot $name)
    }
    'status' {
        Invoke-LabEnsure
        & docker ps --filter "name=^/${ContainerName}$" --format "{{.Names}}`t{{.Status}}`t{{.Image}}"
        & docker volume ls --format '{{.Name}}' | Where-Object { $_ -eq $VolumeName }
    }
    'destroy' {
        if (Test-DockerOk @('inspect', $ContainerName)) {
            & docker rm -f $ContainerName
        }
        if (Test-DockerOk @('volume', 'inspect', $VolumeName)) {
            & docker volume rm $VolumeName
        }
        Write-Host 'Lab destroyed'
    }
    default {
        throw "Unknown command '$Command'. Valid commands: up, exec, in, in-stdin, out, status, destroy"
    }
}
