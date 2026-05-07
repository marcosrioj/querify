# Local Development and Docker

## Purpose

This guide is the operational runbook for running the Querify backend locally. It combines the repository bootstrap flow, Docker infrastructure, optional full-container execution, and the backend-facing parts of local host and proxy setup.

## Recommended local model

The repository is easiest to run in this mode:

1. Start infrastructure with Docker.
2. Run the `.NET` APIs on the host.
3. Run the worker on the host.
4. Run the Portal frontend on the host only when you need browser flows.

This gives fast iteration without losing the production-like infrastructure components.

## Prerequisites

- Docker Engine and Docker Compose v2
- `.NET SDK 10.0.100`
- `dotnet dev-certs https --trust` if you want local HTTPS without certificate issues
- Node.js LTS and npm only if you also need the Portal frontend

## Step-by-step local bootstrap

### 1. Restore and build

```bash
dotnet restore Querify.sln
dotnet build Querify.sln --no-restore
```

### 2. Start base services

macOS/Linux:

```bash
./devops/local/docker/base.sh
```

Windows PowerShell:

```powershell
.\devops\local\docker\base.ps1
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

The helper script also recreates the expected PostgreSQL databases through `devops/local/docker/postgres/create_databases.sql`.

### 3. Initialize local schema and seed data

```bash
dotnet run --project dotnet/Querify.Tools.Seed
```

Common choices:

- `2`: seed essential tenant metadata only
- `3`: clean databases and seed essential plus sample QnA data

On a clean machine, this is the fastest way to create the tenant schema and the seed-target module schema because the seed application runs EF Core migrations before inserting data.

### 4. Use the migration tool when you change supported module schema

Use the migration tool after tenant metadata already exists:

```bash
dotnet run --project dotnet/Querify.Tools.Migration
```

Or run the QnA module database update non-interactively:

```bash
dotnet run --project dotnet/Querify.Tools.Migration -- --module QnA --command database-update
```

If you want full manual schema control from scratch, first migrate `TenantDbContext`, then use `Querify.Tools.Migration` for supported tenant module databases.

Manual tenant database migration:

```bash
dotnet ef database update \
  --project dotnet/Querify.Common.EntityFramework.Tenant \
  --startup-project dotnet/Querify.Tenant.BackOffice.Api
```

### 5. Run the host-based services

```bash
dotnet run --project dotnet/Querify.Tenant.BackOffice.Api
dotnet run --project dotnet/Querify.Tenant.Portal.Api
dotnet run --project dotnet/Querify.Tenant.Public.Api
dotnet run --project dotnet/Querify.QnA.Portal.Api
dotnet run --project dotnet/Querify.QnA.Public.Api
dotnet run --project dotnet/Querify.Tenant.Worker.Api
```

For the Portal frontend runtime and Auth0 setup, use [`../../frontend/tools/portal-runtime.md`](../../frontend/tools/portal-runtime.md).

## Docker runtime alternatives

If you want the backend APIs and worker in containers:

```bash
./devops/local/docker/backend.sh
```

If you want the Portal frontend container only:

```bash
./devops/local/docker/frontend.sh
```

If you want the entire container stack with the previous single-command behavior:

```bash
./devops/local/docker/docker.sh
```

PowerShell equivalents live beside these scripts under `devops/local/docker/*.ps1`.

Equivalent manual commands:

```bash
docker compose -p qf_services -f devops/local/docker/docker-compose.backend.yml up -d --build
docker compose -p qf_services -f devops/local/docker/docker-compose.frontend.yml up -d --build
docker compose -p qf_services \
  -f devops/local/docker/docker-compose.backend.yml \
  -f devops/local/docker/docker-compose.frontend.yml \
  up -d --build
```

Notes:

- the app/API stack expects the external Docker network `qf-network`, which is created by the base-services stack
- the application images use the repository root as the Docker build context
- the default appsettings values use `host.docker.internal`, which keeps host and container networking aligned
- `devops/local/docker/docker-compose.backend.yml` boots the APIs plus `Querify.Tenant.Worker.Api`
- `devops/local/docker/docker-compose.frontend.yml` boots only `querify.portal.app`
- `./devops/local/docker/docker.sh` combines only `devops/local/docker/docker-compose.backend.yml` and `devops/local/docker/docker-compose.frontend.yml`

## Service endpoints

| Service | URL |
|---|---|
| Tenant BackOffice API | `http://localhost:5000` |
| Tenant Portal API | `http://localhost:5002` |
| Tenant Public API | `http://localhost:5004` |
| QnA Portal API | `http://localhost:5010` |
| QnA Public API | `http://localhost:5020` |
| Tenant Worker API | no HTTP surface; background host only |
| PostgreSQL | `localhost:5432` |
| Redis | `localhost:6379` |
| RabbitMQ UI | `http://localhost:15672` |
| SMTP4Dev | `http://localhost:4590` |
| Jaeger | `http://localhost:16686` |
| Prometheus | `http://localhost:9090` |
| Alertmanager | `http://localhost:9093` |
| Grafana | `http://localhost:3000` |

Grafana default local credentials are `admin` and `admin`.

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

## Local subdomains

If you want local hostnames such as `dev.portal.querify.net`, `dev.qna.portal.querify.net`, and `dev.qna.public.querify.net`, use the helper documented in [`../../frontend/tools/local-subdomains.md`](../../frontend/tools/local-subdomains.md).

The helper runs an Nginx reverse proxy in Docker and updates the hosts file with managed entries. Use elevated privileges because hosts-file updates are mandatory.

## Auth0 and local login

Protected APIs and the Portal frontend expect Auth0-based JWT flows.

Local Swagger UI callbacks commonly needed:

- `http://localhost:5000/swagger/oauth2-redirect.html`
- `http://localhost:5002/swagger/oauth2-redirect.html`
- `http://localhost:5010/swagger/oauth2-redirect.html`

For the Portal SPA callbacks and logout URLs, see [`../../frontend/tools/portal-runtime.md`](../../frontend/tools/portal-runtime.md).

## Common problems

### `network qf-network declared as external, but could not be found`

Start the base-services stack first.

### Redis complains that `REDIS_PASSWORD` is missing

Use `./devops/local/docker/base.sh` or export the variable manually before running Compose:

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
docker compose -p qf_baseservices -f devops/local/docker/docker-compose.baseservices.yml down
```

Stop app and API containers:

```bash
docker compose -p qf_services \
  -f devops/local/docker/docker-compose.backend.yml \
  -f devops/local/docker/docker-compose.frontend.yml \
  down
```

That same `down` command also stops containers started through `./devops/local/docker/backend.sh` and `./devops/local/docker/frontend.sh`.
