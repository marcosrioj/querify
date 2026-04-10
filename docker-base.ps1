$ErrorActionPreference = 'Stop'

if (-not $env:REDIS_PASSWORD) {
  $env:REDIS_PASSWORD = 'RedisTempPassword'
}

Write-Host ""
Write-Host "=======================================================================" -ForegroundColor Green
Write-Host "Stopping BaseFaq base services (project only)..." -ForegroundColor Green
Write-Host "=======================================================================" -ForegroundColor Green
Write-Host ""

docker compose -p bf_baseservices -f ./docker/docker-compose.baseservices.yml down --remove-orphans

Write-Host ""
Write-Host "=======================================================================" -ForegroundColor Green
Write-Host "Starting base services..." -ForegroundColor Green
Write-Host "=======================================================================" -ForegroundColor Green
Write-Host ""

$networkExists = docker network inspect bf-network 2>$null
if (-not $networkExists) {
  docker network create bf-network
}

docker compose -p bf_baseservices -f ./docker/docker-compose.baseservices.yml up -d --force-recreate --no-build --remove-orphans --wait

$username = 'postgres'
$password = 'Pass123$'
$command = "PGPASSWORD=$password psql -U $username -d postgres -f /docker-entrypoint-initdb.d/create_databases.sql"

docker exec -i postgres sh -c $command
