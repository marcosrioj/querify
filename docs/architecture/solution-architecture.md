# BaseFAQ Solution Architecture

## Purpose

This document explains how the repository is organized, how the runtime is split across services, and which architectural patterns are consistently used across the solution.

## Solution shape

The repository root contains one primary `.NET` solution file, `BaseFaq.sln`. It currently includes 51 `.NET` projects under `dotnet/`, while `apps/portal` remains a separate frontend app outside the `.sln`. Two AI scaffold projects also exist under `dotnet/` but are not currently included in the solution: `BaseFaq.AI.Common.Contracts` and `BaseFaq.AI.Common.VectorStore`.

| Delivery root | Responsibility |
|---|---|
| `apps/portal` | Tenant-facing web application for authenticated workspace flows |
| `dotnet` | API hosts, business modules, persistence projects, shared infrastructure, tests, and console tools |
| `docker` | Local base services and containerized app/API runtime |
| `local` | Local-only helpers such as reverse proxy and subdomain simulation |
| `azure` | Stage-based deployment automation for `dev`, `qa`, and `prod` |
| `docs` | Architecture, operations, developer workflows, and standards |

## Runtime surfaces

| Surface | Role | Local port |
|---|---|---:|
| `apps/portal` | Authenticated tenant portal frontend | `5500` |
| `BaseFaq.Tenant.BackOffice.Api` | back-office tenant and user administration | `5000` |
| `BaseFaq.Tenant.Portal.Api` | tenant workspace management APIs | `5002` |
| `BaseFaq.Faq.Portal.Api` | authenticated FAQ management APIs | `5010` |
| `BaseFaq.Faq.Public.Api` | public FAQ access and public FAQ item creation | `5020` |
| `BaseFaq.AI.Api` | AI worker host for generation and matching plus health endpoint | `5030` |
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

- `BaseFaq.Faq.*`
- `BaseFaq.Tenant.*`
- `BaseFaq.AI.*`

Inside each area, business modules are further split by feature, for example:

- `BaseFaq.Faq.Portal.Business.Faq`
- `BaseFaq.Faq.Portal.Business.FaqItem`
- `BaseFaq.Tenant.Portal.Business.Tenant`
- `BaseFaq.Tenant.BackOffice.Business.User`

This keeps controller, service, command, and query code grouped by domain capability instead of by technical layer alone.

### 3. CQRS with MediatR is the standard application pattern

Write and read paths are separated through commands and queries. The usual flow is:

1. Controller receives the request.
2. A thin application service coordinates the use case.
3. The service dispatches a MediatR command or query.
4. The handler executes validation, persistence, and event publication.

The write-side rules are formalized in [`../standards/solution-cqrs-write-rules.md`](../standards/solution-cqrs-write-rules.md).

### 4. Controllers stay thin

Controllers are expected to:

- expose routes
- map HTTP concerns
- delegate to services
- return simple write results or query DTOs

They should not contain read-after-write orchestration, persistence logic, or cross-cutting infrastructure code.

### 5. Persistence is explicitly split by database responsibility

BaseFAQ uses two main EF Core contexts:

| Context | Responsibility |
|---|---|
| `TenantDbContext` | global tenant metadata, users, tenant memberships, AI provider credentials, tenant-to-database mapping, and control-plane background-processing state |
| `FaqDbContext` | tenant-specific FAQ product data such as FAQs, FAQ items, content references, tags, and feedback |

The split matters operationally:

- tenant metadata is centralized
- control-plane operational workloads belong with tenant metadata
- FAQ data lives in tenant databases
- migration and seed tooling must coordinate both stores

### 6. Multitenancy is part of the request model

The solution uses different request contexts depending on the surface:

- authenticated portal flows use `X-Tenant-Id`
- public FAQ flows use `X-Client-Key`
- shared services resolve the tenant context before hitting tenant-scoped data

Tenant resolution is not an optional add-on; it is part of the backend contract.

### 7. AI integration is event-driven

Generation and matching are not implemented as direct synchronous controller-to-provider calls. Instead:

- FAQ services publish RabbitMQ events
- the AI worker host consumes those events
- the AI worker publishes completion or failure callbacks

This keeps provider latency and retries out of the main request path and allows AI work to evolve independently of the CRUD APIs.

### 8. Control-plane background work is isolated from AI work

Control-plane background processing is hosted separately in `BaseFaq.Tenant.Worker.Api`.

That separation is intentional:

- AI generation and matching belong to `BaseFaq.AI.Api`
- billing webhooks, email outbox delivery, entitlements, and recurring tenant operational jobs belong to `BaseFaq.Tenant.Worker.Api`
- the worker should operate against `TenantDbContext` and should not take ownership of FAQ product-data workflows

### 9. Cross-cutting concerns are centralized in shared libraries

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

### 10. Integration tests prefer real infrastructure

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
5. A business module executes the command or query against the correct DbContext.

### Public FAQ flow

1. A public client calls the FAQ Public API.
2. The request includes `X-Client-Key`.
3. The API resolves the tenant behind that client key.
4. Public FAQ data is served from the tenant's FAQ database.

### AI flow

1. A FAQ feature publishes an event to RabbitMQ.
2. The AI worker host consumes the event.
3. The AI worker calls the configured provider for that tenant and AI command type.
4. The worker publishes a callback event back into the system.

The current AI implementation details are documented in [`basefaq-ai-generation-matching-architecture.md`](basefaq-ai-generation-matching-architecture.md).

## Practical guidance for contributors

- Preserve the existing composition-root pattern in API hosts.
- Add new business features under the appropriate bounded-context module instead of enlarging unrelated projects.
- Keep write flows simple and aligned with the CQRS rules.
- Treat `TenantDbContext` and `FaqDbContext` as separate ownership boundaries.
- Update the specific docs in `docs/` when boundaries, startup steps, or operational assumptions change.
