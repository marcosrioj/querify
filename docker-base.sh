#!/usr/bin/env bash
set -e

export REDIS_PASSWORD="${REDIS_PASSWORD:-RedisTempPassword}"

echo ""
printf "\e[32m%s\e[0m\n" "======================================================================="
printf "\e[32m%s\e[0m\n" "Stopping BaseFaq base services (project only)..."
printf "\e[32m%s\e[0m\n" "======================================================================="
echo ""

docker compose -p bf_baseservices -f ./docker/docker-compose.baseservices.yml down --remove-orphans

echo ""
printf "\e[32m%s\e[0m\n" "======================================================================="
printf "\e[32m%s\e[0m\n" "Starting base services..."
printf "\e[32m%s\e[0m\n" "======================================================================="
echo ""

docker network inspect bf-network >/dev/null 2>&1 || docker network create bf-network

docker compose -p bf_baseservices -f ./docker/docker-compose.baseservices.yml up -d --force-recreate --no-build --remove-orphans --wait

username="postgres"
password="Pass123$"
command="PGPASSWORD=$password psql -U $username -d postgres -f /docker-entrypoint-initdb.d/create_databases.sql"

docker exec -i postgres sh -c "$command"
