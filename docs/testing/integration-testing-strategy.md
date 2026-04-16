# Integration Testing Strategy

## Purpose

This document explains what integration testing means in BaseFAQ, which parts of the solution are already covered, and where the current test investment should grow next.

## Guiding principle

BaseFAQ integration tests should validate real behavior across component boundaries with production-like rules, not mocked approximations of the stack.

That means preferring:

- real PostgreSQL
- real EF Core migrations
- realistic tenant and session context
- realistic middleware and handler execution

## Current automated test landscape

The repository currently contains these backend-facing automated test projects:

- `BaseFaq.Faq.Portal.Test.IntegrationTests`
- `BaseFaq.Faq.Public.Test.IntegrationTests`
- `BaseFaq.Tenant.BackOffice.Test.IntegrationTests`
- `BaseFaq.Tenant.Portal.Test.IntegrationTests`
- `BaseFaq.Common.Architecture.Test.IntegrationTest`

The first four focus on service behavior. The architecture test project enforces repository rules such as the write-side contract expectations from `PROJECT_RULES.md`.

## What counts as an integration test here

An integration test should verify at least two real components working together, for example:

- handler plus EF Core plus PostgreSQL
- middleware plus auth/session context plus persistence
- controller plus tenant resolution plus database state
- event publication plus consumer execution

What it is not:

- a pure unit test with mocked repositories
- a browser-level end-to-end test
- a DTO-only contract test without runtime behavior

## Current strengths

The current test suite is strongest on:

- command and query correctness
- tenant-aware persistence rules
- soft-delete and filter behavior
- background worker and platform-flow integration coverage
- repository rule-compliance checks for command/write conventions

## Current weaker areas

The weaker areas are still:

- full API auth and authorization coverage
- queue callback end-to-end verification back into the caller services
- observability assertions
- resilience and fault-injection scenarios
- Redis-backed collaboration paths

## Highest-risk areas

| Area | Why it matters |
|---|---|
| tenant isolation | a regression here becomes a cross-tenant data leak |
| auth and claim mapping | protected APIs can silently fail open or fail closed |
| migrations | schema drift blocks releases and breaks runtime startup |
| public client-key resolution | public FAQ traffic depends on correct tenant resolution |
| background processing flows | retries, leases, or duplicate handling can change system behavior |

## Execution tiers

### PR / CI tier

Use this tier for fast, deterministic checks that should block merges.

Focus:

- real database-backed tests
- command and query correctness
- tenant isolation
- critical auth and negative-path coverage

### Nightly tier

Use this tier for heavier cases that are useful but too expensive for every PR.

Focus:

- concurrency and race conditions
- migration drift
- retry and idempotency behavior
- Redis, RabbitMQ, and other container-backed collaboration points

### Pre-release tier

Use this tier to validate production readiness.

Focus:

- the full integration pack
- provider sandbox checks where applicable
- backward compatibility and observability validation

## Environment strategy

- use isolated databases per run when possible
- keep test data deterministic
- avoid sharing mutable global state between tests
- prefer builders and factory helpers over ad hoc entity setup
- never depend on test execution order

## Data strategy

- use synthetic data only
- keep time-sensitive tests deterministic by controlling timestamps explicitly
- seed only the minimum scenario data needed for the test

## Test commands

Run the current service integration suites individually:

```bash
dotnet test dotnet/BaseFaq.Faq.Portal.Test.IntegrationTests/BaseFaq.Faq.Portal.Test.IntegrationTests.csproj
dotnet test dotnet/BaseFaq.Faq.Public.Test.IntegrationTests/BaseFaq.Faq.Public.Test.IntegrationTests.csproj
dotnet test dotnet/BaseFaq.Tenant.BackOffice.Test.IntegrationTests/BaseFaq.Tenant.BackOffice.Test.IntegrationTests.csproj
dotnet test dotnet/BaseFaq.Tenant.Portal.Test.IntegrationTests/BaseFaq.Tenant.Portal.Test.IntegrationTests.csproj
```

Run the architecture rules suite:

```bash
dotnet test dotnet/BaseFaq.Common.Architecture.Test.IntegrationTest/BaseFaq.Common.Architecture.Test.IntegrationTest.csproj
```

## Current priorities

### Must stay strong

- tenant-aware DB behavior
- CRUD and business-rule flows in the FAQ and tenant domains
- migration application from a clean database
- architecture compliance around command and write contracts

### Should be expanded next

- API-level auth coverage across protected endpoints
- queue-driven platform worker scenarios
- Redis-backed tenant access behavior
- failure-path testing for provider or infrastructure outages

## Practical rules for new tests

- use real infrastructure unless the dependency is intentionally outside the test boundary
- keep assertions tied to business behavior, not only implementation details
- cover both happy paths and security or tenancy negative paths
- when a production dependency becomes mandatory, update the tests instead of weakening production code
- add architecture-rule assertions when the repository introduces new cross-cutting standards

## Related documents

- [`../backend/dotnet-backend-overview.md`](../backend/dotnet-backend-overview.md)
- [`../standards/solution-cqrs-write-rules.md`](../standards/solution-cqrs-write-rules.md)
- [`../../PROJECT_RULES.md`](../../PROJECT_RULES.md)
