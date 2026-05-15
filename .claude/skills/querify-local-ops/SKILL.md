---
name: querify-local-ops
description: "Querify local operations and validation commands. Use for Docker base services, API/worker startup, Portal runtime, seed, migration, local subdomains, and test commands."
when_to_use: "Use when the user asks to run or verify the local stack, seed or migrate databases, start APIs/workers/Portal, debug local endpoints, or choose validation commands."
paths:
  - "devops/**"
  - "docs/backend/tools/**"
  - "docs/frontend/tools/**"
  - "docs/backend/testing/**"
  - "docs/frontend/testing/**"
---

# Querify Local Ops

Read:

1. `docs/backend/tools/local-development.md`
2. `docs/backend/tools/seed-tool.md`
3. `docs/backend/tools/migration-tool.md`
4. `docs/frontend/tools/portal-runtime.md`
5. `docs/frontend/testing/validation-guide.md`

## Standard local model

1. Start infrastructure with Docker.
2. Run .NET APIs and workers on the host.
3. Run Portal on the host when browser flows are needed.

## Common commands

Restore and build:

```bash
dotnet restore Querify.sln
dotnet build Querify.sln --no-restore
```

Start base services:

```bash
./devops/local/docker/base.sh
```

Seed:

```bash
dotnet run --project dotnet/Querify.Tools.Seed
```

Seed choices:

- `2`: essential tenant metadata only.
- `3`: clean databases and seed essential plus sample QnA data.

Migration tool:

```bash
dotnet run --project dotnet/Querify.Tools.Migration
dotnet run --project dotnet/Querify.Tools.Migration -- --module QnA --command database-update
```

Do not run or generate EF migrations unless the user explicitly asks for migration work.

Host APIs and workers:

```bash
dotnet run --project dotnet/Querify.Tenant.BackOffice.Api
dotnet run --project dotnet/Querify.Tenant.Portal.Api
dotnet run --project dotnet/Querify.Tenant.Public.Api
dotnet run --project dotnet/Querify.QnA.Portal.Api
dotnet run --project dotnet/Querify.QnA.Public.Api
dotnet run --project dotnet/Querify.Tenant.Worker.Api
dotnet run --project dotnet/Querify.QnA.Worker.Api
```

Portal:

```bash
cd apps/portal
npm install --legacy-peer-deps
cp .env.example .env
npm run dev
npm run lint
npm run build
```

## Endpoints

- Portal: `http://localhost:5500`
- Tenant BackOffice API: `http://localhost:5000`
- Tenant Portal API: `http://localhost:5002`
- Tenant Public API: `http://localhost:5004`
- QnA Portal API: `http://localhost:5010`
- QnA Public API: `http://localhost:5020`
- QnA Worker Hangfire: `http://localhost:5030/HangfireDashboard`
- RabbitMQ UI: `http://localhost:15672`
- MinIO API: `http://localhost:5900`
- MinIO Console: `http://localhost:5901`
- SMTP4Dev: `http://localhost:4590`

## Validation

- Backend: run targeted `dotnet build` or `dotnet test` for touched projects first.
- Architecture rules: `dotnet test dotnet/Querify.Common.Architecture.Test.IntegrationTest/Querify.Common.Architecture.Test.IntegrationTest.csproj`
- Frontend: `npm run lint` and `npm run build` from `apps/portal`.
- Manual Portal regression is required for UI changes because there is no dedicated frontend automated test suite yet.
