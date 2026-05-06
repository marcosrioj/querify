$ErrorActionPreference = 'Stop'

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ComposeFile = Join-Path $ScriptDir 'docker-compose.backend.yml'
$Services = @(
  'querify.qna.portal.api',
  'querify.tenant.backoffice.api',
  'querify.tenant.portal.api',
  'querify.tenant.public.api',
  'querify.qna.public.api',
  'querify.tenant.worker.api'
)

function Write-Banner {
  param([string]$Message)

  Write-Host ""
  Write-Host "=======================================================================" -ForegroundColor Green
  Write-Host $Message -ForegroundColor Green
  Write-Host "=======================================================================" -ForegroundColor Green
  Write-Host ""
}

Write-Banner "Removing Backend Containers..."

docker compose -p qf_services -f $ComposeFile stop @Services 2>$null | Out-Null
docker compose -p qf_services -f $ComposeFile rm -f @Services 2>$null | Out-Null

Write-Banner "Querify Backend Docker Images..."

$images = docker images --format '{{.Repository}} {{.ID}}' |
  Where-Object { $_ -match '^querify\.(tenant|qna)\.[^ ]+\s+' } |
  ForEach-Object { $_.Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries)[1] }

if ($images) {
  docker rmi -f $images
}

Write-Banner "Starting Backend Containers..."

docker compose -p qf_services -f $ComposeFile up -d --build @Services

Write-Host ""
Write-Host "Backend services started" -ForegroundColor Green

Write-Banner "Cleaning Docker..."

docker image prune -f
