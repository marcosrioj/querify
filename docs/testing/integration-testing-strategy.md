# Integration Testing Strategy

## Purpose

This document explains what integration testing means in BaseFAQ, which areas are already covered, and how tests should be prioritized as the solution evolves.

## Guiding principle

BaseFAQ integration tests should validate real behavior across component boundaries with production-like rules, not mocked approximations of the stack.

That means preferring:

- real PostgreSQL
- real EF Core migrations
- realistic tenant and session context
- realistic middleware and handler execution

## Current test landscape

The solution already contains integration test projects for:

- `BaseFaq.Faq.Portal`
- `BaseFaq.Faq.Public`
- `BaseFaq.Tenant.BackOffice`
- `BaseFaq.Tenant.Portal`
- `BaseFaq.AI`

The current strength of the test suite is business-rule validation around handlers, persistence, tenant filters, and command/query flows.

The weaker areas are still:

- full API auth and authorization coverage
- queue callback end-to-end coverage
- observability assertions
- resilience and fault-injection scenarios

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

## Highest-risk areas

| Area | Why it matters |
|---|---|
| tenant isolation | a regression here becomes a cross-tenant data leak |
| auth and claim mapping | protected APIs can silently fail open or fail closed |
| migrations | schema drift blocks releases and breaks runtime startup |
| public client-key resolution | public FAQ traffic depends on correct tenant resolution |
| event-driven AI flows | message loss or duplicate handling changes system behavior |

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

Run the current integration suites individually:

```bash
dotnet test dotnet/BaseFaq.Faq.Portal.Test.IntegrationTests/BaseFaq.Faq.Portal.Test.IntegrationTests.csproj
dotnet test dotnet/BaseFaq.Faq.Public.Test.IntegrationTests/BaseFaq.Faq.Public.Test.IntegrationTests.csproj
dotnet test dotnet/BaseFaq.Tenant.BackOffice.Test.IntegrationTests/BaseFaq.Tenant.BackOffice.Test.IntegrationTests.csproj
dotnet test dotnet/BaseFaq.Tenant.Portal.Test.IntegrationTests/BaseFaq.Tenant.Portal.Test.IntegrationTests.csproj
dotnet test dotnet/BaseFaq.AI.Test.IntegrationTest/BaseFaq.AI.Test.IntegrationTest.csproj
```

Optional OpenAI-backed integration coverage:

```bash
export BASEFAQ_RUN_OPENAI_INTEGRATION_TESTS=true
export OPENAI_API_KEY=<your-openai-api-key>

dotnet test dotnet/BaseFaq.AI.Test.IntegrationTest/BaseFaq.AI.Test.IntegrationTest.csproj \
  --filter FullyQualifiedName~OpenAiGenerationMatchingFlowTests
```

## Current priorities

### Must stay strong

- tenant-aware DB behavior
- CRUD and business-rule flows in the FAQ and tenant domains
- migration application from a clean database

### Should be expanded next

- API-level auth coverage across protected endpoints
- queue-driven AI callback scenarios
- Redis-backed tenant access behavior
- failure-path testing for provider or infrastructure outages

## Practical rules for new integration tests

- use real infrastructure unless the dependency is intentionally outside the test boundary
- keep assertions tied to business behavior, not only implementation details
- cover both happy paths and security or tenancy negative paths
- when a production dependency becomes mandatory, update the tests instead of weakening production code

## Related documents

- [`../backend/dotnet-backend-overview.md`](../backend/dotnet-backend-overview.md)
- [`../architecture/basefaq-ai-generation-matching-architecture.md`](../architecture/basefaq-ai-generation-matching-architecture.md)
- [`../standards/solution-cqrs-write-rules.md`](../standards/solution-cqrs-write-rules.md)
- Preconditions: endpoint metadata with `SkipTenantAccessValidationAttribute`.
- Steps: call annotated and non-annotated endpoints.
- Expected: annotated passes, non-annotated fails.
- Assert: behavior split is explicit and stable.
- Log/trace: endpoint metadata path visible.

### B) Core CRUD flows
6. Create FAQ persists with tenant id from session.
- Preconditions: empty tenant DB for FAQ.
- Steps: execute create command.
- Expected: entity created.
- Assert: `Faq.TenantId` equals session tenant id.
- Log/trace: command correlation id and entity id.

7. Update FAQ on missing id returns 404 error contract.
- Preconditions: no such FAQ.
- Steps: update command.
- Expected: `ApiErrorException`/404 mapping.
- Assert: no new row inserted.
- Log/trace: error code and request id.

8. Delete FAQ soft-deletes and hidden by default filters.
- Preconditions: existing FAQ.
- Steps: delete command then query with filters on/off.
- Expected: hidden by default, visible when filter disabled.
- Assert: `IsDeleted=true`.
- Log/trace: delete action logged once.

9. Tenant create/update uses current tenant connection.
- Preconditions: current tenant connection exists for app.
- Steps: create/update tenant command.
- Expected: tenant connection string updated from current connection.
- Assert: slug/name/edition persisted.
- Log/trace: selected current connection id.

