# Querify

Querify is a multi-tenant **Question-to-Knowledge** platform. It turns repeated customer,
community, and team questions into reusable knowledge assets that can be used in self-service,
private support, public/community responses, and auditable decision workflows.

The repository is built from a React 19 Portal, multiple `.NET 10` APIs, tenant-aware PostgreSQL
databases, background workers, and local Docker infrastructure.

## Current Snapshot

| Area | Current state |
|---|---|
| Portal frontend | `apps/portal` is the tenant-facing React/Vite app for authenticated workspace flows, settings, billing, profile, and QnA management. |
| Tenant module | BackOffice, Portal, Public API, and Worker surfaces exist for tenant metadata, users, billing, entitlements, client keys, webhooks, and email outbox processing. |
| QnA module | Portal API, Public API, Worker, domain, persistence, source upload verification, public vote/feedback, activity, spaces, questions, answers, sources, and tags are active. |
| Direct module | Contracts and tenant persistence exist for conversations and messages. Portal/Public APIs and workflows are planned. |
| Broadcast module | Contracts and tenant persistence exist for public/community threads and captured items. Portal APIs, workers, grouping, and response workflows are planned. |
| Trust module | `TrustDbContext` exists as the persistence boundary. Review, approval, decision, and audit entities/workflows are planned. |
| AI and MCP | AI is a cross-cutting capability, not a separate product module. QnA supports `AiConfidenceScore`; a TypeScript MCP proxy can call current QnA/Tenant REST tools; the native `.NET` MCP server and Source-to-Q&A generation pipeline are planned. |

## Product Summary

Querify is organized around five module boundaries:

| Module | Ownership |
|---|---|
| `Tenant` | Workspace identity, users, permissions, billing, entitlements, client keys, database connections, and platform operations. |
| `QnA` | Reusable questions, canonical answers, sources, tags, visibility, workflow state, activity, public vote/feedback, and knowledge gaps accepted for curation. |
| `Direct` | Private 1:1 conversations, messages, suggested responses, handoff context, and private gap evidence. |
| `Broadcast` | Public or shared-channel threads, comments, community interactions, grouping, public response coordination, and social signals. |
| `Trust` | Validation, approval, voting, decisions, rationale, contestation, policy, and audit history. |

The Creator MVP package is:

```text
Querify Creator = QnA Answer Hub + Direct Ask Me Inbox + Broadcast Comment Collector + Trust Approval Log
```

QnA is the current implementation foundation. Direct, Broadcast, and Trust are intentionally kept
separate so private conversations, public/community interactions, and governance state do not get
stored inside QnA.

## AI Direction

AI is designed as a shared capability for orchestration, retrieval, generation, classification,
evaluation, and safety. Product state still belongs to the owning module.

Current and planned AI behavior includes:

- human-in-the-loop source-to-Q&A draft generation
- hybrid/semantic retrieval and RAG over QnA sources
- structured outputs validated before they become commands
- prompt versioning and provider-agnostic LLM adapters
- confidence scoring, provenance, model/cost/latency telemetry, and review status
- Direct answer suggestions, conversation summaries, and handoff signals
- Broadcast comment classification, grouping, spam/noise separation, and public reply drafts
- Trust review packages, decision summaries, and contradiction checks before publication
- MCP tools so AI clients can query spaces, create drafts, and operate through module-scoped tools

See [`docs/integrations/mcp-server.md`](docs/integrations/mcp-server.md) for the MCP proxy that can
work with the current REST APIs, and [`docs/future/integrations/mcp.md`](docs/future/integrations/mcp.md)
for the native `.NET` MCP server design.

## Repository Layout

- `apps/portal`: tenant-facing Portal frontend built with React 19, Vite, Tailwind CSS, Auth0,
  TanStack Query/Table, `react-hook-form`, `zod`, `react-intl`, and SignalR client support.
- `dotnet/`: API hosts, worker hosts, feature-scoped business projects, module persistence,
  shared infrastructure, integration tests, seed tooling, and migration tooling.
- `devops/local/`: local Docker Compose stacks, helper scripts, and simulated local subdomain
  tooling.
- `docs/`: canonical repository documentation.

## Backend Surfaces

| Surface | Responsibility | Local port |
|---|---|---:|
| `Querify.Tenant.BackOffice.Api` | global tenant, user, billing, and tenant metadata administration | `5000` |
| `Querify.Tenant.Portal.Api` | authenticated tenant workspace, member, settings, profile, and billing flows | `5002` |
| `Querify.Tenant.Public.Api` | public tenant ingress such as Stripe webhooks | `5004` |
| `Querify.QnA.Portal.Api` | authenticated QnA management and Portal SignalR notifications | `5010` |
| `Querify.QnA.Public.Api` | public QnA access, vote, and feedback APIs | `5020` |
| `Querify.Tenant.Worker.Api` | billing webhook inbox and email outbox processing | n/a |
| `Querify.QnA.Worker.Api` | QnA source upload verification and worker jobs | `5030` |

## Core Stack

Backend:

