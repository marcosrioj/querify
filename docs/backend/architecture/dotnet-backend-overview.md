# Querify .NET Backend Overview

## Purpose

This guide explains how the backend is organized under `dotnet/`, which APIs exist, how multitenancy works, and how to reason about the common development flow.

## Service catalog

| API | Responsibility | Auth | Tenant context | Local port |
|---|---|---|---|---:|
| `Querify.Tenant.BackOffice.Api` | global administration of tenants, tenant users, billing, and tenant metadata | Auth0 JWT | none by default | `5000` |
| `Querify.Tenant.Portal.Api` | tenant workspace settings and tenant-member operations | Auth0 JWT | `X-Tenant-Id` for tenant-scoped operations | `5002` |
| `Querify.Tenant.Public.Api` | public tenant ingress endpoints such as Stripe webhooks | public surface | none | `5004` |
| `Querify.QnA.Portal.Api` | authenticated QnA management for spaces, questions, answers, tags, sources, workflow, activity, and Portal SignalR notifications | Auth0 JWT | `X-Tenant-Id` for HTTP APIs; SignalR validates `tenantId` on connect | `5010` |
| `Querify.QnA.Public.Api` | public QnA access plus vote and feedback signaling over questions and answers | public surface | `X-Client-Key` | `5020` |

| Worker | Responsibility | Data boundary | Local port |
|---|---|---|---:|
| `Querify.Tenant.Worker.Api` | control-plane background processing for billing webhooks and email outbox | `TenantDbContext` only | n/a |
| `Querify.QnA.Worker.Api` | QnA-owned background processing for RabbitMQ source upload verification and operational jobs | `TenantDbContext` + tenant-scoped `QnADbContext` + `HangfireQnaDbContext` | `5030` |

## Project taxonomy inside `dotnet/`

`Querify.sln` includes the active `.NET` projects used by the local backend. The inventory below reflects the projects that are actually in the solution, not every folder that exists under `dotnet/`.

### API hosts

These projects contain ASP.NET Core startup, middleware, and DI registration:

- `Querify.QnA.Portal.Api`
- `Querify.QnA.Public.Api`
- `Querify.Tenant.BackOffice.Api`
- `Querify.Tenant.Portal.Api`
- `Querify.Tenant.Public.Api`

### Worker hosts

- `Querify.Tenant.Worker.Api`
- `Querify.QnA.Worker.Api`

### Business modules

Each service area is split into feature projects.

The current Querify modules are Tenant, QnA, Direct, Broadcast, and Trust. Tenant owns the control plane. QnA, Direct, Broadcast, and Trust own product behavior. Each module uses feature-scoped projects for API behavior and module-owned persistence for data.

Current API/business implementation in this solution:

- QnA Portal:
  - `Querify.QnA.Portal.Business.Space`
  - `Querify.QnA.Portal.Business.Question`
  - `Querify.QnA.Portal.Business.Answer`
  - `Querify.QnA.Portal.Business.Tag`
  - `Querify.QnA.Portal.Business.Source`
  - `Querify.QnA.Portal.Business.Activity`
- QnA Public:
  - `Querify.QnA.Public.Business.Space`
  - `Querify.QnA.Public.Business.Question`
  - `Querify.QnA.Public.Business.Vote`
  - `Querify.QnA.Public.Business.Feedback`
- Tenant BackOffice:
  - `Querify.Tenant.BackOffice.Business.Tenant`
  - `Querify.Tenant.BackOffice.Business.User`
  - `Querify.Tenant.BackOffice.Business.Billing`
- Tenant Portal:
  - `Querify.Tenant.Portal.Business.Tenant`
  - `Querify.Tenant.Portal.Business.User`
- Tenant Public:
  - `Querify.Tenant.Public.Business.Billing`
- Tenant Worker:
  - `Querify.Tenant.Worker.Business.Billing`
  - `Querify.Tenant.Worker.Business.Email`

Current module persistence implementation:

- QnA:
  - `Querify.QnA.Common.Domain`
  - `Querify.QnA.Common.Persistence.QnADb`
  - `Querify.QnA.Common.Persistence.HangfireQnaDb`
