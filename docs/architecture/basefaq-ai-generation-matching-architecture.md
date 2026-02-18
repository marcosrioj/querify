# BaseFAQ AI Architecture Proposal

## Document purpose
Define the target architecture, integration model, delivery phases, and operational controls for `BaseFaq.AI.Generation` and `BaseFaq.AI.Matching`.

## Intended audience
- Engineers implementing AI services and integration flows
- Architects reviewing service boundaries and persistence ownership
- Platform/operations teams running messaging, observability, and secret-management controls

## Business outcomes
- Deliver asynchronous AI generation and matching with reliable callback completion semantics.
- Preserve existing BaseFAQ boundaries while expanding AI capability.
- Minimize delivery risk through phased rollout and explicit operational safeguards.

## Technical outcomes
- Keep `bf_ai_db` as the AI lifecycle source of truth.
- Maintain FAQ domain ownership in FAQ DB with explicit integration-write paths.
- Ensure reliability through idempotency, retry, DLQ, tracing, and monitoring patterns.

## How to read this document
1. Use the Executive Summary and Recommended Architecture for direction.
2. Use Event Flow and Asynchronous Integration sections for implementation design.
3. Use Idempotency/Retry/Tracing/Security sections for production hardening.
4. Use Implementation Plan, Backlog, and Checklist sections for delivery governance.

## Executive Summary
This document defines how `BaseFaq.AI.Generation` and `BaseFaq.AI.Matching` should be added to BaseFAQ with minimal incremental changes while respecting the new AI service boundaries.

The proposal preserves the existing architecture model where possible:
- Existing API hosts remain composition roots.
- Existing `Business` modules continue using MediatR orchestration.
- Existing infrastructure conventions (auth, Sentry, Redis, RabbitMQ baseline) remain in place.

The proposal also applies the new constraints:
- `BaseFaq.AI` uses a dedicated database: `bf_ai_db`.
- `BaseFaq.AI` uses a dedicated persistence context (for example `AiDbContext`).
- `BaseFaq.AI` has no tenant model.
- No FK relationship exists between AI DB and FAQ DB.
- `BaseFaq.Faq` APIs communicate with `BaseFaq.AI` asynchronously via RabbitMQ.
- `BaseFaq.AI` processes requests, updates FAQ data through integration write flow, and publishes callback events so FAQ APIs know processing is done and ready.

The AI capability remains organized under:
- `BaseFaq.AI/Generation`
- `BaseFaq.AI/Matching`
- `BaseFaq.AI/Common`

## 1) Recommended Architecture (Aligned to Existing BaseFAQ Model)
### Architectural assumptions
- No active production-grade queue consumers are currently implemented.
- AI generation and matching are delivered as AI services integrated with FAQ APIs through RabbitMQ.
- Existing folder conventions (`Api`, `Business`, `Test`) should be preserved.
- `BaseFaq.AI` does not use tenant resolution or tenant partitioning.

### Existing model to preserve
- API startup and middleware pattern in API host projects.
- Feature composition via extension methods in `Api/Extensions`.
- Application orchestration in Business projects with MediatR handlers.
- FAQ persistence ownership remains in `BaseFaq.Faq.Common.Persistence.FaqDb` (`FaqDbContext`) for FAQ domain data.
- Shared infrastructure utilities from `BaseFaq.Common.Infrastructure.*`.

### Target high-level model
- `Generation (ContentRef -> New FaqItems)`: asynchronous workflow driven by RabbitMQ events and worker consumers.
- `Matching (New FaqItem -> Similar Existing FaqItems)`: separate workflow for duplicate/similarity checks.
- `Common`: provider and vector abstractions shared by both modules.
- `Persistence split`:
  - AI lifecycle and operational state in `bf_ai_db` via AI context.
  - FAQ product data in FAQ DB via `FaqDbContext`.

