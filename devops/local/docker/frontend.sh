#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
COMPOSE_FILE="$SCRIPT_DIR/docker-compose.frontend.yml"
SERVICE="querify.portal.app"

print_banner() {
  echo ""
  printf "\e[32m%s\e[0m\n" "======================================================================="
  printf "\e[32m%s\e[0m\n" "$1"
  printf "\e[32m%s\e[0m\n" "======================================================================="
  echo ""
}

print_banner "Removing Frontend Containers..."

docker compose -p qf_services -f "$COMPOSE_FILE" stop "$SERVICE" >/dev/null 2>&1 || true
docker compose -p qf_services -f "$COMPOSE_FILE" rm -f "$SERVICE" >/dev/null 2>&1 || true

print_banner "Querify Frontend Docker Images..."

docker images --format '{{.Repository}} {{.ID}}' | awk '$1 == "querify.portal.app" {print $2}' | xargs -r docker rmi -f

print_banner "Starting Frontend Containers..."

docker compose -p qf_services -f "$COMPOSE_FILE" up -d --build "$SERVICE"

echo ""
printf "\e[32m%s\e[0m\n" "Frontend services started"

print_banner "Cleaning Docker..."

docker image prune -f
