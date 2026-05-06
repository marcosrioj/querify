$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$composeFile = Join-Path $scriptDir "docker-compose.nginx-proxy.yml"
$composeProject = "qf_baseservices"
$composeService = "querify.local.nginx"
$runtimeDir = Join-Path $scriptDir "runtime"
$hostsBackupDir = Join-Path $runtimeDir "hosts-backups"
$hostsFile = Join-Path $env:SystemRoot "System32\drivers\etc\hosts"

$markerBegin = "# >>> QUERIFY LOCAL SUBDOMAINS >>>"
$markerEnd = "# <<< QUERIFY LOCAL SUBDOMAINS <<<"

function Test-IsAdmin {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Remove-MarkerBlock {
    param(
        [string[]]$Lines,
        [string]$Begin,
        [string]$End
    )

    $result = New-Object System.Collections.Generic.List[string]
    $skip = $false

    foreach ($line in $Lines) {
        if ($line -eq $Begin) {
            $skip = $true
            continue
        }
        if ($line -eq $End) {
            $skip = $false
            continue
        }
        if (-not $skip) {
            $null = $result.Add($line)
        }
    }

    return $result
}

if (Get-Command docker -ErrorAction SilentlyContinue) {
    try {
        $env:COMPOSE_IGNORE_ORPHANS = "1"
        docker compose -p $composeProject -f $composeFile stop $composeService
        docker compose -p $composeProject -f $composeFile rm -f $composeService
    }
    catch {
        Write-Host "Could not stop proxy container with docker compose."
    }
    finally {
        Remove-Item Env:\COMPOSE_IGNORE_ORPHANS -ErrorAction SilentlyContinue
    }
}
else {
    Write-Host "docker not found, skipping proxy shutdown."
}

if (-not (Test-IsAdmin)) {
    throw "Run this script in an elevated PowerShell session (Administrator) to update the hosts file."
}

$hostsLines = Get-Content -Path $hostsFile -ErrorAction Stop
if (-not ($hostsLines -contains $markerBegin)) {
    Write-Host "No Querify hosts block found in $hostsFile."
    exit 0
}

New-Item -ItemType Directory -Path $hostsBackupDir -Force | Out-Null
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$backupFile = Join-Path $hostsBackupDir "hosts.$timestamp.bak"
Copy-Item -Path $hostsFile -Destination $backupFile -Force

$cleanHosts = Remove-MarkerBlock -Lines $hostsLines -Begin $markerBegin -End $markerEnd
Set-Content -Path $hostsFile -Encoding ascii -Value $cleanHosts

Write-Host "Removed Querify hosts block from $hostsFile."
Write-Host "Hosts backup: $backupFile"