10. Tenant create skipped when no current connection for app.
- Preconditions: only non-current connection exists.
- Steps: create/update tenant command.
- Expected: no tenant created.
- Assert: count unchanged.
- Log/trace: explicit skip reason.

### C) Search / filter / sort / pagination
11. FAQ list sorted by `name ASC`.
- Preconditions: at least 3 FAQs.
- Steps: query with sorting.
- Expected: lexical order.
- Assert: ordered ids/names.
- Log/trace: sorting input logged.

12. FAQ list with invalid sorting field falls back to `updatedDate DESC`.
- Preconditions: two FAQs with known update order.
- Steps: query with invalid sorting.
- Expected: fallback order.
- Assert: first item is most recently updated.
- Log/trace: fallback marker.

13. FAQ list pagination returns stable slice.
- Preconditions: deterministic set.
- Steps: `skip=1`, `take=1`.
- Expected: second item only.
- Assert: `TotalCount` full, `Items.Count=1`.
- Log/trace: pagination params.

14. FAQ item search matches `Question` term.
- Preconditions: mixed questions.
- Steps: search keyword.
- Expected: only matching items.
- Assert: total and returned content.
- Log/trace: normalized search term.

15. FAQ item search matches `Answer` and `AdditionalInfo` terms.
- Preconditions: target terms in body fields only.
- Steps: search.
- Expected: item found though question doesn’t match.
- Assert: returned ids match expected.
- Log/trace: query shape.

16. FAQ item search with multi-FAQ ids groups by FAQ then strategy.
- Preconditions: two FAQs with distinct sort strategies.
- Steps: search by both ids.
- Expected: grouped ordering by FAQ then strategy rule.
- Assert: item order sequence.
- Log/trace: selected `groupByFaq=true`.

17. Pagination boundary returns empty items when skip > total.
- Preconditions: small dataset.
- Steps: large skip.
- Expected: empty page.
- Assert: `TotalCount` unchanged.
- Log/trace: skip/take.

### D) Event / queue workflows (implement as queue layer is enabled)
18. Successful create emits domain event once.
- Preconditions: event publisher + outbox enabled.
- Steps: create FAQ item.
- Expected: one outbox/message record.
- Assert: payload schema and tenant id.
- Log/trace: event id correlation.

19. Consumer idempotency on duplicate event delivery.
- Preconditions: same message delivered twice.
- Steps: process twice.
- Expected: one side effect.
- Assert: dedupe key persisted.
- Log/trace: duplicate-detected metric increments.

20. Event replay from dead-letter succeeds without duplication.
- Preconditions: failed event in DLQ and fixed dependency.
- Steps: replay.
- Expected: processing succeeds.
- Assert: final state exactly-once effect.
- Log/trace: replay marker.

### E) External provider flows (mock vs sandbox)
21. Email provider timeout triggers retry policy and graceful error.
- Preconditions: provider stub timeout.
- Steps: send notification.
- Expected: bounded retries then controlled failure.
- Assert: no duplicate business write.
- Log/trace: retry count + timeout tags.

22. Provider 429 obeys backoff and does not hammer endpoint.
- Preconditions: provider returns 429.
- Steps: trigger provider call.
- Expected: exponential/backoff behavior.
- Assert: attempt count within policy.
- Log/trace: rate-limit metric.

23. Sandbox provider success path contract is valid.
- Preconditions: sandbox credentials in pre-release only.
- Steps: invoke real sandbox.
- Expected: accepted response contract.
- Assert: mapped DTO fields and stored external id.
- Log/trace: external request id captured.

### F) Multi-tenant isolation
24. Tenant A cannot read Tenant B tag/content/faq item/vote.
- Preconditions: shared DB, two tenant contexts.
- Steps: create in A, query from B.
- Expected: null/not found.
- Assert: no cross-tenant visibility.
- Log/trace: tenant ids in logs.

25. Tenant B cannot update Tenant A entity via id guessing.
- Preconditions: entity in tenant A.
- Steps: update from tenant B context.
- Expected: not found/forbidden.
- Assert: row unchanged.
- Log/trace: denied mutation attempt.

26. Soft delete remains tenant-scoped.
- Preconditions: same logical record type in multiple tenants.
- Steps: delete in A.
- Expected: B records unaffected.
- Assert: B record visible/active.
- Log/trace: tenant-scoped delete log.

### G) Failures and retries
27. Vote creation for missing FAQ item returns 404 and no vote row.
- Preconditions: no item with id.
- Steps: create vote.
- Expected: 404.
- Assert: vote count unchanged.
- Log/trace: error classification `not_found`.

28. Vote duplicate by same user fingerprint is idempotent.
- Preconditions: same IP + UA + faqItem.
- Steps: submit twice.
- Expected: same vote id returned.
- Assert: one row only; score increments once.
- Log/trace: idempotent-hit signal.

29. Vote by different users on same item creates separate rows.
- Preconditions: two distinct fingerprints.
- Steps: submit both.
- Expected: two rows.
- Assert: score reflects both votes.
- Log/trace: distinct identity hashes.

