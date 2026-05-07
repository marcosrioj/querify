$ErrorActionPreference = 'Stop'

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ComposeFile = Join-Path $ScriptDir 'docker-compose.baseservices.yml'

function Write-Banner {
  param([string]$Message)

  Write-Host ""
  Write-Host "=======================================================================" -ForegroundColor Green
  Write-Host $Message -ForegroundColor Green
  Write-Host "=======================================================================" -ForegroundColor Green
  Write-Host ""
}

if (-not $env:REDIS_PASSWORD) {
  $env:REDIS_PASSWORD = 'RedisTempPassword'
}

Write-Banner "Stopping Querify base services (project only)..."

docker compose -p qf_baseservices -f $ComposeFile down --remove-orphans

Write-Banner "Starting base services..."

$networkExists = docker network inspect qf-network 2>$null
if (-not $networkExists) {
  docker network create qf-network
}

docker compose -p qf_baseservices -f $ComposeFile up -d --force-recreate --no-build --remove-orphans --wait

$username = 'postgres'
$password = 'Pass123$'
$command = "PGPASSWORD=$password psql -U $username -d postgres -f /docker-entrypoint-initdb.d/create_databases.sql"

docker exec -i postgres sh -c $command
