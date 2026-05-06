#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
COMPOSE_FILE="$SCRIPT_DIR/docker-compose.nginx-proxy.yml"
COMPOSE_PROJECT="qf_baseservices"
COMPOSE_SERVICE="querify.local.nginx"

RUNTIME_DIR="$SCRIPT_DIR/runtime"
NGINX_CONF_DIR="$RUNTIME_DIR/nginx/conf.d"
NGINX_CONF_FILE="$NGINX_CONF_DIR/querify-subdomains.conf"
NGINX_CERT_DIR="$SCRIPT_DIR/certs"
NGINX_CERT_FILE="$NGINX_CERT_DIR/dev.querify.net.crt"
NGINX_CERT_KEY_FILE="$NGINX_CERT_DIR/dev.querify.net.key"
HOSTS_BACKUP_DIR="$RUNTIME_DIR/hosts-backups"

HOSTS_FILE="/etc/hosts"
HOSTS_MARKER_BEGIN="# >>> QUERIFY LOCAL SUBDOMAINS >>>"
HOSTS_MARKER_END="# <<< QUERIFY LOCAL SUBDOMAINS <<<"

UPSTREAM_HOST="${UPSTREAM_HOST:-host.docker.internal}"
HOST_IP="${HOST_IP:-127.0.0.1}"

TENANT_BACKOFFICE_PORT="${TENANT_BACKOFFICE_PORT:-5000}"
TENANT_PUBLIC_PORT="${TENANT_PUBLIC_PORT:-5004}"
TENANT_PORTAL_PORT="${TENANT_PORTAL_PORT:-5002}"
PORTAL_APP_PORT="${PORTAL_APP_PORT:-5500}"
QNA_PORTAL_PORT="${QNA_PORTAL_PORT:-5010}"
QNA_PUBLIC_PORT="${QNA_PUBLIC_PORT:-5020}"
TEST_PORT="${TEST_PORT:-5999}"

check_dependencies() {
  if ! command -v docker >/dev/null 2>&1; then
    echo "docker is required but not installed."
    exit 1
  fi

  if ! docker compose version >/dev/null 2>&1; then
    echo "docker compose (v2) is required but not available."
    exit 1
  fi

  if [[ ! -f "$NGINX_CERT_FILE" || ! -f "$NGINX_CERT_KEY_FILE" ]]; then
    echo "TLS files are required for HTTPS listener support."
    echo "Missing files:"
    echo "  $NGINX_CERT_FILE"
    echo "  $NGINX_CERT_KEY_FILE"
    exit 1
  fi

  if [[ "${EUID:-$(id -u)}" -ne 0 ]] && ! command -v sudo >/dev/null 2>&1; then
    echo "root or sudo is required to update /etc/hosts."
    exit 1
  fi
}

create_directories() {
  mkdir -p "$NGINX_CONF_DIR"
  mkdir -p "$HOSTS_BACKUP_DIR"
}

write_nginx_config() {
  cat > "$NGINX_CONF_FILE" <<EOF
map \$http_upgrade \$connection_upgrade {
    default upgrade;
    '' close;
}

server {
    listen 80 default_server;
    server_name _;
    return 404;
}

server {
    listen 443 ssl default_server;
    server_name _;
    ssl_certificate /etc/nginx/certs/dev.querify.net.crt;
    ssl_certificate_key /etc/nginx/certs/dev.querify.net.key;
    return 404;
}

server {
    listen 80;
    listen 443 ssl;
    server_name dev.portal.querify.net;
    ssl_certificate /etc/nginx/certs/dev.querify.net.crt;
    ssl_certificate_key /etc/nginx/certs/dev.querify.net.key;

    location /api/tenant/ {
        proxy_pass http://$UPSTREAM_HOST:$TENANT_PORTAL_PORT;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection \$connection_upgrade;
    }

    location /api/user/ {
        proxy_pass http://$UPSTREAM_HOST:$TENANT_PORTAL_PORT;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection \$connection_upgrade;
    }

    location /api/qna/ {
        proxy_pass http://$UPSTREAM_HOST:$QNA_PORTAL_PORT;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection \$connection_upgrade;
    }

    location / {
        proxy_pass http://$UPSTREAM_HOST:$PORTAL_APP_PORT;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection \$connection_upgrade;
    }
}

server {
    listen 80;
    listen 443 ssl;
    server_name dev.tenant.backoffice.querify.net;
    ssl_certificate /etc/nginx/certs/dev.querify.net.crt;
    ssl_certificate_key /etc/nginx/certs/dev.querify.net.key;

    location / {
        proxy_pass http://$UPSTREAM_HOST:$TENANT_BACKOFFICE_PORT;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection \$connection_upgrade;
    }
}

server {
    listen 80;
    listen 443 ssl;
    server_name dev.tenant.portal.querify.net;
    ssl_certificate /etc/nginx/certs/dev.querify.net.crt;
    ssl_certificate_key /etc/nginx/certs/dev.querify.net.key;

    location / {
        proxy_pass http://$UPSTREAM_HOST:$TENANT_PORTAL_PORT;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection \$connection_upgrade;
    }
}

server {
    listen 80;
    listen 443 ssl;
    server_name dev.tenant.public.querify.net;
    ssl_certificate /etc/nginx/certs/dev.querify.net.crt;
    ssl_certificate_key /etc/nginx/certs/dev.querify.net.key;

    location / {
        proxy_pass http://$UPSTREAM_HOST:$TENANT_PUBLIC_PORT;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection \$connection_upgrade;
    }
}

server {
    listen 80;
    listen 443 ssl;
    server_name dev.qna.portal.querify.net;
    ssl_certificate /etc/nginx/certs/dev.querify.net.crt;
    ssl_certificate_key /etc/nginx/certs/dev.querify.net.key;

    location / {
        proxy_pass http://$UPSTREAM_HOST:$QNA_PORTAL_PORT;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection \$connection_upgrade;
    }
}

server {
    listen 80;
    listen 443 ssl;
    server_name dev.qna.public.querify.net;
    ssl_certificate /etc/nginx/certs/dev.querify.net.crt;
    ssl_certificate_key /etc/nginx/certs/dev.querify.net.key;

    location / {
        proxy_pass http://$UPSTREAM_HOST:$QNA_PUBLIC_PORT;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection \$connection_upgrade;
    }
}

server {
    listen 80;
    listen 443 ssl;
    server_name dev.test.querify.net *.test.querify.net;
    ssl_certificate /etc/nginx/certs/dev.querify.net.crt;
    ssl_certificate_key /etc/nginx/certs/dev.querify.net.key;

    location / {
        proxy_pass http://$UPSTREAM_HOST:$TEST_PORT;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection \$connection_upgrade;
    }
}
EOF
}