## 2) Current Solution Style + Incremental Additions
### Existing BaseFAQ components (unchanged)
- `BaseFaq.Faq.Portal.Api` and `BaseFaq.Faq.Public.Api` startup conventions.
- Existing middleware: auth, API error handling, Sentry.
- Existing `BaseFaq.Faq.Portal.Business.*` and `BaseFaq.Faq.Public.Business.*` patterns.
- Existing FAQ entity model and DB context ownership in `BaseFaq.Faq.Common.Persistence.FaqDb`.

### Existing components with additive changes only
- `BaseFaq.Faq.Portal.Api`:
  - Register AI generation feature in existing `AddFeatures(...)`.
  - Add generation endpoints through new Business module controllers.
- `BaseFaq.Faq.Public.Api`:
  - Register AI matching feature in existing `AddFeatures(...)`.
  - Add asynchronous matching endpoints through new Business module controllers.
- `BaseFaq.Faq.Common.Persistence.FaqDb`:
  - Keep FAQ domain ownership.
  - Store only FAQ-side final data/state needed for product usage.
  - Do not store AI lifecycle state that belongs to `bf_ai_db`.

### New components (added)
- New AI projects under `BaseFaq.AI` only (no restructuring of existing modules).
- New worker processes for asynchronous generation, asynchronous matching, and optional embedding refresh.
- New contracts shared for AI events and provider abstractions.
- New AI persistence layer with dedicated context and migrations for `bf_ai_db`.

## 3) BaseFaq.AI.Generation and BaseFaq.AI.Matching Project Divisions
| Division | Responsibility | Recommended .NET technologies/libraries | Applicable patterns | Risks | Mitigations |
|---|---|---|---|---|---|
| API integration endpoints | Expose commands/queries in existing API model (`Portal` for generation, `Public` for matching) | ASP.NET Core controllers, existing auth middleware | Thin controller + application service | API bloat | Keep strict route namespace `api/faqs/ai/*` |
| Application orchestration layer | Validate input, coordinate workflows, dispatch commands/queries | MediatR, FluentValidation (optional) | CQRS, orchestration service | Business logic leakage into controllers | Keep orchestration in handlers/services only |
| AI processing worker/service | Consume generation jobs, call LLM provider, persist outputs in `bf_ai_db`, update FAQ data, emit ready/failure callbacks | Worker Service, RabbitMQ consumer, Polly, HttpClientFactory | Async consumer, retry/circuit-breaker | Duplicate side effects | Idempotency keys + dedupe store + exactly-once business semantics |
| Domain rules for FAQ generation lifecycle | Enforce status transitions and review gates | Domain services + enums + guard methods | State machine | Invalid transitions | Transition guard + integration tests |
| Persistence and FAQ versioning | Split ownership: AI state in `bf_ai_db`; final FAQ versions in FAQ DB | EF Core, Npgsql, existing migration flow | Explicit integration write flow + eventual consistency | Cross-DB consistency drift | Correlation IDs, retries, compensating status, callback confirmation |
| Messaging/events integration | Decouple request/processing/completion and status notifications | RabbitMQ (MassTransit optional abstraction) | Event-driven architecture, outbox/inbox | Duplicate delivery | Idempotency keys + processed-message store |
| Security and secret management | Secure provider keys and prevent secret leakage | .NET config providers, User Secrets (dev), cloud secret manager | Secret abstraction + rotation | Secret exposure | Vault-only in non-dev + redaction filters |
| Observability and operations | Visibility across APIs, workers, broker, DB, provider calls | Sentry (existing), OpenTelemetry, HealthChecks, structured logs | Correlation tracing + SLO monitoring | Blind failure modes | End-to-end tracing + alerting thresholds |
| Prompt governance and answer quality controls | Prompt versioning, policy checks, publication gates | Prompt registry (JSON/YAML + DB ref), evaluation runner (optional) | Prompt-as-code + human-in-the-loop | Hallucinations/quality drift | Quality rubric + approval workflow + fallback |