- Direct:
  - `Querify.Direct.Common.Persistence.DirectDb`
- Broadcast:
  - `Querify.Broadcast.Common.Persistence.BroadcastDb`
- Trust:
  - no active persistence project in this repository snapshot

### Shared infrastructure and persistence

- `Querify.Common.EntityFramework.Core`: base EF Core context, shared model loading, connection resolution, and database infrastructure used across the solution
- `Querify.Common.EntityFramework.Core.Audit`: auditable entity state, audit model configuration, and audit write rules
- `Querify.Common.EntityFramework.Core.AutoHistory`: auto history model configuration and history capture helpers
- `Querify.Common.EntityFramework.Core.SoftDelete`: soft-delete abstractions, model filters, indexes, and write rules
- `Querify.Common.EntityFramework.Core.Tenant`: tenant-scoped entity abstractions, tenant filters, tenant indexes, and module `DbContext` tenant-integrity helpers
- `Querify.Common.EntityFramework.Tenant`: tenant database context, tenant resolution helpers, and shared tenant infrastructure for the control-plane database
- `Querify.QnA.Common.Domain`: QnA domain entities and reusable entity-related business rules shared by QnA persistence, business features, seed data, and tests
- `Querify.QnA.Common.Persistence.QnADb`: QnA module database context and persistence
- `Querify.QnA.Common.Persistence.HangfireQnaDb`: QnA worker Hangfire storage registration, design-time context, and migrations boundary
- `Querify.Direct.Common.Persistence.DirectDb`: Direct module tenant persistence for conversations and conversation messages
- `Querify.Broadcast.Common.Persistence.BroadcastDb`: Broadcast module tenant persistence for public/community threads and captured items
- `Querify.Common.Infrastructure.Core`: shared core abstractions and backend helper services
- `Querify.Common.Infrastructure.ApiErrorHandling`: API error handling conventions and
  `ApiErrorException`, the exception type handlers should use for request-time API failures
- `Querify.Common.Infrastructure.Hangfire`: persisted Hangfire registration for durable internal
  background jobs; Hangfire background job classes call services and do not own feature behavior
- `Querify.Common.Infrastructure.MassTransit`: MassTransit registration and messaging conventions,
  including RabbitMQ consumers for event-driven source upload verification and Portal notifications
- `Querify.Common.Infrastructure.MediatR`: MediatR integration and related pipeline behavior
- `Querify.Common.Infrastructure.Mvc`: MVC filters and ASP.NET Core glue
- `Querify.Common.Infrastructure.Sentry`: Sentry integration
- `Querify.Common.Infrastructure.Signalr`: shared SignalR infrastructure. Portal-specific base
  contracts, options, extensions, hubs, notification envelopes, groups, and SignalR publishers live
  under its `Portal/` folder. Product-specific events and notification commands stay in the owning
  business feature project.
- `Querify.Common.Infrastructure.Swagger`: Swagger/OpenAPI wiring
- `Querify.Common.Infrastructure.Telemetry`: shared telemetry wiring (OpenTelemetry tracing, OTLP export).
  API and worker hosts register it at composition root, while feature spans are started in
  services by default:
  `Controller -> Service -> Command/Query`,
  `Consumer -> ConsumerService -> Command/Query`,
  `HostedService -> ProcessorService -> Command/Query`,
  `BackgroundService (Hangfire) -> Service -> Command/Query`,
  and `Event -> NotificationService -> Command/Query`.
  Worker hosted services call `ProcessorService` classes; broker consumers call
  `ConsumerService` classes; processor and consumer services coordinate
  telemetry plus MediatR dispatch only, while commands/queries own workflow behavior.
- `Querify.Models.Common`: shared primitive DTOs and common contracts, including `ModuleEnum`
- `Querify.Models.QnA`: QnA-facing contracts
- `Querify.Models.Direct`: Direct-facing contracts
- `Querify.Models.Broadcast`: Broadcast-facing contracts
- `Querify.Models.Tenant`: tenant-facing contracts
- `Querify.Models.User`: user and profile contracts

### Tests and tools

