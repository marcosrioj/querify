# BaseFAQ

BaseFAQ is a multi-tenant FAQ platform built from a React portal, multiple `.NET 10` APIs, tenant-aware PostgreSQL databases, and an AI runtime for FAQ generation and similarity matching.

This `README` is intentionally short. It explains what the repository is and how to bootstrap the essential local stack. Deep technical guidance lives in [`docs/README.md`](docs/README.md).

## Repository layout

- `apps/portal`: customer-facing portal frontend built with React, Vite, Tailwind, and Auth0.
- `dotnet/`: API hosts, business modules, shared infrastructure libraries, persistence projects, integration tests, and console tools.
- `docker/`: local Docker Compose stacks for base services and containerized app/API execution.
- `local/`: local-only helpers such as the `simulatedev` reverse proxy and subdomain setup scripts.
- `azure/`: stage-based Azure provisioning and deployment scripts.
- `docs/`: the project knowledge base.

## Prerequisites

- Docker Engine with Docker Compose v2
- `.NET SDK 10.0.100` from [`global.json`](global.json)
- Node.js LTS and npm for `apps/portal`
- Auth0 tenant/application if you need live authenticated flows

## Essential local startup

### 1. Restore and build

```bash
dotnet restore BaseFaq.sln
dotnet build BaseFaq.sln
```

### 2. Start local infrastructure

macOS/Linux:

```bash
./docker-base.sh
```

Windows PowerShell:

```powershell
.\docker-base.ps1
```

This brings up PostgreSQL, RabbitMQ, Redis, SMTP4Dev, Jaeger, Prometheus, Alertmanager, and Grafana.

### 3. Initialize the databases

```bash
dotnet run --project dotnet/BaseFaq.Tools.Seed
```

Recommended choices:

- `2`: essential data only, when you just need AI providers and the AI Agent user
- `3`: clean databases and seed essential plus sample FAQ data

If the seed prints a new AI Agent user id, copy it to `Ai:UserId` in `dotnet/BaseFaq.AI.Api/appsettings.json`.

### 4. Run the services you need

Typical local host-based workflow:

```bash
dotnet run --project dotnet/BaseFaq.Tenant.BackOffice.Api
dotnet run --project dotnet/BaseFaq.Tenant.Portal.Api
dotnet run --project dotnet/BaseFaq.Faq.Portal.Api
dotnet run --project dotnet/BaseFaq.Faq.Public.Api
dotnet run --project dotnet/BaseFaq.AI.Api
```

Frontend:

```bash
cd apps/portal
npm install --legacy-peer-deps
cp .env.example .env
npm run dev
```

Containerized app/API alternative:

```bash
./docker.sh
```

### 5. Use the migration tool when FAQ schema changes

Interactive mode:

```bash
dotnet run --project dotnet/BaseFaq.Tools.Migration
```

Non-interactive FAQ database update:

```bash
dotnet run --project dotnet/BaseFaq.Tools.Migration -- --app Faq --command database-update
```

Use this after the tenant metadata already exists, or when you need to apply FAQ migrations across all tenant databases.

## Local endpoints

| Surface | URL |
|---|---|
| Portal app | `http://localhost:5500` |
| Tenant BackOffice API | `http://localhost:5000` |
| Tenant Portal API | `http://localhost:5002` |
| FAQ Portal API | `http://localhost:5010` |
| FAQ Public API | `http://localhost:5020` |
| AI API health | `http://localhost:5030/health` |
| PostgreSQL | `localhost:5432` |
| RabbitMQ UI | `http://localhost:15672` |
| Jaeger | `http://localhost:16686` |
| Prometheus | `http://localhost:9090` |
| Grafana | `http://localhost:3000` |

## Auth and request context

- Protected APIs use Auth0-issued JWTs.
- Tenant-scoped endpoints use `X-Tenant-Id`.
- Public FAQ requests use `X-Client-Key`.
- Swagger UI auth is configured in the protected API `appsettings.json` files.

## Documentation

- Unified docs index: [`docs/README.md`](docs/README.md)
- Architecture overview: [`docs/architecture/solution-architecture.md`](docs/architecture/solution-architecture.md)
- Backend guide: [`docs/backend/dotnet-backend-overview.md`](docs/backend/dotnet-backend-overview.md)
- Frontend guide: [`docs/frontend/portal-app.md`](docs/frontend/portal-app.md)
- Local dev and Docker: [`docs/devops/local-development.md`](docs/devops/local-development.md)
- Azure delivery: [`docs/devops/azure-delivery.md`](docs/devops/azure-delivery.md)
- Tooling: [`docs/tools/migration-tool.md`](docs/tools/migration-tool.md), [`docs/tools/seed-tool.md`](docs/tools/seed-tool.md)
- Testing strategy: [`docs/testing/integration-testing-strategy.md`](docs/testing/integration-testing-strategy.md)

## Troubleshooting

- `bf-network declared as external, but could not be found`: start the base services first.
- `set REDIS_PASSWORD`: use `./docker-base.sh` or export `REDIS_PASSWORD=RedisTempPassword` before manual compose runs.
- HTTPS trust issues: run `dotnet dev-certs https --trust`.
- Linux `host.docker.internal` resolution issues: see [`docs/devops/local-development.md`](docs/devops/local-development.md).
