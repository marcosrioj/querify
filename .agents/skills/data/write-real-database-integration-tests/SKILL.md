---
name: write-real-database-integration-tests
description: Validate BaseFAQ behavior against real PostgreSQL-backed integration flows instead of mocked approximations.
category: data
priority: high
triggers:
  - add integration test
  - verify tenant isolation
  - real db test
  - handler integration coverage
owned_paths:
  - dotnet/BaseFaq.*.Test.IntegrationTests
  - dotnet/BaseFaq.Common.Architecture.Test.IntegrationTest
collaborates_with:
  - build-cqrs-feature-module
  - process-control-plane-work-items
  - apply-seed-and-migrations-safely
---

# Write Real Database Integration Tests

## When to Use

- A backend or worker behavior change needs production-like verification.
- The risk area involves tenancy, persistence, retries, migrations, or public credential resolution.

## Responsibilities

- Use isolated databases and real EF Core migrations.
- Verify happy paths and negative paths that matter to BaseFAQ.
- Keep test data deterministic and minimal.

## Workflow

1. Start from the repository test helper path for the target service area.
2. Let the helper create and migrate a real database.
3. Seed only the records needed for the scenario.
4. Execute the real handler, middleware, processor, or controller path.
5. Assert both returned outcomes and persisted state changes.
6. Cover at least one security, tenancy, or failure edge.

## BaseFAQ Domain Alignment

- Tenant isolation is a top-risk area and deserves explicit tests.
- Production contracts win over test convenience.
- Integration tests are the primary safety net for worker and persistence behavior.

## Collaborates With

- [`build-cqrs-feature-module`](../../backend/build-cqrs-feature-module/SKILL.md)
- [`process-control-plane-work-items`](../process-control-plane-work-items/SKILL.md)
- [`apply-seed-and-migrations-safely`](../apply-seed-and-migrations-safely/SKILL.md)

## Done When

- The changed behavior is covered with real DB-backed verification.
- The test proves both a normal path and a high-risk edge.
- The test suite enforces production behavior rather than a weakened variant.
