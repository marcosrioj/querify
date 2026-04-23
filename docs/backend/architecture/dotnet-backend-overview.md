# BaseFAQ .NET Backend Overview

## Purpose

This guide explains how the backend is organized under `dotnet/`, which APIs exist, how multitenancy works, and how to reason about the common development flow.

## Service catalog

| API | Responsibility | Auth | Tenant context | Local port |
|---|---|---|---|---:|
| `BaseFaq.Tenant.BackOffice.Api` | global administration of tenants, tenant users, billing, and tenant metadata | Auth0 JWT | none by default | `5000` |
| `BaseFaq.Tenant.Portal.Api` | tenant workspace settings and tenant-member operations | Auth0 JWT | `X-Tenant-Id` for tenant-scoped operations | `5002` |
| `BaseFaq.Tenant.Public.Api` | public tenant ingress endpoints such as Stripe webhooks | public surface | none | `5004` |
| `BaseFaq.QnA.Portal.Api` | authenticated QnA management for spaces, questions, answers, tags, sources, workflow, and activity | Auth0 JWT | `X-Tenant-Id` | `5010` |
| `BaseFaq.QnA.Public.Api` | public QnA access plus vote and feedback signaling over questions and answers | public surface | `X-Client-Key` | `5020` |

| Worker | Responsibility | Data boundary | Local port |
|---|---|---|---:|
| `BaseFaq.Tenant.Worker.Api` | control-plane background processing for billing webhooks, email outbox, and future platform jobs | `TenantDbContext` only | n/a |

## Project taxonomy inside `dotnet/`

`BaseFaq.sln` includes the active `.NET` projects used by the local backend. The inventory below reflects the projects that are actually in the solution, not every folder that exists under `dotnet/`.

### API hosts

These projects contain ASP.NET Core startup, middleware, and DI registration:

- `BaseFaq.QnA.Portal.Api`
- `BaseFaq.QnA.Public.Api`
- `BaseFaq.Tenant.BackOffice.Api`
- `BaseFaq.Tenant.Portal.Api`
- `BaseFaq.Tenant.Public.Api`

### Worker hosts

- `BaseFaq.Tenant.Worker.Api`

### Business modules

Each service area is split into feature projects.

QnA is the product surface and remains split by feature project instead of monolithic business assemblies.

- QnA Portal:
  - `BaseFaq.QnA.Portal.Business.Space`
  - `BaseFaq.QnA.Portal.Business.Question`
  - `BaseFaq.QnA.Portal.Business.Answer`
  - `BaseFaq.QnA.Portal.Business.Tag`
  - `BaseFaq.QnA.Portal.Business.Source`
  - `BaseFaq.QnA.Portal.Business.Activity`
- QnA Public:
  - `BaseFaq.QnA.Public.Business.Space`
  - `BaseFaq.QnA.Public.Business.Question`
  - `BaseFaq.QnA.Public.Business.Vote`
  - `BaseFaq.QnA.Public.Business.Feedback`
- Tenant BackOffice:
  - `BaseFaq.Tenant.BackOffice.Business.Tenant`
  - `BaseFaq.Tenant.BackOffice.Business.User`
  - `BaseFaq.Tenant.BackOffice.Business.Billing`
- Tenant Portal:
  - `BaseFaq.Tenant.Portal.Business.Tenant`
  - `BaseFaq.Tenant.Portal.Business.User`
- Tenant Public:
  - `BaseFaq.Tenant.Public.Business.Billing`
- Tenant Worker:
  - `BaseFaq.Tenant.Worker.Business.Billing`
  - `BaseFaq.Tenant.Worker.Business.Email`

### Shared infrastructure and persistence

- `BaseFaq.Common.EntityFramework.Core`: shared EF Core helpers and database infrastructure used across the solution
- `BaseFaq.Common.EntityFramework.Tenant`: tenant database context, tenant resolution helpers, and shared tenant infrastructure
- `BaseFaq.QnA.Common.Persistence.QnADb`: QnA database context and QnA-side persistence
- `BaseFaq.Common.Infrastructure.Core`: shared core abstractions and backend helper services
- `BaseFaq.Common.Infrastructure.ApiErrorHandling`: API error handling conventions
- `BaseFaq.Common.Infrastructure.MassTransit`: MassTransit registration and messaging conventions
- `BaseFaq.Common.Infrastructure.MediatR`: MediatR integration and related pipeline behavior
- `BaseFaq.Common.Infrastructure.Mvc`: MVC filters and ASP.NET Core glue
- `BaseFaq.Common.Infrastructure.Sentry`: Sentry integration
- `BaseFaq.Common.Infrastructure.Swagger`: Swagger/OpenAPI wiring
- `BaseFaq.Common.Infrastructure.Telemetry`: shared telemetry wiring (OpenTelemetry tracing, OTLP export)
- `BaseFaq.Models.Common`: shared primitive DTOs and common contracts
- `BaseFaq.Models.QnA`: QnA-facing contracts
- `BaseFaq.Models.Tenant`: tenant-facing contracts
- `BaseFaq.Models.User`: user and profile contracts

