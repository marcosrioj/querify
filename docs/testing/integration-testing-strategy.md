# Integration Testing Strategy (BaseFaq)

## Document purpose
Provide a business-aligned and technically rigorous integration testing strategy for BaseFAQ, including risk prioritization, execution tiers, and concrete implementation targets.

## Intended audience
- Engineers implementing integration suites
- Technical leads and architects approving quality gates
- QA and release stakeholders validating production readiness

## Business outcomes
- Prevent tenant data leaks and authorization regressions before release.
- Reduce release risk through deterministic CI coverage and staged resilience testing.
- Improve incident response quality through observability-oriented assertions.

## Technical outcomes
- Use real PostgreSQL and EF migrations as baseline integration infrastructure.
- Validate middleware/session/auth behavior across realistic boundaries.
- Enforce repeatable CI/nightly/pre-release execution tiers.

## Reading order
1. Scope and assumptions.
2. Environment and data strategy.
3. Advanced techniques and resilience patterns.
4. Risk matrix and concrete test catalog.
5. Tooling, gates, and immediate backlog.

## Concise Summary
- Integration tests in this solution should validate real behavior across module boundaries with real PostgreSQL, real EF migrations, and production-like middleware/session rules.
- Current suites are strongest on command/query + DB persistence and tenant/soft-delete filters; biggest gaps are API auth flows, tenant-resolution middleware behavior, observability assertions, resilience/fault-injection, and third-party integration boundaries.
- Recommended execution tiers:
  - CI PR: deterministic, fast DB-backed integration pack.
  - Nightly: stress/concurrency/resilience and migration-drift checks.
  - Pre-release: full regression including auth and sandbox-provider checks.
- This plan defines environment/data strategy, risk matrix, and 30 concrete test cases with assertions/logging expectations.

## 1) Scope, Architecture Assumptions, and Boundaries
### Assumptions (explicit)
- Platform is multi-service but codebase currently exposes 4 APIs:
  - `BaseFaq.Faq.Portal.Api`
  - `BaseFaq.Faq.Public.Api`
  - `BaseFaq.Tenant.BackOffice.Api`
  - `BaseFaq.Tenant.Portal.Api`
- Persistence is PostgreSQL via EF Core with tenant-aware filters and soft delete.
- Redis is used for allowed-tenant caching (`RedisAllowedTenantStore`).
- RabbitMQ/SMTP are available in docker base services, but queue/worker execution paths are currently limited or not yet covered by existing integration suites.
- Existing integration tests are mostly handler + DbContext level with real DBs and migrations.

### What “integration test” means here
- Integration test:
  - Verifies interactions between at least two components (e.g., handler + EF + DB constraints, middleware + claims + cache, API endpoint + auth + DB).
  - Uses real infrastructure components where practical (PostgreSQL always, Redis optionally, provider sandbox where needed).
- Not unit test:
  - Does not isolate with pure mocks of repositories/DbContexts.
- Not contract-only test:
  - Contract tests validate API/provider schema compatibility; they complement but do not replace DB/middleware behavior tests.
- Not full E2E:
  - E2E covers browser/client journeys across deployed services; integration tests stay at API/service boundary with controlled fixtures.

### Integration points to cover
- REST APIs (controllers + middleware + auth).
- DB integration (migrations, constraints, tenant filters, soft delete, sorting/pagination queries).
- Cache (Redis allowed-tenant store).
- Queues/event bus (when worker/event flows are implemented or enabled).
- File storage (if added later; currently treat as planned scope).
- Auth/identity (JWT validation, claims mapping, tenant authorization).
- External providers:
  - Auth0/OIDC metadata and token assumptions.
  - SMTP/email/analytics/webhooks where applicable.
- Background jobs/migrations runner flows.

### Execution tiers
- CI (per PR, blocking):
  - DB-backed deterministic integration tests.
  - No long sleeps, no external network dependency.
  - Includes migrations apply + core CRUD + tenant isolation + critical auth happy/negative paths.
- Nightly:
  - Concurrency/race tests, retry/idempotency, migration drift, resilience/fault injection.
  - Optional Redis and queue containerized runs.
- Pre-release:
  - Full integration pack + provider sandbox checks + backward compatibility + observability assertions.

## 2) Environment and Data Strategy
### Ephemeral environments
- Local and CI should run isolated ephemeral infrastructure:
  - Preferred: docker compose project per pipeline run (`bf_baseservices_<runId>`).
  - DB names per test run already follow random naming (`bf_faq_test_<guid>`), keep this pattern.
  - For API-level integration tests, run each API in test profile with dedicated test namespace/config.
- Add standardized env presets:
  - `INTEGRATION_TEST=true`
  - `ASPNETCORE_ENVIRONMENT=IntegrationTest`
  - test-specific auth issuer/audience and Redis DB index.

### Deterministic data
- Use builders/factories (`TestDataFactory`) only; avoid ad hoc inline entities unless test intent requires edge fields.
- Seed baselines by scenario, not global mega-seed.
- Reset strategy:
  - Current per-test database creation/drop is good isolation.
  - For API-level suites with shared DB, use transaction rollback or schema recreate per class.