- `.NET 10`, ASP.NET Core, EF Core, CQRS with MediatR
- PostgreSQL with tenant-aware module databases
- Auth0 JWT authentication
- MassTransit with RabbitMQ
- Hangfire-backed QnA worker storage
- SignalR Portal notifications
- S3-compatible object storage for QnA sources
- Redis, OpenTelemetry, Sentry, Swagger/OpenAPI
- xUnit integration tests and architecture tests

Frontend:

- React 19, TypeScript, Vite, Tailwind CSS v4
- Auth0 SPA authentication
- TanStack Query and TanStack Table
- `react-hook-form`, `zod`, `react-intl`, `lucide-react`, `sonner`
- responsive Portal shell with workspace switching, localization, and realtime notification inbox

## Prerequisites

- Docker Engine with Docker Compose v2
- `.NET SDK 10.0.100` from [`global.json`](global.json)
- Node.js LTS and npm for `apps/portal`
- Auth0 tenant/application for live authenticated flows

## Essential Local Startup

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

This starts PostgreSQL, RabbitMQ, Redis, SMTP4Dev, Jaeger, Prometheus, Alertmanager, and Grafana.

### 3. Initialize databases

```bash
dotnet run --project dotnet/Querify.Tools.Seed
```

Recommended choices:

- `2`: seed essential tenant metadata only
- `3`: clean databases and seed essential plus sample QnA data

### 4. Run APIs and Portal

Typical host-based backend workflow:

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

# or start the full app stack
./devops/local/docker/docker.sh
```

Windows PowerShell equivalents live beside the shell scripts under `devops/local/docker/*.ps1`.

### 5. Apply QnA migrations when schema changes

Interactive mode:

```bash
dotnet run --project dotnet/Querify.Tools.Migration
```

Non-interactive QnA database update:

```bash
dotnet run --project dotnet/Querify.Tools.Migration -- --command database-update
```

Run this after tenant metadata exists, because module databases are resolved through Tenant.

## Local Endpoints

| Surface | URL |
|---|---|
| Portal app | `http://localhost:5500` |
| Tenant BackOffice API | `http://localhost:5000` |
| Tenant Portal API | `http://localhost:5002` |
| Tenant Public API | `http://localhost:5004` |
| QnA Portal API | `http://localhost:5010` |
| QnA Public API | `http://localhost:5020` |
| QnA Worker | `http://localhost:5030` |
| PostgreSQL | `localhost:5432` |
| RabbitMQ UI | `http://localhost:15672` |
| Jaeger | `http://localhost:16686` |
| Prometheus | `http://localhost:9090` |
| Grafana | `http://localhost:3000` |

## Auth And Request Context

- Protected APIs use Auth0-issued JWTs.
- Authenticated tenant-scoped requests use `X-Tenant-Id`.
- Public QnA requests use `X-Client-Key`.
- Public billing webhooks use `Querify.Tenant.Public.Api` and do not require JWT, `X-Tenant-Id`, or
  `X-Client-Key`.
- Swagger UI auth is configured in the protected API `appsettings.json` files.
- Portal realtime notifications connect to the QnA Portal SignalR hub and join authorized
  tenant/module groups.

## Documentation

- Unified docs index: [`docs/README.md`](docs/README.md)
- Work routing: [`docs/execution-guide.md`](docs/execution-guide.md)
- Product/module ownership: [`docs/business/value_proposition/value_proposition.md`](docs/business/value_proposition/value_proposition.md)
- Creator MVP plan: [`docs/business/mvp/creator_mvp_plan.md`](docs/business/mvp/creator_mvp_plan.md)
- Creator MVP technical architecture: [`docs/business/mvp/creator_mvp_technical_architecture.md`](docs/business/mvp/creator_mvp_technical_architecture.md)
- Backend architecture: [`docs/backend/architecture/dotnet-backend-overview.md`](docs/backend/architecture/dotnet-backend-overview.md)
- Backend local runtime: [`docs/backend/tools/local-development.md`](docs/backend/tools/local-development.md)
- Backend testing: [`docs/backend/testing/integration-testing-strategy.md`](docs/backend/testing/integration-testing-strategy.md)
- Frontend architecture: [`docs/frontend/architecture/portal-app.md`](docs/frontend/architecture/portal-app.md)
- Frontend runtime: [`docs/frontend/tools/portal-runtime.md`](docs/frontend/tools/portal-runtime.md)
- Frontend validation: [`docs/frontend/testing/validation-guide.md`](docs/frontend/testing/validation-guide.md)
- MCP current proxy: [`docs/integrations/mcp-server.md`](docs/integrations/mcp-server.md)

## Troubleshooting

- `qf-network declared as external, but could not be found`: start the base services first.
- `set REDIS_PASSWORD`: use `./devops/local/docker/base.sh` or export
  `REDIS_PASSWORD=RedisTempPassword` before manual compose runs.
- HTTPS trust issues: run `dotnet dev-certs https --trust`.
- Linux `host.docker.internal` resolution issues: see
  [`docs/backend/tools/local-development.md`](docs/backend/tools/local-development.md).
