$ErrorActionPreference = 'Stop'

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ComposeArgs = @(
  '-f', (Join-Path $ScriptDir 'docker-compose.backend.yml'),
  '-f', (Join-Path $ScriptDir 'docker-compose.frontend.yml')
)

function Write-Banner {
  param([string]$Message)

  Write-Host ""
  Write-Host "=======================================================================" -ForegroundColor Green
  Write-Host $Message -ForegroundColor Green
  Write-Host "=======================================================================" -ForegroundColor Green
  Write-Host ""
}

Write-Banner "Removing Docker Containers..."

docker compose -p bf_services @ComposeArgs down --remove-orphans

Write-Banner "BaseFaq Docker Images..."

$images = docker images --format '{{.Repository}} {{.ID}}' |
  Where-Object { $_ -match '^basefaq\.[^ ]+\s+' } |
  ForEach-Object { $_.Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries)[1] }

if ($images) {
  docker rmi -f $images
}

Write-Banner "Starting Docker Containers..."

docker compose -p bf_services @ComposeArgs up -d --build

Write-Host ""
Write-Host "Services started" -ForegroundColor Green

Write-Banner "Cleaning Docker..."

docker image prune -f
