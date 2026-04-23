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
| `BaseFaq.Tenant.Worker.Api` | control-plane worker for billing webhooks, email outbox, and future tenant operations | n/a |

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

### 2. Business code is split by bounded context

The repository follows a consistent naming model:

- `BaseFaq.Tenant.*`
- `BaseFaq.QnA.*`

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

This keeps controller, service, command, and query code grouped by domain capability instead of by technical layer alone. QnA follows a one-feature-per-project physical layout and is the primary product model for backend work.

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

BaseFAQ uses two important EF Core contexts:

| Context | Responsibility |
|---|---|
| `TenantDbContext` | global tenant metadata, users, tenant memberships, tenant-to-database mapping, and control-plane background-processing state |
| `QnADbContext` | tenant-specific QnA product data such as spaces, questions, answers, source links, tag links, workflow state, and activity |

The split matters operationally:

- tenant metadata is centralized
- control-plane operational workloads belong with tenant metadata
- QnA data lives in tenant databases and is the primary product path
- migration and seed tooling must coordinate tenant metadata plus the active product store

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
- the worker should operate against `TenantDbContext` and should not take ownership of QnA product-data workflows

### 8. Cross-cutting concerns are centralized in shared libraries

The `BaseFaq.Common.Infrastructure.*` and `BaseFaq.Common.EntityFramework.*` projects encapsulate recurring concerns:

- authentication and session access
- tenant resolution
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

### Public QnA flow

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
- Treat `TenantDbContext` and `QnADbContext` as separate ownership boundaries.
- Put public tenant ingress endpoints such as billing webhooks in `BaseFaq.Tenant.Public.Api`, not in authenticated portal hosts.
- Update the specific docs in `docs/` when boundaries, startup steps, or operational assumptions change.