30. DB transient failure during save does not leave half-updated aggregate.
- Preconditions: injected failure between vote insert and score update.
- Steps: execute command under fault injection.
- Expected: transaction consistency.
- Assert: either both persisted or none.
- Log/trace: retry/rollback details.

## 6) Tooling and Automation
### Recommended libraries/patterns
- Test framework: xUnit (already used).
- Assertions: FluentAssertions (optional for readable object graph assertions).
- Test data: factory + builder pattern (already present).
- API integration: `Microsoft.AspNetCore.Mvc.Testing` + `WebApplicationFactory`.
- Contracts: PactNet (or equivalent) for consumer/provider checks.
- Resilience testing: policy test harness around retry/circuit behavior.
- Optional container orchestration in tests: Testcontainers for deterministic service startup.

### Local run
- Start base services: `./docker-base.sh`
- Run a project: `dotnet test dotnet/BaseFaq.Faq.Public.Test.IntegrationTests/BaseFaq.Faq.Public.Test.IntegrationTests.csproj`
- Run all integration projects:
  - `dotnet test dotnet/BaseFaq.Faq.Portal.Test.IntegrationTests/BaseFaq.Faq.Portal.Test.IntegrationTests.csproj`
  - `dotnet test dotnet/BaseFaq.Faq.Public.Test.IntegrationTests/BaseFaq.Faq.Public.Test.IntegrationTests.csproj`
  - `dotnet test dotnet/BaseFaq.Tenant.BackOffice.Test.IntegrationTests/BaseFaq.Tenant.BackOffice.Test.IntegrationTests.csproj`
  - `dotnet test dotnet/BaseFaq.Tenant.Portal.Test.IntegrationTests/BaseFaq.Tenant.Portal.Test.IntegrationTests.csproj`
  - `dotnet test dotnet/BaseFaq.AI.Test.IntegrationTest/BaseFaq.AI.Test.IntegrationTest.csproj`
- Run OpenAI live generation+matching flow (opt-in only):
  - `BASEFAQ_RUN_OPENAI_INTEGRATION_TESTS=true OPENAI_API_KEY=<key> dotnet test dotnet/BaseFaq.AI.Test.IntegrationTest/BaseFaq.AI.Test.IntegrationTest.csproj --filter FullyQualifiedName~OpenAiGenerationMatchingFlowTests`
  - You can also configure this flow in `dotnet/BaseFaq.AI.Test.IntegrationTest/appsettings.json` or `dotnet/BaseFaq.AI.Test.IntegrationTest/appsettings.Development.json` using:
  - `OpenAiIntegrationTest:Enabled`, `OpenAiIntegrationTest:ApiKey`, `OpenAiIntegrationTest:GenerationModel`, `OpenAiIntegrationTest:MatchingModel`.
  - Optional overrides: `BASEFAQ_OPENAI_GENERATION_MODEL` and `BASEFAQ_OPENAI_MATCHING_MODEL`.
  - Keep this flow out of default CI because it depends on external provider/network and real credentials.

### CI workflow
- Stage 1: restore/build/lint.
- Stage 2: integration tests in parallel by project.
- Stage 3 (nightly only): migration drift + resilience suite.
- Publish:
  - trx/junit reports,
  - flaky test dashboard,
  - trend of duration and failure categories.

### Flaky test management
- Define flake as non-deterministic pass/fail over 10 reruns.
- Auto-tag flaky tests and quarantine to nightly until fixed.
- Require root-cause issue for any quarantined test.
- Track top flake causes: timing, shared state, environment instability.

## 7) Exit Criteria and Quality Gates
### Pass/fail criteria
- 0 failed tests in required CI integration suite.
- No severity-1 flaky tests in blocking set.
- Migration apply from empty DB succeeds for release candidate.

### Coverage expectations (integration-level)
- 100% coverage of critical user/data isolation flows.
- >= 90% coverage of high-risk matrix P0/P1 scenarios.
- At least one negative-path test per critical command/query.

### Performance/stability thresholds
- CI integration pack completes within target SLA (for example <= 12 min).
- Flake rate <= 1% over rolling 14 days.
- No single test > 30s unless explicitly marked stress/nightly.

### Release readiness checklist
- [ ] All blocking integration tests pass in CI.
- [ ] Nightly resilience suite green for last 3 runs.
- [ ] Migration + rollback/drift checks completed.
- [ ] Auth and tenant-isolation negative tests green.
- [ ] Observability assertions (logs/traces/metrics) green for critical flows.
- [ ] External provider sandbox checks green or approved exception.
- [ ] No untriaged flaky failures.

## 8) Recommended Immediate Additions (next PRs)
- Add API-level integration suite with `WebApplicationFactory` for auth + middleware assertions.
- Add Redis-backed `IAllowedTenantStore` integration tests using containerized Redis.
- Add migration drift check job in nightly.
- Add resilience tests for transient DB failures and retry policy behavior.
