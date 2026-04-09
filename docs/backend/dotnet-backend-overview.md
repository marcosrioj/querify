# BaseFAQ .NET Backend Overview

## Purpose

This guide explains how the backend is organized under `dotnet/`, which APIs exist, how multitenancy works, and how to reason about the common development flow.

## Service catalog

| API | Responsibility | Auth | Tenant context | Local port |
|---|---|---|---|---:|
| `BaseFaq.Tenant.BackOffice.Api` | global administration of tenants, tenant users, AI providers, and tenant metadata | Auth0 JWT | none by default | `5000` |
| `BaseFaq.Tenant.Portal.Api` | tenant workspace settings and tenant-member operations | Auth0 JWT | `X-Tenant-Id` for tenant-scoped operations | `5002` |
| `BaseFaq.Faq.Portal.Api` | authenticated FAQ management, content references, tags, feedbacks, generation request entrypoint | Auth0 JWT | `X-Tenant-Id` | `5010` |
| `BaseFaq.Faq.Public.Api` | public FAQ access and public FAQ item creation flow | public surface | `X-Client-Key` | `5020` |
| `BaseFaq.AI.Api` | AI worker host and health endpoint | no user-facing auth flow | tenant inferred from message payload | `5030` |

## Project taxonomy inside `dotnet/`

`BaseFaq.sln` currently includes 47 `.NET` projects. The inventory below reflects the projects that are actually in the solution, not every folder that exists under `dotnet/`.

### API hosts

These projects contain ASP.NET Core startup, middleware, and DI registration:

- `BaseFaq.Faq.Portal.Api`
- `BaseFaq.Faq.Public.Api`
- `BaseFaq.Tenant.BackOffice.Api`
- `BaseFaq.Tenant.Portal.Api`
- `BaseFaq.AI.Api`

### Business modules

Each service area is split into feature projects:

- FAQ Portal:
  - `BaseFaq.Faq.Portal.Business.Faq`
  - `BaseFaq.Faq.Portal.Business.FaqItem`
  - `BaseFaq.Faq.Portal.Business.ContentRef`
  - `BaseFaq.Faq.Portal.Business.Tag`
  - `BaseFaq.Faq.Portal.Business.Feedback`
- FAQ Public:
  - `BaseFaq.Faq.Public.Business.Faq`
  - `BaseFaq.Faq.Public.Business.FaqItem`
  - `BaseFaq.Faq.Public.Business.Feedback`
- Tenant BackOffice:
  - `BaseFaq.Tenant.BackOffice.Business.Tenant`
  - `BaseFaq.Tenant.BackOffice.Business.User`
  - `BaseFaq.Tenant.BackOffice.Business.AiProvider`
- Tenant Portal:
  - `BaseFaq.Tenant.Portal.Business.Tenant`
  - `BaseFaq.Tenant.Portal.Business.User`
  - `BaseFaq.Tenant.Portal.Business.AiProvider`
- AI:
  - `BaseFaq.AI.Business.Common`
  - `BaseFaq.AI.Business.Generation`
  - `BaseFaq.AI.Business.Matching`

### Shared infrastructure and persistence

- `BaseFaq.Common.EntityFramework.Core`: shared EF Core helpers and database infrastructure used across the solution
- `BaseFaq.Common.EntityFramework.Tenant`: tenant database context, tenant resolution helpers, and shared tenant infrastructure
- `BaseFaq.Faq.Common.Persistence.FaqDb`: FAQ database context and FAQ-side persistence
- `BaseFaq.Common.Infrastructure.Core`: shared core abstractions and backend helper services
- `BaseFaq.Common.Infrastructure.ApiErrorHandling`: API error handling conventions
- `BaseFaq.Common.Infrastructure.MassTransit`: MassTransit registration and messaging conventions
- `BaseFaq.Common.Infrastructure.MediatR`: MediatR integration and related pipeline behavior
- `BaseFaq.Common.Infrastructure.Mvc`: MVC filters and ASP.NET Core glue
- `BaseFaq.Common.Infrastructure.Sentry`: Sentry integration
- `BaseFaq.Common.Infrastructure.Swagger`: Swagger/OpenAPI wiring
- `BaseFaq.Common.Infrasctructure.Telemetry`: shared telemetry wiring
- `BaseFaq.Models.Common`: shared primitive DTOs and common contracts
- `BaseFaq.Models.Faq`: FAQ-facing contracts
- `BaseFaq.Models.Tenant`: tenant-facing contracts
- `BaseFaq.Models.User`: user and profile contracts
- `BaseFaq.Models.Ai`: AI-facing contracts currently used by the active solution code

