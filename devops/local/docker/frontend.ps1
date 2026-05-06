$ErrorActionPreference = 'Stop'

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ComposeFile = Join-Path $ScriptDir 'docker-compose.frontend.yml'
$Service = 'querify.portal.app'

function Write-Banner {
  param([string]$Message)

  Write-Host ""
  Write-Host "=======================================================================" -ForegroundColor Green
  Write-Host $Message -ForegroundColor Green
  Write-Host "=======================================================================" -ForegroundColor Green
  Write-Host ""
}

Write-Banner "Removing Frontend Containers..."

docker compose -p qf_services -f $ComposeFile stop $Service 2>$null | Out-Null
docker compose -p qf_services -f $ComposeFile rm -f $Service 2>$null | Out-Null

Write-Banner "Querify Frontend Docker Images..."

$images = docker images --format '{{.Repository}} {{.ID}}' |
  Where-Object { $_ -match '^querify\.portal\.app\s+' } |
  ForEach-Object { $_.Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries)[1] }

if ($images) {
  docker rmi -f $images
}

Write-Banner "Starting Frontend Containers..."

docker compose -p qf_services -f $ComposeFile up -d --build $Service

Write-Host ""
Write-Host "Frontend services started" -ForegroundColor Green

Write-Banner "Cleaning Docker..."

docker image prune -f
