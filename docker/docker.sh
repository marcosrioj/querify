#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
COMPOSE_ARGS=(
  -f "$SCRIPT_DIR/docker-compose.backend.yml"
  -f "$SCRIPT_DIR/docker-compose.frontend.yml"
)

print_banner() {
  echo ""
  printf "\e[32m%s\e[0m\n" "======================================================================="
  printf "\e[32m%s\e[0m\n" "$1"
  printf "\e[32m%s\e[0m\n" "======================================================================="
  echo ""
}

print_banner "Removing Docker Containers..."

docker compose -p bf_services "${COMPOSE_ARGS[@]}" down --remove-orphans

print_banner "BaseFaq Docker Images..."

docker images --format '{{.Repository}} {{.ID}}' | awk '$1 ~ /^basefaq/ {print $2}' | xargs -r docker rmi -f

print_banner "Starting Docker Containers..."

docker compose -p bf_services "${COMPOSE_ARGS[@]}" up -d --build

echo ""
printf "\e[32m%s\e[0m\n" "Services started"

print_banner "Cleaning Docker..."

docker image prune -f
