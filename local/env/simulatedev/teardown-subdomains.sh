#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
COMPOSE_FILE="$SCRIPT_DIR/docker-compose.nginx-proxy.yml"
COMPOSE_PROJECT="bf_baseservices"
COMPOSE_SERVICE="basefaq.local.nginx"

RUNTIME_DIR="$SCRIPT_DIR/runtime"
HOSTS_BACKUP_DIR="$RUNTIME_DIR/hosts-backups"

HOSTS_FILE="/etc/hosts"
HOSTS_MARKER_BEGIN="# >>> BASEFAQ LOCAL SUBDOMAINS >>>"
HOSTS_MARKER_END="# <<< BASEFAQ LOCAL SUBDOMAINS <<<"

stop_proxy() {
  if ! command -v docker >/dev/null 2>&1; then
    echo "docker not found, skipping proxy shutdown."
    return
  fi

  if ! docker compose version >/dev/null 2>&1; then
    echo "docker compose not found, skipping proxy shutdown."
    return
  fi

  COMPOSE_IGNORE_ORPHANS=1 docker compose -p "$COMPOSE_PROJECT" -f "$COMPOSE_FILE" stop "$COMPOSE_SERVICE" || true
  COMPOSE_IGNORE_ORPHANS=1 docker compose -p "$COMPOSE_PROJECT" -f "$COMPOSE_FILE" rm -f "$COMPOSE_SERVICE" || true
}

remove_hosts_block() {
  local tmp_file backup_file

  if ! grep -Fq "$HOSTS_MARKER_BEGIN" "$HOSTS_FILE"; then
    echo "No BaseFAQ hosts block found in $HOSTS_FILE."
    return
  fi

  if [[ "${EUID:-$(id -u)}" -ne 0 ]] && ! command -v sudo >/dev/null 2>&1; then
    echo "root or sudo is required to update /etc/hosts."
    exit 1
  fi

  mkdir -p "$HOSTS_BACKUP_DIR"
  backup_file="$HOSTS_BACKUP_DIR/hosts.$(date +%Y%m%d-%H%M%S).bak"
  tmp_file="$(mktemp)"

  if [[ "${EUID:-$(id -u)}" -eq 0 ]]; then
    cp "$HOSTS_FILE" "$backup_file"
  else
    sudo cp "$HOSTS_FILE" "$backup_file"
  fi

  awk -v begin="$HOSTS_MARKER_BEGIN" -v end="$HOSTS_MARKER_END" '
    $0 == begin { skip = 1; next }
    $0 == end { skip = 0; next }
    !skip { print }
  ' "$HOSTS_FILE" > "$tmp_file"

  if [[ "${EUID:-$(id -u)}" -eq 0 ]]; then
    cp "$tmp_file" "$HOSTS_FILE"
  else
    sudo cp "$tmp_file" "$HOSTS_FILE"
  fi
  rm -f "$tmp_file"
  echo "Removed BaseFAQ hosts block from $HOSTS_FILE."
  echo "Hosts backup: $backup_file"
}

main() {
  stop_proxy
  remove_hosts_block
}

main "$@"
