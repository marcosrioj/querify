# Querify

Querify is a multi-tenant knowledge platform built from a React portal, multiple `.NET 10` APIs, tenant-aware PostgreSQL databases, and background worker services.

This `README` is intentionally short. It explains what the repository is and how to bootstrap the essential local stack. All repository-owned documentation lives under [`docs/README.md`](docs/README.md).

## Repository layout

- `apps/portal`: customer-facing portal frontend built with React, Vite, Tailwind, and Auth0.
- `dotnet/`: API hosts, business modules, shared infrastructure libraries, persistence projects, integration tests, and console tools.
- `devops/local/`: local Docker Compose stacks, helper scripts, and the `simulatedev` reverse proxy tooling.
- `docs/`: the project knowledge base.

## Prerequisites

- Docker Engine with Docker Compose v2
- `.NET SDK 10.0.100` from [`global.json`](global.json)
- Node.js LTS and npm for `apps/portal`
- Auth0 tenant/application if you need live authenticated flows

## Essential local startup

### 1. Restore and build

```bash
dotnet restore Querify.sln
dotnet build Querify.sln
```

### 2. Start local infrastructure

macOS/Linux:

```bash
./devops/local/docker/base.sh
```

Windows PowerShell:

```powershell
.\devops\local\docker\base.ps1
```

This brings up PostgreSQL, RabbitMQ, Redis, SMTP4Dev, Jaeger, Prometheus, Alertmanager, and Grafana.

### 3. Initialize the databases

```bash
dotnet run --project dotnet/Querify.Tools.Seed
```

Recommended choices:

- `2`: essential data only, when you just need the base tenant metadata
- `3`: clean databases and seed essential plus sample QnA data

### 4. Run the services you need

Typical local host-based workflow:

```bash
dotnet run --project dotnet/Querify.Tenant.BackOffice.Api
dotnet run --project dotnet/Querify.Tenant.Portal.Api
dotnet run --project dotnet/Querify.Tenant.Public.Api
dotnet run --project dotnet/Querify.QnA.Portal.Api
dotnet run --project dotnet/Querify.QnA.Public.Api
```

Frontend:

```bash
cd apps/portal
npm install --legacy-peer-deps
cp .env.example .env
npm run dev
```

Containerized app/API alternatives:

```bash
./devops/local/docker/backend.sh
./devops/local/docker/frontend.sh

# or start the full container stack in one command
./devops/local/docker/docker.sh
```

`./devops/local/docker/docker.sh` now composes the stack only from `devops/local/docker/docker-compose.backend.yml` and `devops/local/docker/docker-compose.frontend.yml`. Windows PowerShell equivalents live beside these scripts under `devops/local/docker/*.ps1`.

### 5. Use the migration tool when QnA schema changes

Interactive mode:

```bash
dotnet run --project dotnet/Querify.Tools.Migration
```

Non-interactive QnA database update:

```bash
dotnet run --project dotnet/Querify.Tools.Migration -- --command database-update
```

Use this after the tenant metadata already exists, or when you need to apply QnA migrations across all tenant databases.

## Local endpoints

| Surface | URL |
|---|---|
| Portal app | `http://localhost:5500` |
| Tenant BackOffice API | `http://localhost:5000` |
| Tenant Portal API | `http://localhost:5002` |
| Tenant Public API | `http://localhost:5004` |
| QnA Portal API | `http://localhost:5010` |
| QnA Public API | `http://localhost:5020` |
| PostgreSQL | `localhost:5432` |
| RabbitMQ UI | `http://localhost:15672` |
| Jaeger | `http://localhost:16686` |
| Prometheus | `http://localhost:9090` |
| Grafana | `http://localhost:3000` |

## Auth and request context

- Protected APIs use Auth0-issued JWTs.
- Tenant-scoped endpoints use `X-Tenant-Id`.
- Public QnA requests use `X-Client-Key`.
- Public billing webhooks use `Querify.Tenant.Public.Api` and do not require JWT, `X-Tenant-Id`, or `X-Client-Key`.
- Swagger UI auth is configured in the protected API `appsettings.json` files.

## Documentation

- Unified docs index: [`docs/README.md`](docs/README.md)
- Repository execution guide: [`docs/execution-guide.md`](docs/execution-guide.md)
- Backend architecture: [`docs/backend/architecture/dotnet-backend-overview.md`](docs/backend/architecture/dotnet-backend-overview.md)
- Backend runtime and tools: [`docs/backend/tools/local-development.md`](docs/backend/tools/local-development.md)
- Backend testing: [`docs/backend/testing/integration-testing-strategy.md`](docs/backend/testing/integration-testing-strategy.md)
- Frontend architecture: [`docs/frontend/architecture/portal-app.md`](docs/frontend/architecture/portal-app.md)
- Frontend runtime and tools: [`docs/frontend/tools/portal-runtime.md`](docs/frontend/tools/portal-runtime.md)
- Frontend testing: [`docs/frontend/testing/validation-guide.md`](docs/frontend/testing/validation-guide.md)

## Troubleshooting

- `qf-network declared as external, but could not be found`: start the base services first.
- `set REDIS_PASSWORD`: use `./devops/local/docker/base.sh` or export `REDIS_PASSWORD=RedisTempPassword` before manual compose runs.
- HTTPS trust issues: run `dotnet dev-certs https --trust`.
- Linux `host.docker.internal` resolution issues: see [`docs/backend/tools/local-development.md`](docs/backend/tools/local-development.md).