## 4) Event Flow (Step-by-Step)
### Process A: Generation (ContentRef -> New FaqItems)
1. Client calls `POST /api/faqs/ai/generation-jobs` (Portal API).
2. FAQ API validates request/user context and stores FAQ-side request metadata as `Requested`.
3. FAQ API publishes `FaqGenerationRequestedV1` with correlation and idempotency metadata to RabbitMQ.
4. AI Generation worker consumes request event.
5. Worker creates/updates AI job in `bf_ai_db` and sets status `Processing`.
6. Worker loads prompt profile and source context.
7. Worker calls provider (`OpenAI`/`Azure OpenAI`) with retries and timeout policy.
8. Worker applies quality checks and business rules.
9. Worker persists AI artifacts and lifecycle details in `bf_ai_db`.
10. Worker updates FAQ data in FAQ DB through integration write flow (`FaqDbContext`) with final approved payload.
11. Worker sets final AI job status:
   - `AwaitingReview` (if review gate applies), or
   - `Completed` / `Published`, or
   - `Failed`.
12. Worker publishes callback event (`FaqGenerationReadyV1` or `FaqGenerationFailedV1`) to RabbitMQ.
13. FAQ API-side consumer receives callback and marks request as done/failed and ready to use.
14. The cycle repeats for subsequent generation requests.

### Process B: Matching (New FaqItem -> Similar Existing FaqItems)
1. Client/API sends matching request with `FaqItemId` for a newly created FAQ item.
2. Matching API validates the request and tenant access to that `FaqItemId`.
3. Matching service enqueues/starts matching workflow and returns `202 Accepted`.
4. Matching worker/process compares the new FAQ item against existing FAQ items.
5. Matching result is stored/published for downstream use (for example review or merge decisions).

## 5) Asynchronous Integration
### Use asynchronous when
- Work is compute-heavy, long-running, expensive, or failure-prone.
- Example: FAQ generation, matching, re-generation, bulk embedding refresh.
- API should return `202 Accepted` with job identifier and polling/subscription mechanism.

### Rule of thumb
- `Generation (ContentRef -> New FaqItems)`: async by default.
- `Matching (New FaqItem -> Similar Existing FaqItems)`: separate async job flow.

## 6) RabbitMQ and MassTransit Evaluation
### Scenario A: Use both together (recommended if already standardized)
- Use RabbitMQ as transport broker.
- Use MassTransit as .NET messaging abstraction/runtime.

Pros:
- Faster delivery for consumers/producers.
- Built-in middleware for retry, delayed retry, error queues, topology.
- Better developer ergonomics and observability hooks.

Cons:
- Additional abstraction layer to operate and understand.

### Scenario B: Use only RabbitMQ client library
Pros:
- Lower abstraction overhead.
- Full control of broker semantics.

Cons:
- More boilerplate for retries, correlation, poison handling, and instrumentation.
- Higher maintenance and inconsistency risk across services.

### Scenario C: Use only MassTransit (without RabbitMQ)
- Not aligned because RabbitMQ is the required communication backbone in this architecture.
- Would require selecting another transport and operating model.

### Trade-offs
| Option | Complexity | Resilience | Observability | Operational cost |
|---|---|---|---|---|
| RabbitMQ + MassTransit | Medium | High | High | Medium |
| RabbitMQ only | Medium-High (app code) | Medium | Medium-Low | Medium |
| Alternative transport with MassTransit only | Medium-High (migration) | Medium-High | Medium-High | Medium-High |

Recommended for BaseFAQ:
- Keep RabbitMQ as broker and service communication channel.
- Use MassTransit where it accelerates delivery and reliability in .NET services.

## 7) Idempotency, Retry, DLQ, Tracing, Monitoring
### Idempotency strategy
- Require `Idempotency-Key` on generation command API.
- Persist unique key at job creation (`FaqId + IdempotencyKey` unique index, or `JobId` uniqueness).
- Consumer dedupe:
  - Store processed `MessageId` + handler name.
  - Skip if already processed.

### Retry policy
- Transport/consumer-level retries for transient infrastructure issues.
- Provider-level retries with exponential backoff + jitter (429/5xx/timeouts).
- FAQ DB integration write retries with safe idempotent upsert semantics.
- No retries for validation/domain errors.