- `Querify.QnA.Portal.Test.IntegrationTests`
- `Querify.QnA.Public.Test.IntegrationTests`
- `Querify.Tenant.BackOffice.Test.IntegrationTests`
- `Querify.Tenant.Portal.Test.IntegrationTests`
- `Querify.Tenant.Public.Test.IntegrationTests`
- `Querify.Tenant.Worker.Test.IntegrationTests`
- `Querify.Common.Architecture.Test.IntegrationTest`
- `Querify.Tools.Migration`
- `Querify.Tools.Seed`

## Standard request flow

The backend follows a consistent pattern across the business modules:

1. The API host configures auth, middleware, and `AddFeatures(...)`.
2. A controller delegates to a service.
3. The service sends a MediatR command or query.
4. The handler performs validation, persistence, and optional event publication.

Implications:

- controllers should remain thin
- write flows should return simple values
- query DTOs belong to read handlers, not command handlers
- read handlers are hot paths and must project DTOs with `AsNoTracking()` instead of loading entity graphs
- API hosts compose multiple feature-owned business modules rather than one catch-all business assembly
- module business projects use the same one-business-project-per-feature pattern across the owning module
- feature projects keep real source files inside the owning project directory and avoid linked compile items
- integration tests should use feature folders and names such as `Tests/Answer/AnswerCommandQueryTests.cs`
- HTTP route segments should use lowercase kebab-case for action-style paths such as `add-tenant-member` or `refresh-allowed-tenant-cache`

The write-side rules are formalized in [`solution-cqrs-write-rules.md`](solution-cqrs-write-rules.md).

## Persistence model

### Tenant database

`TenantDbContext` stores:

- tenants
- users
- tenant memberships
- tenant-to-module database connection strings
- client keys
- control-plane background-processing state such as billing webhook inbox records and email outbox records
- normalized billing state such as billing customers, subscriptions, invoices, payments, and entitlement snapshots

This is the global control plane for the platform.

That also means these responsibilities belong in `TenantDbContext` and not in tenant module persistence:

- billing webhook inboxes
- email outbox
- tenant entitlements
- platform recurring jobs

### QnA database

The QnA module database stores tenant module data for the QnA module:

- spaces
- questions
- answers
- space, question, and answer tag/source links
- activity and public signaling metadata derived from activity
- lifecycle state for questions, answer activation or archival, and activity-backed public signals

### QnA Hangfire database

`HangfireQnaDbContext` owns the QnA worker's durable Hangfire storage boundary. It is not tenant-scoped and it does not contain QnA domain entities. Runtime registration happens through `AddHangfireQnaDb(...)`, which resolves `ConnectionStrings:HangfireQnaDb`, registers the design-time EF context, and delegates provider setup to `Querify.Common.Infrastructure.Hangfire`.

Hangfire's internal tables belong to `Hangfire.PostgreSql`. Keep the provider version pinned through `Querify.Common.Infrastructure.Hangfire`; do not re-create those tables as Querify entities.

Source upload verification is RabbitMQ-driven, not a recurring Hangfire sweep. Hangfire remains
available in the QnA worker for unrelated operational jobs and a future low-frequency
reconciliation job if stuck `Uploaded` sources need to be re-enqueued.

Each tenant can point to its own module database connection, which is why module migration and seed tooling must resolve tenant metadata first.

### Module DbContext standards

Tenant module persistence should follow these default conventions unless a module has a documented reason not to:

- place the context class in `DbContext/<Module>DbContext.cs`
- place save-time module rules under `DbContext/<Concern>`
- keep `Extensions` folders for service registration only
- load entity configuration through `ConfigurationNamespaces`
- let `BaseDbContext<TContext>` apply soft-delete rules, audit rules, UTC date normalization, tenant filters, and tenant indexes
- put module invariants that must run before audit/history in `OnBeforeSaveChangesRules()`
- put auto-history capture in `OnBeforeSaveChanges()` so it runs after soft-delete and audit fields are applied

Tenant integrity is a `DbContext` responsibility, not a command-handler convention. If an `IMustHaveTenant` entity references another tenant-owned record, the owning module context must enforce the relationship before save.

The default tenant-integrity pattern is:

- add a private `EnsureTenantIntegrity()` method on the owning context
- call it from `OnBeforeSaveChangesRules()`
- create one focused extension per checked entity or relationship under `DbContext/TenantIntegrity/<Entity>TenantIntegrityExtension.cs`
- use `TenantIntegrityGuard` for tenant comparisons
- use `TenantIntegrityLookupCacheBase` or a module-specific `TenantIntegrityLookupCache` to resolve referenced tenant ids with `IgnoreQueryFilters()`
- validate added and modified relationship rows, plus explicit append-only restrictions where the entity requires them
- throw when a referenced record is missing or belongs to another tenant
- avoid empty tenant-integrity extensions when an entity has no tenant-owned relationships

### Direct and Broadcast databases

`DirectDbContext` and `BroadcastDbContext` are present as tenant module persistence boundaries for the Querify module split described in [`../../business/value_proposition.md`](../../business/value_proposition.md).

`DirectDbContext` stores the 1:1 resolution behavior that should not live in QnA:

- conversations
- conversation messages

`BroadcastDbContext` stores the public and community interaction behavior that should not live in QnA:

- external and community interaction threads
- captured thread items

These projects define the current entity, enum, configuration, DbContext, and registration-extension scope for their modules. API hosts, business modules, migrations, additional workflow entities, and seed flows belong in the same module boundary as the behavior.

Trust has no active persistence project in this repository snapshot. Validation, governance, and auditability data belongs to the Trust module boundary instead of sharing QnA, Direct, or Broadcast persistence by default.

## Multitenancy model

### Authenticated flows

- BackOffice uses JWT auth but does not always require tenant scoping.
- Portal APIs use JWT auth and usually require `X-Tenant-Id`.

### Public flows

- QnA Public resolves the tenant from `X-Client-Key`.
- Public handlers use tenant resolution before reading or writing tenant QnA data.
- Tenant Public billing webhooks are anonymous ingress endpoints and do not rely on `X-Tenant-Id` or `X-Client-Key`.
- Tenant identity for billing may be resolved later by the worker from provider metadata and normalized billing records.

## Local backend startup

The usual backend bootstrap sequence is:

1. Start base services with `./devops/local/docker/base.sh`.
2. On a clean environment, initialize schema and data with `Querify.Tools.Seed`.
3. Use `Querify.Tools.Migration` when you need to apply supported module schema updates across tenant module databases.
4. Run the specific APIs needed for the workflow you are testing.

Typical command set:

```bash
dotnet run --project dotnet/Querify.Tenant.BackOffice.Api
dotnet run --project dotnet/Querify.Tenant.Portal.Api
dotnet run --project dotnet/Querify.Tenant.Public.Api
dotnet run --project dotnet/Querify.QnA.Portal.Api
dotnet run --project dotnet/Querify.QnA.Public.Api
dotnet run --project dotnet/Querify.Tenant.Worker.Api
dotnet run --project dotnet/Querify.QnA.Worker.Api
```

For the full local operations model, see [`../tools/local-development.md`](../tools/local-development.md).
For worker-specific configuration and feature guidance, see [`querify-tenant-worker.md`](querify-tenant-worker.md).

## Development conventions

- Add new features to the correct bounded-context project rather than enlarging an unrelated one.
- For module backend work, keep source files real inside the owning feature project instead of creating a monolithic or linked-source business project.
- Keep behavior in its owning module project; do not model another module's workflow in QnA entities as a shortcut.
- Preserve the API-host composition pattern through `AddFeatures(...)`.
- Keep controllers and services thin; push actual use-case behavior into handlers and domain-specific services.
- Optimize `GET` and query handlers as the default read contract: use no-tracking DTO projections, avoid `Include`, page before loading child details, and add indexes plus migrations for new filters or sort fields.
- Prefer lowercase kebab-case in route path segments when a controller exposes named actions beyond plain resource ids.
- Treat tenant, QnA, Direct, Broadcast, and Trust data as separate ownership boundaries.
- Treat tenant integrity as a module `DbContext` save-time responsibility whenever tenant-owned entities reference each other.
- Put public tenant ingress endpoints such as billing webhooks in `Querify.Tenant.Public.Api`, not in authenticated portal hosts.
- Update the corresponding docs when request headers, ports, startup requirements, or operational assumptions change.