### Tests, tools, and samples

- `BaseFaq.Faq.Portal.Test.IntegrationTests`
- `BaseFaq.Faq.Public.Test.IntegrationTests`
- `BaseFaq.Tenant.BackOffice.Test.IntegrationTests`
- `BaseFaq.Tenant.Portal.Test.IntegrationTests`
- `BaseFaq.AI.Test.IntegrationTest`
- `BaseFaq.Common.Architecture.Test.IntegrationTest`
- `BaseFaq.Tools.Migration`
- `BaseFaq.Tools.Seed`
- `BaseFaq.Sample.Ai.Generation`

### Repo-only AI scaffolds outside `BaseFaq.sln`

The repository also contains these AI projects under `dotnet/`, but they are not currently included in `BaseFaq.sln`:

- `BaseFaq.AI.Common.Contracts`: parallel contracts project with generation and matching message types
- `BaseFaq.AI.Common.VectorStore`: scaffold project reserved for future vector-store integrations

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
- HTTP route segments should use lowercase kebab-case for action-style paths such as `add-tenant-member` or `refresh-allowed-tenant-cache`

The write-side rules are formalized in [`../standards/solution-cqrs-write-rules.md`](../standards/solution-cqrs-write-rules.md).

## Persistence model

### Tenant database

`TenantDbContext` stores:

- tenants
- users
- tenant memberships
- tenant-to-FAQ database connection strings
- client keys
- tenant AI provider credentials

This is the global control plane for the platform.

### FAQ databases

`FaqDbContext` stores tenant product data:

- FAQs
- FAQ items
- content references
- tags
- feedbacks

Each tenant can point to its own FAQ database connection, which is why migration and seed tooling must resolve tenant metadata first.

## Multitenancy model

### Authenticated flows

- BackOffice uses JWT auth but does not always require tenant scoping.
- Portal APIs use JWT auth and usually require `X-Tenant-Id`.

### Public flows

- FAQ Public resolves the tenant from `X-Client-Key`.
- Public handlers use tenant resolution before reading or writing tenant FAQ data.

### AI flows

- AI workers do not resolve tenants from HTTP headers.
- They receive `TenantId` inside message contracts and use that to resolve provider context and FAQ database access.

## Local backend startup

The usual backend bootstrap sequence is:

1. Start base services with `./docker-base.sh`.
2. On a clean environment, initialize schema and data with `BaseFaq.Tools.Seed`.
3. Use `BaseFaq.Tools.Migration` when you need to apply FAQ schema updates across tenant FAQ databases.
4. Run the specific APIs needed for the workflow you are testing.

Typical command set:

```bash
dotnet run --project dotnet/BaseFaq.Tenant.BackOffice.Api
dotnet run --project dotnet/BaseFaq.Tenant.Portal.Api
dotnet run --project dotnet/BaseFaq.Faq.Portal.Api
dotnet run --project dotnet/BaseFaq.Faq.Public.Api
dotnet run --project dotnet/BaseFaq.AI.Api
```

For the full local operations model, see [`../devops/local-development.md`](../devops/local-development.md).

## Development conventions

- Add new features to the correct bounded-context project rather than enlarging an unrelated one.
- Preserve the API-host composition pattern through `AddFeatures(...)`.
- Keep controllers and services thin; push actual use-case behavior into handlers and domain-specific services.
- Prefer lowercase kebab-case in route path segments when a controller exposes named actions beyond plain resource ids.
- Treat tenant data and FAQ data as separate ownership boundaries.
- Update the corresponding docs when request headers, ports, startup requirements, or operational assumptions change.