- Time control:
  - Introduce injectable clock abstraction for time-sensitive sorting/retry tests.
  - If not available, explicitly update timestamps in DB setup to remove race with system clock.

### PII and privacy-safe data
- Use synthetic addresses/domains (`example.test`, RFC5737 IP ranges) only.
- For any copied prod incidents, sanitize through deterministic tokenization.
- Block real tokens/secrets from fixtures and snapshots.

### Parallelization and isolation
- Keep test-level random DB names; this supports parallel test workers safely.
- Ensure Redis keys include suite prefix + test id when Redis integration tests are added.
- Disable flaky order dependencies by never relying on prior test side effects.

## 3) Advanced Integration Testing Techniques
### Consumer-driven contracts (Pact-style)
- Use contract tests for:
  - API consumers of each service and external provider adapters.
  - Versioned DTO fields and backward compatibility.
- Workflow:
  - Consumer generates pact from typed client tests.
  - Provider verifies pact in CI before merge.
  - Add broker/tag strategy per branch/release.
- Do not replace DB integration tests with pact tests; they serve different risk classes.

### DB integration patterns
- Migration validation:
  - Apply latest migrations from empty DB in CI.
  - Validate migration rollback on nightly (where feasible).
- Schema drift checks:
  - Compare EF model snapshot vs generated SQL hash in nightly.
- Transaction boundary tests:
  - Verify partial failures do not persist half-written aggregates.
- Idempotency checks:
  - Repeated command execution under same key/user should not duplicate records (already partially covered in voting).

### Event-driven testing
- When event publishing exists:
  - Assert event emitted exactly once per successful transaction.
  - Verify ordering constraints (if required) and idempotent consumers.
  - Test at-least-once behavior by replaying duplicate messages.
  - Add replay tests from dead-letter payloads.

### Resilience/fault injection
- Inject:
  - DB timeout / transient network fault.
  - Redis unavailable.
  - provider 429 and 5xx.
- Assert:
  - retries follow policy,
  - no duplicate writes,
  - circuit breaker open/half-open behavior,
  - graceful error mapping via API error middleware.

### Security integration
- Validate:
  - JWT token validation (issuer/audience/signature/expiry).
  - role/claim enforcement per endpoint.
  - tenant claim vs requested tenant mismatch handling.
  - skip-tenant-validation attribute only where explicitly allowed.

### Observability validation
- For critical flows assert:
  - structured logs include request id and tenant id.
  - traces propagate correlation id across middleware/handlers.
  - key counters/histograms emitted (errors, latency, retries).

## 4) High-Risk Matrix (Impact x Probability)
| Integration Point | Impact | Probability | Priority | Notes |
|---|---|---:|---:|---|
| Auth token + claims mapping | High | Medium | P0 | Breaks all protected APIs silently if misconfigured |
| Tenant isolation filters | High | Medium | P0 | Data leak risk across tenants |
| Vote idempotency/concurrency | Medium | Medium | P1 | Double counting possible under race |
| Migrations/model drift | High | Medium | P0 | Release-blocking runtime failures |
| Sorting/pagination correctness | Medium | High | P1 | User-visible correctness regressions |
| Redis allowed-tenant cache | High | Low-Med | P1 | Wrong cache data can bypass/deny access |
| External provider outages/rate limits | Medium | Medium | P1 | Degraded UX; retry storms if unmanaged |
| Queue/event delivery semantics | High | Low (current) | P2 | Becomes P0 once workers are active |
| Observability pipeline | Medium | Medium | P2 | Slows incident response and root cause |

### Edge-case coverage matrix
- Concurrency: repeated create/update/vote from same and different identities.
- Eventual consistency: if async jobs/events introduced, poll-with-timeout assertions.
- Timezone/clock skew: UTC storage and deterministic sort by timestamps.
- Large payloads: max text lengths for FAQ answers and references.
- Pagination boundaries: empty page, last page, large skip.
- Filtering/sorting: unknown sort field fallback, multi-field sort.
- Versioning/backward compatibility: old client DTOs against new provider.

## 5) Concrete Integration Test Cases (30)

### A) Authentication / Authorization
1. Protected endpoint without JWT returns 401.
- Preconditions: API host with auth enabled.
- Steps: call protected endpoint without `Authorization`.
- Expected: 401.
- Assert: no DB side effects.
- Log/trace: auth failure reason and correlation id present.

2. JWT with invalid issuer returns 401.
- Preconditions: signed token with wrong `iss`.
- Steps: call protected endpoint.
- Expected: 401.
- Assert: no command handler invocation side effects.
- Log/trace: token validation failure includes issuer mismatch.

3. JWT with missing role claim returns 403 for admin route.
- Preconditions: valid token lacking required role.
- Steps: call admin endpoint.
- Expected: 403.
- Assert: DB unchanged.
- Log/trace: authorization policy failure recorded.

4. Tenant mismatch in header/context rejected.
- Preconditions: token/user allowed tenant A only.
- Steps: request tenant B context.
- Expected: 403/validation error.
- Assert: no tenant B data exposed.
- Log/trace: denied tenant id logged.

5. SkipTenantAccessValidation endpoint bypasses tenant gate only for annotated endpoint.
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