### Tests and tools

- `BaseFaq.QnA.Portal.Test.IntegrationTests`
- `BaseFaq.QnA.Public.Test.IntegrationTests`
- `BaseFaq.Tenant.BackOffice.Test.IntegrationTests`
- `BaseFaq.Tenant.Portal.Test.IntegrationTests`
- `BaseFaq.Tenant.Public.Test.IntegrationTests`
- `BaseFaq.Tenant.Worker.Test.IntegrationTests`
- `BaseFaq.Common.Architecture.Test.IntegrationTest`
- `BaseFaq.Tools.Migration`
- `BaseFaq.Tools.Seed`

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
- API hosts compose multiple feature-owned business modules rather than one catch-all business assembly
- QnA uses the same one-business-project-per-feature pattern across the bounded context
- QnA feature projects keep real source files inside the owning project directory and avoid linked compile items
- QnA integration tests should use feature folders and names such as `Tests/Answer/AnswerCommandQueryTests.cs`
- HTTP route segments should use lowercase kebab-case for action-style paths such as `add-tenant-member` or `refresh-allowed-tenant-cache`

The write-side rules are formalized in [`solution-cqrs-write-rules.md`](solution-cqrs-write-rules.md).

## Persistence model

### Tenant database

`TenantDbContext` stores:

- tenants
- users
- tenant memberships
- tenant-to-QnA database connection strings
- client keys
- control-plane background-processing state such as billing webhook inbox records and email outbox records
- normalized billing state such as billing customers, subscriptions, invoices, payments, and entitlement snapshots

This is the global control plane for the platform.

That also means these responsibilities belong in `TenantDbContext` and not in tenant product persistence:

- billing webhook inboxes
- email outbox
- tenant entitlements
- platform recurring jobs

### QnA databases

`QnADbContext` stores tenant product data for the QnA model:

- spaces
- questions
- answers
- space, question, and answer tag/source links
- activity and public signaling metadata derived from activity
- workflow state for question moderation and answer publication or validation

Each tenant can point to its own QnA database connection, which is why QnA migration and seed tooling must resolve tenant metadata first.

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

1. Start base services with `./docker-base.sh`.
2. On a clean environment, initialize schema and data with `BaseFaq.Tools.Seed`.
3. Use `BaseFaq.Tools.Migration` when you need to apply QnA schema updates across tenant product databases.
4. Run the specific APIs needed for the workflow you are testing.

Typical command set:

```bash
dotnet run --project dotnet/BaseFaq.Tenant.BackOffice.Api
dotnet run --project dotnet/BaseFaq.Tenant.Portal.Api
dotnet run --project dotnet/BaseFaq.Tenant.Public.Api
dotnet run --project dotnet/BaseFaq.QnA.Portal.Api
dotnet run --project dotnet/BaseFaq.QnA.Public.Api
dotnet run --project dotnet/BaseFaq.Tenant.Worker.Api
```

For the full local operations model, see [`../tools/local-development.md`](../tools/local-development.md).
For worker-specific configuration and feature guidance, see [`basefaq-tenant-worker.md`](basefaq-tenant-worker.md).

## Development conventions

- Add new features to the correct bounded-context project rather than enlarging an unrelated one.
- For QnA backend work, keep source files real inside the owning feature project instead of creating a monolithic or linked-source business project.
- Preserve the API-host composition pattern through `AddFeatures(...)`.
- Keep controllers and services thin; push actual use-case behavior into handlers and domain-specific services.
- Prefer lowercase kebab-case in route path segments when a controller exposes named actions beyond plain resource ids.
- Treat tenant data and QnA data as separate ownership boundaries.
- Put public tenant ingress endpoints such as billing webhooks in `BaseFaq.Tenant.Public.Api`, not in authenticated portal hosts.
- Update the corresponding docs when request headers, ports, startup requirements, or operational assumptions change.