update_hosts_file() {
  local backup_file tmp_file

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

  {
    printf "\n%s\n" "$HOSTS_MARKER_BEGIN"
    printf "%s dev.portal.querify.net\n" "$HOST_IP"
    printf "%s dev.tenant.backoffice.querify.net\n" "$HOST_IP"
    printf "%s dev.tenant.public.querify.net\n" "$HOST_IP"
    printf "%s dev.tenant.portal.querify.net\n" "$HOST_IP"
    printf "%s dev.qna.portal.querify.net\n" "$HOST_IP"
    printf "%s dev.qna.public.querify.net\n" "$HOST_IP"
    printf "%s dev.test.querify.net\n" "$HOST_IP"
    printf "%s\n" "$HOSTS_MARKER_END"
  } >> "$tmp_file"

  if [[ "${EUID:-$(id -u)}" -eq 0 ]]; then
    cp "$tmp_file" "$HOSTS_FILE"
  else
    sudo cp "$tmp_file" "$HOSTS_FILE"
  fi
  rm -f "$tmp_file"
}

start_proxy() {
  COMPOSE_IGNORE_ORPHANS=1 docker compose -p "$COMPOSE_PROJECT" -f "$COMPOSE_FILE" up -d --force-recreate --no-deps "$COMPOSE_SERVICE"
}

verify_proxy_reachable() {
  local status
  for _ in {1..20}; do
    status="$(curl -sS -o /dev/null -w "%{http_code}" -H "Host: dev.qna.public.querify.net" http://127.0.0.1/ || true)"
    if [[ "$status" != "000" && -n "$status" ]]; then
      return
    fi
    sleep 0.5
  done

  echo "Proxy health check failed: port 80 is not reachable on this machine."
  echo "Verify external/host port 80 availability and Docker port publishing."
  exit 1
}

verify_https_reachable() {
  local status
  for _ in {1..20}; do
    status="$(curl -k -sS -o /dev/null -w "%{http_code}" -H "Host: dev.qna.public.querify.net" https://127.0.0.1/ || true)"
    if [[ "$status" != "000" && -n "$status" ]]; then
      return
    fi
    sleep 0.5
  done

  echo "HTTPS health check failed: port 443 is not reachable on this machine."
  echo "Verify external/host port 443 availability and TLS files in $NGINX_CERT_DIR."
  exit 1
}

print_summary() {
  echo
  echo "Querify local subdomain proxy is ready."
  echo "Docker compose project: $COMPOSE_PROJECT"
  echo "Upstream host: $UPSTREAM_HOST"
  echo
  echo "Domain mappings:"
  echo "  dev.portal.querify.net            -> $UPSTREAM_HOST:$PORTAL_APP_PORT"
  echo "  dev.portal.querify.net/api/tenant -> $UPSTREAM_HOST:$TENANT_PORTAL_PORT"
  echo "  dev.portal.querify.net/api/user   -> $UPSTREAM_HOST:$TENANT_PORTAL_PORT"
  echo "  dev.portal.querify.net/api/qna    -> $UPSTREAM_HOST:$QNA_PORTAL_PORT"
  echo "  dev.tenant.backoffice.querify.net -> $UPSTREAM_HOST:$TENANT_BACKOFFICE_PORT"
  echo "  dev.tenant.public.querify.net     -> $UPSTREAM_HOST:$TENANT_PUBLIC_PORT"
  echo "  dev.tenant.portal.querify.net     -> $UPSTREAM_HOST:$TENANT_PORTAL_PORT"
  echo "  dev.qna.portal.querify.net        -> $UPSTREAM_HOST:$QNA_PORTAL_PORT"
  echo "  dev.qna.public.querify.net        -> $UPSTREAM_HOST:$QNA_PUBLIC_PORT"
  echo "  dev.test.querify.net              -> $UPSTREAM_HOST:$TEST_PORT"
  echo
  echo "Generated Nginx config:"
  echo "  $NGINX_CONF_FILE"
  echo
  echo "Forward external 80 -> machine 80."
  echo "Forward external 443 -> machine 443."
}

main() {
  check_dependencies
  create_directories
  write_nginx_config
  update_hosts_file
  start_proxy
  verify_proxy_reachable
  verify_https_reachable
  print_summary
}

main "$@"