### Dead-letter queue strategy
- Route poison messages to `_error` queues.
- Add AI-specific quarantine queue for messages exceeding retry policy.
- Operational runbook:
  - Inspect cause.
  - Patch/redeploy.
  - Replay safe messages with dedupe check.

### Distributed tracing
- Propagate `traceparent`, `CorrelationId`, `CausationId` through API -> broker -> worker -> AI DB/FAQ DB/provider.
- Add spans for:
  - API command handling.
  - Event publish/consume.
  - Provider API call.
  - AI DB persistence.
  - FAQ DB integration write.
- Implementation convention:
  - Shared baseline telemetry wiring in `BaseFaq.Common.Infrasctructure.Telemetry` via `AddTelemetry(...)`.
  - Keep the shared telemetry project generic (no service-specific sources hardcoded).
  - Pass service-specific `ActivitySource` names from each API/worker when registering telemetry.

### Monitoring and alerting
- Metrics:
  - Job throughput and completion rate.
  - Failure rate by error code.
  - Queue depth and message age.
  - p95 and p99 generation duration.
  - Matching latency.
  - FAQ update latency after AI completion.
- Alerts:
  - Queue lag threshold exceeded.
  - Failure ratio above baseline.
  - DLQ growth.
  - Provider error spikes / rate limiting spikes.
  - Repeated callback publish failures.

## 8) OpenAI API Key Security Strategy
### Secret manager
- Development and production: keep provider keys in a managed secret manager
  (Azure Key Vault / AWS Secrets Manager / GCP Secret Manager) and apply them via tenant management flows.
- Never store provider keys in repository `appsettings*.json`.
- Implemented at tenant level via `TenantAiProviders` (`AiProviderId` + encrypted `AiProviderKey`).

### Key rotation
- Rotate by updating tenant provider credentials per command (`Generation` / `Matching`).
- Rotate regularly (time-based) and on incident trigger.
- Runbook: `docs/operations/secret-manager-key-rotation.md`.

### Log masking and no leakage
- Redact:
  - `Authorization` headers.
  - API keys and tokens.
  - Prompt fragments containing sensitive content.
- Disable verbose provider request/response logs in production.
- Add logging guard middleware/sinks with explicit masking rules.

## 9) Implementation Plan by Phase
### Phase 1: MVP
- Create AI project skeleton under `BaseFaq.AI`.
- Implement generation command + async worker processing.
- Implement generation status endpoint.
- Add minimum AI persistence model in `bf_ai_db` for job + generated artifacts.
- Implement FAQ integration write flow (`BaseFaq.AI` -> FAQ DB write path) + callback event.
- Implement matching endpoint with asynchronous response (job-based), no fallback behavior.
- Add integration tests for happy-path generation and matching.

### Phase 2: Production Hardening
- Add idempotency enforcement and consumer dedupe store.
- Configure robust retry and DLQ policies.
- Add full tracing and alerting.
- Introduce prompt governance and quality gates.
- Enforce secret manager in non-dev environments.
- Add reconciliation job for AI done but FAQ callback not acknowledged.

### Phase 3: Scale
- Separate workers by workload type (generation vs embedding/index refresh).
- Introduce batching and adaptive concurrency controls.
- Add cost controls and provider routing strategy.
- Improve relevance quality via hybrid retrieval and re-ranking.

## 10) Practical Artifacts
### Event contract examples
```csharp
public record FaqGenerationRequestedV1(
    Guid EventId,
    Guid CorrelationId,
    Guid JobId,
    Guid RequestedByUserId,
    string IdempotencyKey,
    Guid FaqId,
    string Language,
    string PromptProfile,
    DateTime OccurredUtc);

public record FaqGenerationReadyV1(
    Guid EventId,
    Guid CorrelationId,
    Guid JobId,
    Guid FaqVersionId,
    bool RequiresHumanReview,
    DateTime OccurredUtc);

public record FaqGenerationFailedV1(
    Guid EventId,
    Guid CorrelationId,
    Guid JobId,
    string ErrorCode,
    string ErrorMessage,
    DateTime OccurredUtc);
```

