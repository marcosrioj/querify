# BaseFAQ Solution Architecture

## Purpose

This document explains how the repository is organized, how the runtime is split across services, and which architectural patterns are consistently used across the solution.

## Solution shape

The repository root contains one primary `.NET` solution file, `BaseFaq.sln`. It includes the active backend, persistence, worker, tooling, and sample projects under `dotnet/`, while `apps/portal` remains a separate frontend app outside the `.sln`.

| Delivery root | Responsibility |
|---|---|
| `apps/portal` | tenant-facing web application for authenticated workspace flows |
| `dotnet` | API hosts, business modules, persistence projects, shared infrastructure, tests, and console tools |
| `docker` | local base services, split backend/frontend container runtime, and helper scripts that compose the full stack from those files |
| `local` | local-only helpers such as reverse proxy and subdomain simulation |
| `docs` | architecture, operations, developer workflows, and standards |

## Runtime surfaces

| Surface | Role | Local port |
|---|---|---:|
| `apps/portal` | authenticated tenant portal frontend | `5500` |
| `BaseFaq.Tenant.BackOffice.Api` | back-office tenant and user administration | `5000` |
| `BaseFaq.Tenant.Portal.Api` | tenant workspace management APIs | `5002` |
| `BaseFaq.Tenant.Public.Api` | public tenant ingress APIs such as Stripe webhooks | `5004` |
| `BaseFaq.QnA.Portal.Api` | authenticated QnA management APIs | `5010` |
| `BaseFaq.QnA.Public.Api` | public QnA access and public signaling APIs | `5020` |
| `BaseFaq.Tenant.Worker.Api` | control-plane worker for billing webhooks and email outbox processing | n/a |

## Core architectural patterns

### 1. Runtime hosts are composition roots

Each runtime host owns:

- ASP.NET Core startup or generic-host startup
- dependency injection registration
- feature registration through `AddFeatures(...)` or a worker-specific equivalent

API hosts additionally own:

- middleware ordering
- auth and Swagger configuration

Business logic does not live in the host; the host wires the feature modules and infrastructure together.

### 2. Business code is split by module

The repository follows a consistent naming model. The current BaseFaq modules are Tenant, QnA, Direct, Broadcast, and Trust.

| Module | Role |
|---|---|
| Tenant | Control plane for workspaces, users, permissions, billing, entitlements, public keys, and module connection routing |
| QnA | Approved knowledge, questions, answers, sources, tags, workflow state, and public QnA signals |
| Direct | Direct 1:1 resolution conversations, messages, handoff context, and agent-assist records |
| Broadcast | Public and community interaction capture, response coordination, and social signal records |
| Trust | Validation, governance, decision history, and auditability records |

Project prefixes follow the same module names:

- `BaseFaq.Tenant.*`
- `BaseFaq.QnA.*`
- `BaseFaq.Direct.*`
- `BaseFaq.Broadcast.*`
- `BaseFaq.Trust.*`

Inside each area, business modules are further split by feature, for example:

- `BaseFaq.QnA.Portal.Business.Space`
- `BaseFaq.QnA.Portal.Business.Question`
- `BaseFaq.QnA.Portal.Business.Answer`
- `BaseFaq.QnA.Portal.Business.Source`
- `BaseFaq.QnA.Portal.Business.Activity`
- `BaseFaq.QnA.Public.Business.Space`
- `BaseFaq.QnA.Public.Business.Question`
- `BaseFaq.Tenant.Portal.Business.Tenant`
- `BaseFaq.Tenant.BackOffice.Business.User`
- `BaseFaq.Tenant.Public.Business.Billing`
- `BaseFaq.Tenant.BackOffice.Business.Billing`

This keeps controller, service, command, and query code grouped by domain capability instead of by technical layer alone. Each module uses the same feature-scoped ownership rule: add behavior to the smallest module feature project that owns the use case, keep source files in that project, and compose API hosts from feature registrations.

The current runtime catalog contains Tenant and QnA API/business projects. Direct and Broadcast are represented by their persistence boundaries. Trust is part of the module taxonomy and has no active runtime project in this repository snapshot.

### 3. CQRS with MediatR is the standard application pattern

Write and read paths are separated through commands and queries. The usual flow is:

1. Controller receives the request.
2. A thin application service coordinates the use case.
3. The service dispatches a MediatR command or query.
4. The handler executes validation, persistence, and event publication.

The write-side rules are formalized in [`solution-cqrs-write-rules.md`](solution-cqrs-write-rules.md).

### 4. Controllers stay thin

Controllers are expected to:

- expose routes
- map HTTP concerns
- delegate to services
- return simple write results or query DTOs

They should not contain read-after-write orchestration, persistence logic, or cross-cutting infrastructure code.

### 5. Persistence is explicitly split by database responsibility

BaseFAQ uses separate EF Core context boundaries for module data responsibilities:

| Module | Context | Responsibility |
|---|---|---|
| Tenant | `TenantDbContext` | global tenant metadata, users, tenant memberships, module connection mapping, billing, entitlements, and control-plane background-processing state |
| QnA | QnA module `DbContext` | tenant-specific QnA module data such as spaces, questions, answers, source links, tag links, workflow state, and activity |
| Direct | `DirectDbContext` | tenant-specific Direct module data such as conversations and conversation messages |
| Broadcast | `BroadcastDbContext` | tenant-specific Broadcast module data such as external/community threads and captured items |
| Trust | no active EF context | validation, governance, decision history, and auditability records belong to the Trust module boundary |

