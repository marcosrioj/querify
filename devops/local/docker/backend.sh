#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
COMPOSE_FILE="$SCRIPT_DIR/docker-compose.backend.yml"
SERVICES=(
  querify.qna.portal.api
  querify.tenant.backoffice.api
  querify.tenant.portal.api
  querify.tenant.public.api
  querify.qna.public.api
  querify.tenant.worker.api
)

print_banner() {
  echo ""
  printf "\e[32m%s\e[0m\n" "======================================================================="
  printf "\e[32m%s\e[0m\n" "$1"
  printf "\e[32m%s\e[0m\n" "======================================================================="
  echo ""
}

print_banner "Removing Backend Containers..."

docker compose -p qf_services -f "$COMPOSE_FILE" stop "${SERVICES[@]}" >/dev/null 2>&1 || true
docker compose -p qf_services -f "$COMPOSE_FILE" rm -f "${SERVICES[@]}" >/dev/null 2>&1 || true

print_banner "Querify Backend Docker Images..."

docker images --format '{{.Repository}} {{.ID}}' | awk '$1 ~ /^querify\.(tenant|qna)\./ {print $2}' | xargs -r docker rmi -f

print_banner "Starting Backend Containers..."

docker compose -p qf_services -f "$COMPOSE_FILE" up -d --build "${SERVICES[@]}"

echo ""
printf "\e[32m%s\e[0m\n" "Backend services started"

print_banner "Cleaning Docker..."

docker image prune -f