### Solution/project folder structure
```text
dotnet
  /BaseFaq.AI.Api
  /BaseFaq.AI.Business.Generation
  /BaseFaq.AI.Business.Matching
  /BaseFaq.AI.Test.IntegrationTest
  /BaseFaq.AI.Common.VectorStore
  /BaseFaq.AI.Common.Contracts
```

Suggested concrete project names:
```text
dotnet/BaseFaq.AI.Api/BaseFaq.AI.Api.csproj
dotnet/BaseFaq.AI.Business.Generation/BaseFaq.AI.Business.Generation.csproj
dotnet/BaseFaq.AI.Business.Matching/BaseFaq.AI.Business.Matching.csproj
dotnet/BaseFaq.AI.Test.IntegrationTest/BaseFaq.AI.Test.IntegrationTest.csproj

dotnet/BaseFaq.AI.Common.VectorStore/BaseFaq.AI.Common.VectorStore.csproj
dotnet/BaseFaq.AI.Common.Contracts/BaseFaq.AI.Common.Contracts.csproj
```

## 11) Main Risks and Mitigations
| Risk | Impact | Mitigation |
|---|---|---|
| Cross-database update inconsistency (`bf_ai_db` success, FAQ DB write failure) | Callback may not represent real final state | Retry with idempotent upsert, reconciliation worker, failure callback if threshold exceeded |
| Duplicate event delivery | Duplicate generation/persistence | Idempotency keys + processed-message table + unique constraints |
| LLM quality drift | Low trust in generated FAQs | Prompt versioning + quality checks + human approval gate |
| Provider outages/rate limits | Latency and failures | Retry/backoff, circuit breaker, fallback model strategy |
| Secret leakage | Security incident | Secret manager, strict redaction, no secrets in source config |
| Queue backlog growth | SLA degradation | Queue depth alerts, scaling workers, backpressure controls |

## 12) Prompt Governance and Quality Gate Process
### Governance model
- Prompt definitions are managed as code in versioned files under source control.
- Every prompt change requires a pull request with:
  - owner, rationale, and expected behavior delta
  - rollback plan to previous prompt version
  - linked evaluation run (offline or staging)
- Prompts are immutable by version ID after publish; new changes create a new version.
- Runtime requests persist prompt metadata in `bf_ai_db` (`PromptProfile`, prompt version, model ID, and correlation ID) for auditability.

### Quality gate stages
1. `Draft`
- Prompt authored and locally validated for syntax/template variable coverage.
2. `Review`
- At least one engineer reviewer and one product/content reviewer approve semantics and policy alignment.
3. `Evaluation`
- Run regression dataset checks against baseline prompt/model pair.
- Minimum pass criteria:
  - no policy/safety violations
  - factuality and instruction-following score at or above baseline threshold
  - no regressions on blocked test scenarios
4. `Staging`
- Deploy behind a feature flag with limited tenant or percentage rollout.
- Validate operational SLOs (latency, failure rate, token cost budget).
5. `Publish`
- Promote prompt version to active for production traffic.
- Keep previous stable version available for immediate rollback.

### Runtime enforcement
- Generation/Matching workers resolve prompts by explicit `PromptProfile` + version mapping.
- If prompt profile/version is missing or disabled, fail fast and publish failure callback event.
- High-risk outputs are marked `RequiresHumanReview` and cannot auto-publish to FAQ DB.
- Automated quality checks run before FAQ integration writes:
  - schema/format validation
  - banned-content and policy filters
  - confidence/quality rubric threshold

### Audit and operations
- Required telemetry dimensions: `PromptProfile`, `PromptVersion`, `Model`, `Tenant`, `JobId`, `CorrelationId`.
- Dashboards track quality score drift, human-review rate, rollback events, and publish failure causes.
- Incident response runbook:
  - disable active prompt version flag
  - roll back to last known good version
  - requeue failed jobs when safe