The split matters operationally:

- tenant metadata is centralized
- control-plane operational workloads belong with tenant metadata
- module data lives behind its owning module context
- migration and seed tooling must coordinate tenant metadata plus the active module store

Module contexts share the same default `DbContext` pattern. The context class lives under `DbContext/<Module>DbContext.cs`; save-time persistence concerns live under focused `DbContext/<Concern>` folders; and `Extensions` folders are reserved for service registration. `BaseDbContext<TContext>` owns the shared EF behavior: tenant connection resolution, tenant filters and indexes, soft-delete filters and rules, audit rules, UTC date normalization, and save hooks.

Module persistence uses `OnBeforeSaveChangesRules()` for pre-save invariants and tenant integrity, then lets `BaseDbContext<TContext>` apply soft delete, audit, and date normalization, and finally uses `OnBeforeSaveChanges()` for auto history so history rows include audit state.

Tenant integrity is a mandatory `DbContext` responsibility for tenant module data. When an `IMustHaveTenant` entity references another tenant-owned record, the owning context must validate the relationship before save. The default shape is `DbContext/TenantIntegrity/<Entity>TenantIntegrityExtension.cs`, one focused extension per checked entity or relationship, backed by `TenantIntegrityGuard` and `TenantIntegrityLookupCacheBase` or a module-specific lookup cache that reads referenced records with `IgnoreQueryFilters()`.

`BaseFaq.Direct.Common.Persistence.DirectDb` and `BaseFaq.Broadcast.Common.Persistence.BroadcastDb` contain the current Direct and Broadcast entity, enum, configuration, DbContext, and registration-extension scope. API hosts, business modules, migrations, additional workflow entities, and seed flows for those behaviors belong in the same module boundaries.

### 6. Multitenancy is part of the request model

The solution uses different request contexts depending on the surface:

- authenticated portal flows use `X-Tenant-Id`
- public QnA flows use `X-Client-Key`
- public tenant billing ingress uses anonymous webhook routes with provider signature validation instead of tenant headers
- shared services resolve the tenant context before hitting tenant-scoped data

Tenant resolution is not an optional add-on; it is part of the backend contract.

### 7. Control-plane background work is isolated from request APIs

Control-plane background processing is hosted separately in `BaseFaq.Tenant.Worker.Api`.

That separation is intentional:

- billing webhooks, email outbox delivery, entitlements, and recurring tenant operational jobs belong to `BaseFaq.Tenant.Worker.Api`
- the worker should operate against `TenantDbContext` and should not take ownership of product module workflows

### 8. Cross-cutting concerns are centralized in shared libraries

The `BaseFaq.Common.Infrastructure.*` and `BaseFaq.Common.EntityFramework.*` projects encapsulate recurring concerns:

- authentication and session access
- tenant resolution
- tenant filters and tenant-integrity helpers for module `DbContext` implementations
- audit, soft-delete, and auto-history persistence concerns
- Swagger/OpenAPI
- API error handling
- Sentry integration
- MediatR logging
- MassTransit conventions
- telemetry wiring

Feature projects should consume these shared building blocks rather than re-implementing their own versions.

### 9. Integration tests prefer real infrastructure

The solution already contains integration test projects per major service area. The testing style favors:

- real PostgreSQL usage
- real EF Core migrations
- realistic tenant and session context
- validation of business rules close to production behavior

The testing strategy is documented in [`../testing/integration-testing-strategy.md`](../testing/integration-testing-strategy.md).

## End-to-end request model

### Authenticated portal flow

1. The Portal frontend authenticates through Auth0.
2. The frontend calls a protected API with a bearer token.
3. Tenant-scoped requests include `X-Tenant-Id`.
4. The API host resolves session and tenant context.
5. A business module executes the command or query against the correct `DbContext`.

### Public module flow: QnA

1. A public client calls the QnA Public API.
2. The request includes `X-Client-Key`.
3. The API resolves the tenant behind that client key.
4. Public QnA data is served from the tenant's QnA database, and public feedback or vote signals are recorded through QnA activity.

### Public billing webhook flow

1. Stripe calls `BaseFaq.Tenant.Public.Api`.
2. The API reads the exact raw request body and validates the Stripe signature.
3. The API persists a `BillingWebhookInbox` record in `TenantDbContext`.
4. `BaseFaq.Tenant.Worker.Api` claims and processes the inbox item asynchronously.

## Practical guidance for contributors

- Use [`../../execution-guide.md`](../../execution-guide.md) first when you need to choose the correct workflow or owning documentation boundary.
- Preserve the existing composition-root pattern in API hosts.
- Add new business features under the appropriate bounded-context module instead of enlarging unrelated projects.
- Keep write flows simple and aligned with the CQRS rules.
- Treat Tenant, QnA, Direct, Broadcast, and Trust as separate module ownership boundaries.
- Keep behavior inside its owning BaseFaq module boundary instead of folding it into a module with a similar enum, source, channel, or activity value.
- Add or update tenant-integrity rules in the owning module `DbContext` whenever tenant-owned relationships change.
- Put public tenant ingress endpoints such as billing webhooks in `BaseFaq.Tenant.Public.Api`, not in authenticated portal hosts.
- Update the specific docs in `docs/` when boundaries, startup steps, or operational assumptions change.
