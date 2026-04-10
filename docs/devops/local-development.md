# Local Development and Docker

## Purpose

This guide is the operational runbook for running BaseFAQ locally. It combines the repository bootstrap flow, Docker infrastructure, optional full-container execution, and local subdomain simulation.

## Recommended local model

The repository is easiest to run in this mode:

1. Start infrastructure with Docker.
2. Run the `.NET` APIs on the host.
3. Run the Portal frontend on the host.

This gives fast iteration without losing the production-like infrastructure components.

## Prerequisites

- Docker Engine and Docker Compose v2
- `.NET SDK 10.0.100`
- Node.js LTS and npm
- `dotnet dev-certs https --trust` if you want local HTTPS without certificate issues

## Step-by-step local bootstrap

### 1. Restore and build

```bash
dotnet restore BaseFaq.sln
dotnet build BaseFaq.sln --no-restore
```

### 2. Start base services

macOS/Linux:

```bash
./docker-base.sh
```

Windows PowerShell:

```powershell
.\docker-base.ps1
```

What this starts:

- PostgreSQL
- RabbitMQ
- Redis
- SMTP4Dev
- Jaeger
- Prometheus
- Alertmanager
- Grafana

The helper script also recreates the expected PostgreSQL databases through `docker/postgres/create_databases.sql`.

### 3. Initialize local schema and seed data

```bash
dotnet run --project dotnet/BaseFaq.Tools.Seed
```

Common choices:

- `2`: seed essential tenant metadata, AI provider data, and the AI Agent user
- `3`: clean databases and seed essential plus sample FAQ data

On a clean machine, this is the fastest way to create the tenant schema and the seed-target FAQ schema because the seed application runs EF Core migrations before inserting data.

### 4. Use the migration tool when you change FAQ schema

Use the migration tool after tenant metadata already exists:

```bash
dotnet run --project dotnet/BaseFaq.Tools.Migration
```

Or run the FAQ database update non-interactively:

```bash
dotnet run --project dotnet/BaseFaq.Tools.Migration -- --app Faq --command database-update
```

If you want full manual schema control from scratch, first migrate `TenantDbContext`, then use `BaseFaq.Tools.Migration` for tenant FAQ databases.

Manual tenant database migration:

```bash
dotnet ef database update \
  --project dotnet/BaseFaq.Common.EntityFramework.Tenant \
  --startup-project dotnet/BaseFaq.Tenant.BackOffice.Api
```

### 5. Run the host-based services

```bash
dotnet run --project dotnet/BaseFaq.Tenant.BackOffice.Api
dotnet run --project dotnet/BaseFaq.Tenant.Portal.Api
dotnet run --project dotnet/BaseFaq.Faq.Portal.Api
dotnet run --project dotnet/BaseFaq.Faq.Public.Api
dotnet run --project dotnet/BaseFaq.AI.Api
dotnet run --project dotnet/BaseFaq.Tenant.Worker.Api
```

Portal frontend:

```bash
cd apps/portal
npm install --legacy-peer-deps
cp .env.example .env
npm run dev
```

## Full Docker alternative

If you want the app and APIs to run in containers as well:

```bash
./docker.sh
```

Equivalent manual command:

```bash
docker compose -p bf_services -f docker/docker-compose.yml up -d --build
```

Notes:

- the app/API stack expects the external Docker network `bf-network`, which is created by the base-services stack
- the application images use the repository root as the Docker build context
- the default appsettings values use `host.docker.internal`, which keeps host and container networking aligned

## Service endpoints

| Service | URL |
|---|---|
| Portal app | `http://localhost:5500` |
| Tenant BackOffice API | `http://localhost:5000` |
| Tenant Portal API | `http://localhost:5002` |
| FAQ Portal API | `http://localhost:5010` |
| FAQ Public API | `http://localhost:5020` |
| AI API health | `http://localhost:5030/health` |
| Tenant Worker API | no HTTP surface; background host only |
| PostgreSQL | `localhost:5432` |
| Redis | `localhost:6379` |
| RabbitMQ UI | `http://localhost:15672` |
| SMTP4Dev | `http://localhost:4590` |
| Jaeger | `http://localhost:16686` |
| Prometheus | `http://localhost:9090` |
| Alertmanager | `http://localhost:9093` |
| Grafana | `http://localhost:3000` |

Grafana default local credentials are `admin` / `admin`.

SMTP4Dev local delivery settings:

- host: `host.docker.internal`
- port: `1025`
- username: empty
- password: empty
- SSL: `false`

## Hostname strategy

The repository defaults many connection strings to `host.docker.internal` so the same values can work from the host machine and from Docker containers.

### Linux note

Windows and macOS already expose `host.docker.internal`. On Linux, add it to `/etc/hosts` if your environment does not already resolve it:

```bash
echo "127.0.0.1 host.docker.internal" | sudo tee -a /etc/hosts
```

## Local subdomains with the `simulatedev` helper

If you want local hostnames such as `dev.portal.basefaq.com`, use the helper in `local/env/simulatedev`.

Linux:

```bash
chmod +x local/env/simulatedev/setup-subdomains.sh local/env/simulatedev/teardown-subdomains.sh
./local/env/simulatedev/setup-subdomains.sh
```

Windows PowerShell:

```powershell
.\local\env\simulatedev\setup-subdomains.ps1
```

Cleanup:

```bash
./local/env/simulatedev/teardown-subdomains.sh
```

or

```powershell
.\local\env\simulatedev\teardown-subdomains.ps1
```

The helper runs an Nginx reverse proxy in Docker and updates the hosts file with managed entries. Use elevated privileges because hosts-file updates are mandatory.

## Auth0 and local login

Protected APIs and the Portal frontend expect Auth0-based JWT flows.

Local callbacks commonly needed:

- Swagger UI:
  - `http://localhost:5000/swagger/oauth2-redirect.html`
  - `http://localhost:5002/swagger/oauth2-redirect.html`
  - `http://localhost:5010/swagger/oauth2-redirect.html`
- Portal SPA:
  - `http://localhost:5500/login`
  - `http://dev.portal.basefaq.com/login` if the local subdomain helper is active

## Common problems

### `network bf-network declared as external, but could not be found`

Start the base-services stack first.

### Redis complains that `REDIS_PASSWORD` is missing

Use `./docker-base.sh` or export the variable manually before running Compose:

```bash
export REDIS_PASSWORD=RedisTempPassword
```

### Local HTTPS certificate warning

Run:

```bash
dotnet dev-certs https --trust
```

## Shutdown

Stop base services:

```bash
docker compose -p bf_baseservices -f docker/docker-compose.baseservices.yml down
```

Stop app/API containers:

```bash
docker compose -p bf_services -f docker/docker-compose.yml down
```
