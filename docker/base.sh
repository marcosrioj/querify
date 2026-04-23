#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
COMPOSE_FILE="$SCRIPT_DIR/docker-compose.baseservices.yml"

print_banner() {
  echo ""
  printf "\e[32m%s\e[0m\n" "======================================================================="
  printf "\e[32m%s\e[0m\n" "$1"
  printf "\e[32m%s\e[0m\n" "======================================================================="
  echo ""
}

export REDIS_PASSWORD="${REDIS_PASSWORD:-RedisTempPassword}"

print_banner "Stopping BaseFaq base services (project only)..."

docker compose -p bf_baseservices -f "$COMPOSE_FILE" down --remove-orphans

print_banner "Starting base services..."

docker network inspect bf-network >/dev/null 2>&1 || docker network create bf-network

docker compose -p bf_baseservices -f "$COMPOSE_FILE" up -d --force-recreate --no-build --remove-orphans --wait

username="postgres"
password="Pass123$"
command="PGPASSWORD=$password psql -U $username -d postgres -f /docker-entrypoint-initdb.d/create_databases.sql"

docker exec -i postgres sh -c "$command"
