---
name: querify-backend
description: "Querify .NET backend implementation and review rules. Use for APIs, CQRS handlers, services, EF Core persistence, tenancy, workers, migrations, seed, and backend tests."
when_to_use: "Use when editing dotnet/**, backend docs, integration tests, worker behavior, module contracts, persistence, API errors, or tenant-scoped data flows."
paths:
  - "dotnet/**"
  - "docs/backend/**"
  - "docs/behavior-change-playbook.md"
---

# Querify Backend

Read the owning docs before changing code:

1. `docs/backend/architecture/dotnet-backend-overview.md`
2. `docs/backend/architecture/solution-cqrs-write-rules.md`
3. `docs/backend/architecture/repository-rules.md`
4. `docs/backend/testing/integration-testing-strategy.md`
5. Add `docs/backend/architecture/qna-domain-boundary.md` for QnA entities/rules.
6. Add `docs/backend/architecture/querify-tenant-worker.md` for Tenant Worker or QnA worker boundary work.

## Module boundaries

- Tenant owns identity, users, membership, billing, entitlements, client keys, connection routing, and control-plane jobs.
- QnA owns spaces, questions, answers, sources, tags, visibility, workflow state, QnA activity, and public QnA signals.
- Direct owns private conversations, messages, handoff, 1:1 resolution, and private gap evidence.
- Broadcast owns public/community threads, captured items, grouping, public response coordination, and social/community signals.
- Trust owns validation, approval, voting, decisions, rationale, contestation, policy, and audit history.
- Do not park Direct, Broadcast, or Trust workflow state in QnA because a QnA enum sounds close.

## CQRS and APIs

- Standard flow: `Controller -> Service -> Command/Query`.
- Async adapters: `Consumer -> ConsumerService -> Command/Query`, `HostedService -> ProcessorService -> Command/Query`, `BackgroundService (Hangfire) -> Service -> Command/Query`, `Event -> NotificationService -> Command/Query`.
- Controllers map HTTP only. Services coordinate, open feature telemetry when appropriate, and dispatch MediatR.
- Command handlers return only `Guid`, `bool`, `string`, or `void`. The documented QnA Source `upload-intent` credential is the current exception.
- Do not do read-after-write just to shape a richer write response.
- `POST` create returns `201` plus `Guid`; `PUT`/`PATCH` returns `200` plus `Guid` or meaningful `bool`; async commands return `202` plus correlation `Guid`.
- Route action segments use lowercase kebab-case.

## Query performance

- Treat GET and query handlers as hot paths.
- Use `AsNoTracking()` by default.
- Project directly to DTOs before materialization.
- Avoid `Include(...)` unless a measured and documented exception proves it is cheaper.
- Apply filters and sorting before `CountAsync`, `Skip`, and `Take`.
- Page parent rows before loading expensive children.
- Add indexes and migrations when adding query filters, sorts, or high-cardinality lookups. Do not run migrations unless explicitly requested.

## Persistence and contracts

- Product persistence entities stay anemic: state only, no factories, transition methods, or computed projection helpers.
- Module `DbContext` classes live under `DbContext/<Module>DbContext.cs`.
- Save-time rules live under focused `DbContext/<Concern>` folders.
- Tenant-owned relationships are enforced in the owning module `DbContext/TenantIntegrity`, not repeated in handlers.
- Use `TenantIntegrityGuard` plus lookup caches that read referenced tenant ids with `IgnoreQueryFilters()`.
- Backend timestamps are UTC-only. Use `DateTime.UtcNow` or provider UTC values; avoid local time APIs.
- New persisted or DTO timestamp properties use a `Utc` suffix unless inherited audit fields already own the state.
- Module DTOs stay feature-scoped. Do not add catch-all `*Dtos.cs` files.
- Write-side request DTOs are flat and explicit.

## API errors

- Use `ApiErrorException` for request-time validation, missing resources, domain workflow rejection, conflicts, and user-correctable errors.
- Reserve `InvalidOperationException` for internal invariants, configuration bugs, or persistence corruption.
- Treat `ApiErrorException` messages as frontend localization inputs. Keep them stable or add a Portal mapping when they include dynamic ids or operational details.

## Validation

- Prefer targeted builds/tests for touched projects first.
- Use real PostgreSQL and real migrations for integration behavior when that is the established test boundary.
- Run `dotnet test dotnet/Querify.Common.Architecture.Test.IntegrationTest/Querify.Common.Architecture.Test.IntegrationTest.csproj` when architecture rules might be affected.
- Tell the user when migrations, Docker services, or external dependencies were not run.