## 13) Delivery Backlog (Tracked)
Tracking convention:
- IDs use `AI-MVP-*`, `AI-HARD-*`, and `AI-SCALE-*`.
- Status values: `Todo`, `In Progress`, `Done`.
- Source of truth: this section until external board migration is completed.

### MVP backlog
| ID | Item | Status | Target Artifact |
|---|---|---|---|
| `AI-MVP-01` | Generation request/worker happy-path integration test coverage completion | `Done` | `dotnet/BaseFaq.AI.Test.IntegrationTest` |
| `AI-MVP-02` | Matching async request/status contract verification suite | `Done` | `dotnet/BaseFaq.AI.Test.IntegrationTest` |
| `AI-MVP-03` | FAQ write idempotency validation under duplicate event delivery | `Done` | `dotnet/BaseFaq.AI.Test.IntegrationTest` |

### Hardening backlog
| ID | Item | Status | Target Artifact |
|---|---|---|---|
| `AI-HARD-01` | Retry policy and DLQ handling validation for worker consumers | `Done` | `dotnet/BaseFaq.AI.Test.IntegrationTest/Tests/Generation/Infrastructure/RetryAndDlqPolicyTests.cs` |
| `AI-HARD-02` | Tenant-level AI provider key rotation verification | `Done` | `docs/operations/secret-manager-key-rotation.md` |
| `AI-HARD-03` | Logging redaction and sensitive field masking guardrails | `Done` | `dotnet/BaseFaq.AI.Test.IntegrationTest/Tests/Generation/Infrastructure/LoggingRedactionTests.cs` |

### Scale backlog
| ID | Item | Status | Target Artifact |
|---|---|---|---|
| `AI-SCALE-01` | Worker split for generation/matching workloads and throughput baselines | `Todo` | `dotnet/BaseFaq.AI.Business.Generation`, `dotnet/BaseFaq.AI.Business.Matching` |
| `AI-SCALE-02` | Adaptive concurrency controls with queue-depth feedback | `Todo` | `dotnet/BaseFaq.AI.Business.Generation` |
| `AI-SCALE-03` | Provider routing and cost control policy implementation | `Todo` | `dotnet/BaseFaq.AI.Business.Generation`, `dotnet/BaseFaq.AI.Business.Matching` |

Review cadence:
- Weekly backlog triage in architecture review.
- Any `Todo` item moving to implementation must be linked in PR description by backlog ID.

## 14) Final Technical Checklist
- [x] `BaseFaq.AI` root folder and projects created.
- [x] `Generation` and `Matching` projects follow existing `Api/Business/Test` conventions.
- [x] Existing API hosts register new AI features without changing current boundaries.
- [x] AI lifecycle entities and migrations added to `bf_ai_db` persistence.
- [x] `AiDbContext` separated and wired independently from `FaqDbContext`.
- [x] No FK relationship exists between AI and FAQ databases.
- [x] Async generation event flow implemented end-to-end through RabbitMQ.
- [x] Callback flow implemented (`Ready`/`Failed`) and consumed by FAQ Portal API.
- [x] FAQ integration write flow from AI services to FAQ DB is idempotent and validated.
- [x] Matching endpoint implemented with asynchronous response for FAQ item retrieval, validates `FaqItemId` (required, non-empty, and existence/access in FAQ DB for current tenant), no fallback behavior required, and consumed only by FAQ Public API requests.
- [x] Idempotency key support and dedupe table in place.
- [x] Retry and DLQ policies configured and validated.
- [x] Distributed tracing across API, broker, worker, AI DB, FAQ DB, provider enabled.
- [x] Monitoring dashboard and alerts configured.
- [x] Secret manager integration and key rotation implemented for Generation process.
- [x] Secret manager integration and key rotation implemented for Matching process.
- [x] Logging redaction rules validated (no key leakage).
- [x] Prompt governance and quality gate process documented.
- [x] MVP, hardening, and scale backlog items created and tracked.
