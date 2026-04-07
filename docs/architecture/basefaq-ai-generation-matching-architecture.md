# BaseFAQ AI Generation and Matching Architecture

## Purpose

This document describes the AI runtime that currently exists in the repository, how generation and matching flow through RabbitMQ and MassTransit, and which parts are already implemented versus still incomplete.

## Current implementation summary

The repository already contains a real asynchronous AI split:

- `BaseFaq.Faq.Portal.Api` publishes generation requests.
- `BaseFaq.Faq.Public.Api` publishes matching requests when a new FAQ item is created through the public flow.
- `BaseFaq.AI.Api` hosts the consumers and provider-facing execution services.
- Generation and matching both publish completion or failure callbacks after worker execution.

The important constraint is that the AI worker is intentionally stateless for lifecycle tracking. It resolves tenant-specific provider configuration from the tenant database, uses a tenant-specific FAQ database when needed, and publishes events instead of owning a long-lived job store.

## Project map

| Project area | Role |
|---|---|
| `BaseFaq.AI.Api` | worker host and health endpoint |
| `BaseFaq.AI.Business.Common` | shared AI abstractions such as provider resolution and FAQ DbContext factory |
| `BaseFaq.AI.Business.Generation` | content study, prompt building, generation provider integration, generation consumers |
| `BaseFaq.AI.Business.Matching` | candidate loading, ranking provider integration, matching consumers |
| `BaseFaq.Models.Ai` / `BaseFaq.AI.Common.Contracts` | event contracts for generation and matching |
| `BaseFaq.AI.Test.IntegrationTest` | integration coverage for AI flows |

## Event-driven design

### Generation flow

1. `BaseFaq.Faq.Portal.Business.Faq` validates the FAQ and its content references.
2. It checks whether the tenant has a provider configured for `AiCommandType.Generation`.
3. It publishes `FaqGenerationRequestedV1` through `IPublishEndpoint`.
4. `BaseFaq.AI.Business.Generation.Consumers.FaqGenerationRequestedConsumer` receives the message.
5. `ProcessFaqGenerationRequestedCommandHandler` executes the generation service.
6. The worker publishes either `FaqGenerationReadyV1` or `FaqGenerationFailedV1`.
7. `BaseFaq.Faq.Portal.Api` currently consumes those callbacks and logs them.

### Matching flow

1. `BaseFaq.Faq.Public.Business.FaqItem` creates the FAQ item.
2. It checks whether the tenant has a provider configured for `AiCommandType.Matching`.
3. It publishes `FaqMatchingRequestedV1`.
4. `BaseFaq.AI.Business.Matching.Consumers.FaqMatchingRequestedConsumer` receives the message.
5. `ProcessFaqMatchingRequestedCommandHandler` loads the source question and candidate questions from the tenant FAQ database.
6. The worker publishes either `FaqMatchingCompletedV1` or `FaqMatchingFailedV1`.

## What the worker actually does today

### Generation

The generation worker currently:

- resolves the tenant's generation provider configuration
- loads processable content references from the tenant FAQ database
- builds prompt data
- calls the configured provider
- logs successful completion
- publishes ready or failed callback events

The generation worker does **not** currently persist generated FAQ items or generation lifecycle state back into the FAQ database.

### Matching

The matching worker currently:

- resolves the tenant's matching provider configuration
- loads the newly created FAQ item question
- loads other active FAQ item questions for the same tenant
- asks the provider to rank the candidates
- publishes matching-completed or matching-failed callback events

There is no repository-side consumer yet that stores or applies the matching result after `FaqMatchingCompletedV1` is published.

## Architectural decisions already visible in code

### Stateless AI worker host

The AI host uses:

- background messaging through MassTransit
- provider resolution from tenant configuration
- tenant-specific FAQ database access through `IFaqDbContextFactory`
- callback events instead of an internal job database

This keeps the AI runtime small and avoids mixing FAQ domain ownership into the worker host.

### Tenant-aware provider resolution

Provider credentials are not read from `appsettings.json`. Instead, the worker resolves them per tenant and per command type from tenant data. That makes AI behavior part of tenant configuration, not global host configuration.

### Shared infrastructure alignment

The AI host follows the same shared conventions as the rest of the solution where relevant:

- DI composition in the API host
- MediatR command handlers
- shared telemetry package
- shared contracts for inter-service messaging

## Operational requirements

### Required infrastructure

- RabbitMQ for message transport
- PostgreSQL tenant database for provider and tenant metadata
- PostgreSQL FAQ databases for tenant FAQ data access
- an AI provider account and credentials configured through tenant management

### Required configuration

- `Ai:UserId` in `dotnet/BaseFaq.AI.Api/appsettings.json`
- RabbitMQ configuration for generation and matching exchanges/queues
- tenant AI provider credentials stored through tenant flows

The credential handling model is documented in [`../operations/secret-manager-key-rotation.md`](../operations/secret-manager-key-rotation.md).

### Observability

`BaseFaq.AI.Api` is the one place in the solution already wired to the shared telemetry package. Jaeger and the local observability stack can therefore be used to inspect the AI worker independently of the CRUD APIs.

## Current gaps and implementation status

### Implemented

- asynchronous message publication for generation and matching
- AI worker consumers for both flows
- provider resolution per tenant and AI command type
- generation callback events
- matching callback events
- AI health endpoint
- AI integration test project

### Not yet implemented end-to-end

- persistence of generated FAQ items or draft artifacts from the generation worker
- FAQ-side job or lifecycle state tracking
- downstream consumer for matching-completed events
- operator-facing monitoring document beyond the local observability stack

## Practical guidance

- Treat the AI system as an asynchronous integration boundary, not as a synchronous helper method inside FAQ controllers.
- Do not move tenant provider secrets into host configuration files.
- If generation starts writing FAQ data in the future, document that new ownership model explicitly because it changes the current "stateless + callback only" behavior.
- When adding new AI event types, keep them versioned and explicit in the contracts projects.
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
| FAQ DB write failure | Callback may indicate failure for the request | Publish `Failed` callback with error and avoid partial FAQ writes |
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
- Runtime requests include prompt metadata in telemetry/logs (`PromptProfile`, prompt version, model ID, and correlation ID) for auditability.

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
- [x] AI lifecycle persistence removed (no dedicated AI DB).
- [x] Stateless AI workers wired with tenant/FAQ dependencies only.
- [x] No FK relationship exists between AI and FAQ databases.
- [x] Async generation event flow implemented end-to-end through RabbitMQ.
- [x] Callback flow implemented (`Ready`/`Failed`) and consumed by FAQ Portal API.
- [x] FAQ integration write flow from AI services to FAQ DB is idempotent and validated.
- [x] Matching endpoint implemented with asynchronous response for FAQ item retrieval, validates `FaqItemId` (required, non-empty, and existence/access in FAQ DB for current tenant), no fallback behavior required, and consumed only by FAQ Public API requests.
- [x] Idempotency key support and dedupe table in place.
- [x] Retry and DLQ policies configured and validated.
- [x] Distributed tracing across API, broker, worker, FAQ DB, provider enabled.
- [x] Monitoring dashboard and alerts configured.
- [x] Secret manager integration and key rotation implemented for Generation process.
- [x] Secret manager integration and key rotation implemented for Matching process.
- [x] Logging redaction rules validated (no key leakage).
- [x] Prompt governance and quality gate process documented.
- [x] MVP, hardening, and scale backlog items created and tracked.
