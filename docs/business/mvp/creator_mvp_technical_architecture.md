# Creator MVP Technical Architecture

Reference documents:

- [`creator_mvp_plan.md`](creator_mvp_plan.md)
- [`../../behavior-change-playbook.md`](../../behavior-change-playbook.md)
- [`../../backend/architecture/solution-architecture.md`](../../backend/architecture/solution-architecture.md)
- [`../../backend/architecture/dotnet-backend-overview.md`](../../backend/architecture/dotnet-backend-overview.md)

## Purpose

This document turns the Creator MVP product plan into a backend implementation plan based on the
current Querify repository shape.

The target product package is:

```text
Querify Creator = QnA Answer Hub + Direct Ask Me Inbox + Broadcast Comment Collector + Trust Approval Log
```

The architecture goal is to ship the MVP without turning QnA into a catch-all module. QnA owns
canonical questions, answers, sources, tags, activity, and public QnA signals. Direct owns private
1:1 conversations. Broadcast owns public and community interaction capture. Trust owns review,
approval, decision history, and auditability. Tenant owns entitlements, billing state, tenant users,
and module connection routing.

## Current Repository Baseline

The repository already has:

| Area | Current state |
|---|---|
| QnA runtime | `Querify.QnA.Portal.Api`, `Querify.QnA.Public.Api`, `Querify.QnA.Worker.Api` |
| QnA business features | Space, Question, Answer, Source, Tag, Activity, public Question, public Space, public Vote, public Feedback |
| QnA persistence | `Querify.QnA.Common.Domain`, `Querify.QnA.Common.Persistence.QnADb`, `Querify.QnA.Common.Persistence.HangfireQnaDb` |
| QnA worker behavior | RabbitMQ source-upload verification and status notifications |
| Direct contracts | `Querify.Models.Direct` with minimal conversation enums |
| Direct persistence | `Querify.Direct.Common.Persistence.DirectDb` with `Conversation` and `ConversationMessage` |
| Broadcast contracts | `Querify.Models.Broadcast` with minimal thread/item enums |
| Broadcast persistence | `Querify.Broadcast.Common.Persistence.BroadcastDb` with `Thread` and `Item` |
| Trust persistence | `Querify.Trust.Common.Persistence.TrustDb` exists but has no entities yet |
| Trust contracts/business | No `Querify.Models.Trust`, API host, or business feature projects yet |
| Tenant billing | Tenant subscriptions and `TenantEntitlementSnapshot.FeatureJson` already exist |
| Portal frontend | QnA-oriented domains exist; Direct, Broadcast, Trust, and Creator-specific domains do not |

## Non-Negotiable Architecture Rules

These rules come from the behavior-change playbook and should be treated as implementation
constraints:

- Do not store Direct conversation, inbox, resolution, or agent-assist workflow state in QnA.
- Do not store Broadcast comment, public thread, social, campaign, or grouping workflow state in QnA.
- Do not store Trust review, validation, governance, decision, or auditability state in QnA, Direct,
  Broadcast, or Tenant.
- Do not add placeholder entities or empty projects just to complete a taxonomy. Add a project only
  in the stage that introduces real behavior.
- Do not run or generate EF migrations during behavior implementation unless migration work is
  explicitly requested.
- Keep persisted entities anemic: state only, no behavior methods.
- Controllers, consumers, hosted services, and Hangfire jobs are adapters. They call services, and
  services dispatch MediatR commands or queries.
- Command handlers return simple values. Complex DTOs belong to queries.
- Public QnA uses `X-Client-Key`; authenticated Portal flows use `X-Tenant-Id`.
- Repository artifacts must be written in English.

## Target Module Ownership

| Creator capability | Owning module | Persistence owner | Runtime owner |
|---|---|---|---|
| Public Answer Hub | QnA | `QnADbContext` | Existing QnA Public and Portal APIs |
| Create/edit reusable questions and answers | QnA | `QnADbContext` | Existing QnA Portal API |
| AI draft for a QnA answer | QnA | `QnADbContext` for draft state; shared AI infrastructure for model call | Existing QnA Portal API, optional QnA Worker for long jobs |
| Private Ask Me form | Direct | `DirectDbContext` | New Direct Public API |
| Creator private inbox | Direct | `DirectDbContext` | New Direct Portal API |
| Direct suggested response from QnA | Direct owns suggestion record; QnA owns source answer | `DirectDbContext` plus read from `QnADbContext` | Direct Portal API |
| Promote private question to QnA draft | Direct owns origin trace; QnA owns new question/answer draft | `DirectDbContext` and `QnADbContext` | Direct Portal API coordinating module services |
| Manual comment thread | Broadcast | `BroadcastDbContext` | New Broadcast Portal API |
| CSV/paste comment import | Broadcast | `BroadcastDbContext` | Broadcast Portal API plus Broadcast Worker for durable processing |
| Comment classification and grouping | Broadcast | `BroadcastDbContext` | Broadcast Worker |
| Suggested public comment response | Broadcast owns response draft; QnA owns reused answer | `BroadcastDbContext` plus read from `QnADbContext` | Broadcast Portal API/Worker |
| Review required before publishing sensitive answer | Trust | `TrustDbContext` | New Trust Portal API; QnA activation checks Trust state |
| Approval, rejection, rationale, and decision history | Trust | `TrustDbContext` | Trust Portal API |
| Pricing and usage limits | Tenant | `TenantDbContext` | Tenant Portal/BackOffice APIs and module command guards |

## Existing Entities To Reuse

### QnA

Reuse these existing entities for the Answer Hub:

| Entity | MVP use |
|---|---|
| `Space` | Creator Hub or product-specific answer collection. Use `Name`, `Slug`, `Summary`, `Language`, `Status`, `Visibility`, `AcceptsQuestions`, and `AcceptsAnswers`. |
| `Question` | Canonical reusable audience question. A Direct or Broadcast gap may create a draft internal question, but Direct/Broadcast workflow state stays in the origin module. |
| `Answer` | Reusable answer. Draft, active, archived, and visibility state remain QnA lifecycle concerns. |
| `Source` | Pasted text, URL, FAQ page, product page, or uploaded artifact used to generate or support answers. |
| `QuestionTag` and `SpaceTag` | Basic creator tags such as product, price, delivery, access, guarantee, community, support, and partnership. |
| `Activity` | QnA lifecycle and public signal journal. Do not use it as Trust decision history. |

Do not add a `CreatorHub` entity for the first MVP cut. A public Creator Hub can be represented by
an active public `Space` with creator-oriented seed data and Portal navigation.

### Direct

Reuse and extend:

| Entity | Current state | MVP direction |
|---|---|---|
| `Conversation` | Has `Channel`, `Status`, optional `Subject`, `TenantId`, and messages | Extend for ask-form requester fields, consent, resolution timestamps, and optional QnA reference fields. |
| `ConversationMessage` | Has parent conversation, actor kind, body, sent timestamp, tenant | Reuse for follower question, creator reply, copied AI suggestion, and system timeline entries. |

### Broadcast

Reuse and extend:

| Entity | Current state | MVP direction |
|---|---|---|
| `Thread` | Has channel family, status, title, tenant, items | Extend for provider/source metadata and campaign/post context. |
| `Item` | Has parent thread, item kind, actor kind, body, captured timestamp, tenant | Extend for classification, spam-ignore state, grouping, external author, and source metadata. |

### Trust

`TrustDbContext` exists but has no entities. The MVP should add real Trust entities only when the
approval workflow is implemented.

## Entity Changes By Module

### QnA Entity Changes

Prefer no new QnA entity for stage 1. Add only small fields if a concrete Creator Hub behavior cannot
be represented by existing QnA state.

Recommended QnA behavior changes:

| Change | Location | Notes |
|---|---|---|
| Add creator seed/template data | `Querify.Tools.Seed` QnA seed catalog | Seed a `Creator Hub` space and creator tags. |
| Add QnA draft generation command | `Querify.QnA.Portal.Business.Answer` or `Source` depending on input | If the draft is from pasted text or URL, create/update `Source` and create a draft `Answer`. |
| Add answer activation guard for Trust | `Querify.QnA.Portal.Business.Answer` activate command | Query Trust review state before allowing sensitive answers to become active/public. |
| Add public hub query shape if needed | `Querify.QnA.Public.Business.Space` | Prefer composing existing space/question endpoints before adding a Creator-specific endpoint. |

Potential enum use:

| Dimension | Existing enum |
|---|---|
| Public/internal exposure | `VisibilityScope` |
| QnA lifecycle | `SpaceStatus`, `QuestionStatus`, `AnswerStatus` |
| Direct/Broadcast origin as QnA source | `ChannelKind` |
| Official AI/manual answer type | `AnswerKind` |

Do not add Direct inbox state, Broadcast grouping state, or Trust approval state to QnA entities.

### Direct Entity Changes

Extend `Conversation` for ask-form metadata:

| Field | Type | Reason |
|---|---|---|
| `RequesterName` | `string?` | Name submitted in the public ask form. |
| `RequesterContact` | `string?` | Email or handle. Keep generic for MVP. |
| `RequesterHandle` | `string?` | Optional social/community handle when separate from email. |
| `ConsentAcceptedAtUtc` | `DateTime?` | Records basic consent from public submission. |
| `AnsweredAtUtc` | `DateTime?` | Time a creator response was recorded or copied. |
| `ResolvedAtUtc` | `DateTime?` | Time the conversation was marked resolved. |
| `PromotedQuestionId` | `Guid?` | Optional external reference to the QnA question draft created from this conversation. No EF navigation across module databases. |
| `PromotedAnswerId` | `Guid?` | Optional external reference to the QnA answer draft created from this conversation. No EF navigation across module databases. |

Extend `ConversationStatus` for MVP workflow. The existing `Open`/`Closed` lifecycle is too coarse
for the product plan, which needs `new`, `answered`, and `resolved`, but the current enum already
uses `Open = 1` and `Closed = 6`. Preserve those numeric values unless a data reset is explicitly
accepted.

Recommended least-disruptive status model:

| Status | Meaning |
|---|---|
| `Open = 1` | Conversation can still receive work. The Portal can label open conversations without `AnsweredAtUtc` as `New`. |
| `Closed = 6` | Historical or administratively closed conversation. |
| `Answered = 11` | A creator response or copied response has been recorded, but the conversation has not been resolved. |
| `Resolved = 16` | The creator marked the conversation complete. |

Add a `ConversationResponseDraft` entity when suggestions need to persist:

| Field | Type | Reason |
|---|---|---|
| `ConversationId` | `Guid` | Parent Direct conversation. |
| `Body` | `string` | Suggested response text shown in the inbox. |
| `Status` | `ResponseDraftStatus` | Draft, accepted, dismissed, copied, or promoted. If Direct and Broadcast use the exact same lifecycle, put this enum in `Querify.Models.Common`; otherwise keep a Direct-specific enum. |
| `SourceQuestionId` | `Guid?` | QnA question used as evidence, external reference only. |
| `SourceAnswerId` | `Guid?` | QnA answer used as evidence, external reference only. |
| `AiConfidenceScore` | `int?` | Confidence returned by the suggestion service. |
| `GeneratedAtUtc` | `DateTime` | Domain timestamp for model output. |
| `AcceptedAtUtc` | `DateTime?` | Time the creator accepted or copied the draft. |
| `TenantId` | `Guid` | Tenant ownership. |

If stage 2 only returns suggestions on demand and does not need history, skip
`ConversationResponseDraft` until the UI needs saved suggestions.

### Broadcast Entity Changes

Extend `Thread`:

| Field | Type | Reason |
|---|---|---|
| `Provider` | `BroadcastProviderKind` | Distinguishes Instagram, TikTok, YouTube, LinkedIn, X, community, or other provider. This is narrower than `ChannelKind`. |
| `ExternalThreadUrl` | `string?` | Link to the post, video, live, lesson, campaign, or community thread. |
| `ExternalThreadId` | `string?` | Optional provider id when known from CSV/manual paste. |
| `CampaignLabel` | `string?` | Launch, live, lesson, or campaign grouping label. |
| `ImportedAtUtc` | `DateTime?` | Last time comments were imported for this thread. |
| `ClosedAtUtc` | `DateTime?` | Time the thread was completed. |

Extend `Item`:

| Field | Type | Reason |
|---|---|---|
| `ExternalItemId` | `string?` | Provider item/comment id when available. |
| `ExternalAuthorLabel` | `string?` | Public author label or handle from the import. |
| `Classification` | `BroadcastItemClassification` | Question, objection, praise, complaint, suggestion, spam, or other. |
| `IgnoreReason` | `string?` | Optional reason when item is ignored as spam/noise. |
| `ClusterId` | `Guid?` | Group assignment for recurring themes. |
| `SuggestedResponseId` | `Guid?` | Optional response draft reference inside Broadcast. |
| `ProcessedAtUtc` | `DateTime?` | Time AI/manual processing classified the item. |

Add `ImportBatch` for durable CSV/paste import:

| Field | Type | Reason |
|---|---|---|
| `ThreadId` | `Guid` | Parent Broadcast thread. |
| `Status` | `ImportBatchStatus` | Pending, processing, completed, failed, canceled. |
| `InputKind` | `ImportInputKind` | Paste, CSV upload, or manual entry. |
| `OriginalFileName` | `string?` | CSV filename when available. |
| `RawStorageKey` | `string?` | Object storage pointer if raw CSV is too large for the database. |
| `SubmittedItemCount` | `int` | Number of rows submitted. |
| `ProcessedItemCount` | `int` | Number of items processed successfully. |
| `FailedItemCount` | `int` | Number of rows that failed validation/classification. |
| `StartedAtUtc` | `DateTime?` | Processing start time. |
| `CompletedAtUtc` | `DateTime?` | Processing completion time. |
| `ErrorMessage` | `string?` | Stable user-facing failure reason for the import. |
| `TenantId` | `Guid` | Tenant ownership. |

Add `ItemCluster` for recurring themes:

| Field | Type | Reason |
|---|---|---|
| `ThreadId` | `Guid` | Parent thread. |
| `Title` | `string` | Human-readable cluster theme. |
| `Summary` | `string?` | AI/manual summary of repeated audience question. |
| `Classification` | `BroadcastItemClassification` | Dominant theme classification. |
| `ItemCount` | `int` | Cached count for list views. |
| `RepresentativeItemId` | `Guid?` | Sample item used for display. |
| `PromotedQuestionId` | `Guid?` | Optional QnA draft created from the cluster. |
| `SuggestedResponseId` | `Guid?` | Optional Broadcast response draft. |
| `TenantId` | `Guid` | Tenant ownership. |

Add `BroadcastResponseDraft` when public replies need to persist:

| Field | Type | Reason |
|---|---|---|
| `ThreadId` | `Guid` | Parent thread. |
| `ItemClusterId` | `Guid?` | Cluster the response answers. |
| `Body` | `string` | Suggested public reply. |
| `Status` | `ResponseDraftStatus` | Draft, copied, dismissed, promoted. Reuse a common enum only if the lifecycle is identical to Direct response drafts. |
| `SourceQuestionId` | `Guid?` | QnA question used as evidence, external reference only. |
| `SourceAnswerId` | `Guid?` | QnA answer used as evidence, external reference only. |
| `GeneratedAtUtc` | `DateTime` | Domain timestamp for model output. |
| `TenantId` | `Guid` | Tenant ownership. |

### Trust Entity Changes

Add `Querify.Models.Trust` before adding Trust DTOs or enums. Then add entities in
`Querify.Trust.Common.Persistence.TrustDb`.

Add `ReviewRequest`:

| Field | Type | Reason |
|---|---|---|
| `TargetModule` | `ModuleEnum` | QnA, Direct, or Broadcast asset under review. |
| `TargetKind` | `ReviewTargetKind` | QnA answer, QnA question, Direct response draft, or Broadcast response draft. |
| `TargetId` | `Guid` | External target id. No EF navigation across module databases. |
| `Status` | `ReviewRequestStatus` | Pending, approved, rejected, changes requested, stale, canceled. |
| `RiskCategory` | `ReviewRiskCategory` | Pricing, promise, guarantee, health, finance, legal, partnership, community rule, discount, or other. |
| `Reason` | `string?` | Why review is required. |
| `ContentHash` | `string` | Hash of reviewed content to detect stale approvals after edits. |
| `ContentSnapshotJson` | `string?` | Minimal reviewed content snapshot for decision history. |
| `RequestedByUserId` | `Guid?` | User who requested review, when known. |
| `RequestedAtUtc` | `DateTime` | Domain timestamp for review request. |
| `ResolvedAtUtc` | `DateTime?` | Time final decision was recorded. |
| `TenantId` | `Guid` | Tenant ownership. |

Add `ReviewDecision`:

| Field | Type | Reason |
|---|---|---|
| `ReviewRequestId` | `Guid` | Parent review request. |
| `Decision` | `ReviewDecisionKind` | Approved, rejected, or changes requested. |
| `Rationale` | `string?` | Short reviewer rationale. |
| `DecidedByUserId` | `Guid?` | Reviewer identity when known. |
| `DecidedAtUtc` | `DateTime` | Domain timestamp for decision. |
| `ContentHash` | `string` | Content hash approved or rejected by this decision. |
| `TenantId` | `Guid` | Tenant ownership. |

Use Trust as the blocking source of truth:

- QnA `ActivateAnswer` checks Trust only when the answer is sensitive or has a pending review.
- Trust approval does not become a QnA boolean.
- If answer content changes after approval, mark the Trust request stale through a QnA event or an
  explicit Trust invalidation command.

### Tenant Entity Changes

The current `TenantEntitlementSnapshot.FeatureJson` can hold plan limits for the MVP. That avoids
schema churn for every limit while pricing is still changing.

Recommended MVP feature JSON shape:

```json
{
  "creatorPlan": "starter",
  "limits": {
    "qnaQuestions": 50,
    "directPrivateQuestionsPerMonth": 100,
    "broadcastImportedItemsPerMonth": 100,
    "aiSuggestionsPerMonth": 100,
    "users": 1,
    "spaces": 1,
    "creatorHubs": 1
  }
}
```

If enforcement needs queryable monthly usage, add Tenant-owned usage counters:

| Entity | Owner | Reason |
|---|---|---|
| `TenantUsageCounter` | Tenant | Aggregated monthly usage by feature key, period, and tenant. |
| `TenantUsageEvent` | Tenant or module-specific later | Optional audit trail if overage billing needs item-level evidence. Do not add for MVP unless billing requires it. |

The module command handlers should enforce limits by reading Tenant entitlements through a shared
service, not by duplicating plan rules in QnA, Direct, or Broadcast.

## New Projects To Add

Add projects only in the stage that introduces real behavior.

### Required Contract And Infrastructure Projects

| Project | Stage | Purpose |
|---|---:|---|
| `Querify.Models.Trust` | Trust stage | Trust DTOs and enums. |
| `Querify.Common.Infrastructure.Ai` | AI stage | Provider-neutral AI abstractions for draft generation, answer suggestion, classification, clustering, and summarization. |

`Querify.Common.Infrastructure.Ai` should expose abstractions such as:

- `IAiTextGenerationService`
- `IAiAnswerSuggestionService`
- `IAiClassificationService`
- `IAiEmbeddingService` only if semantic matching is implemented in the stage

Keep prompt templates close to the owning feature when they encode product behavior. Keep provider
clients and retry/timeout configuration in the shared infrastructure project.

### Direct Projects

| Project | Purpose |
|---|---|
| `Querify.Direct.Public.Api` | Public Ask Me form ingress. |
| `Querify.Direct.Public.Business.Ask` | Public ask-form command that creates a conversation and first message. |
| `Querify.Direct.Portal.Api` | Authenticated creator inbox APIs. |
| `Querify.Direct.Portal.Business.Conversation` | Inbox list/detail, reply, resolve, create QnA gap, suggest answer. |
| `Querify.Direct.Portal.Test.IntegrationTests` | Direct Portal API and command/query coverage. |
| `Querify.Direct.Public.Test.IntegrationTests` | Public form tenant resolution and conversation creation coverage. |

Do not add `Querify.Direct.Worker.Api` for the first MVP cut unless a durable async Direct job is
introduced. Direct form submission and inbox actions can be synchronous.

### Broadcast Projects

| Project | Purpose |
|---|---|
| `Querify.Broadcast.Portal.Api` | Authenticated comment collector APIs. |
| `Querify.Broadcast.Portal.Business.Thread` | Thread CRUD, item list/detail, cluster list, response draft actions. |
| `Querify.Broadcast.Portal.Business.Import` | CSV/paste import submission and import status queries. |
| `Querify.Broadcast.Worker.Api` | Durable processing of submitted import batches. |
| `Querify.Broadcast.Worker.Business.Import` | Parse, validate, classify, cluster, and finalize import batches. |
| `Querify.Broadcast.Portal.Test.IntegrationTests` | Thread/import command/query coverage. |
| `Querify.Broadcast.Worker.Test.IntegrationTests` | Import processor and retry/failure coverage. |

Broadcast should have a worker because classification and clustering can exceed normal request
latency, and a failed import leaves user-visible state stuck.

### Trust Projects

| Project | Purpose |
|---|---|
| `Querify.Trust.Portal.Api` | Authenticated review queue and decision APIs. |
| `Querify.Trust.Portal.Business.Review` | Request review, list pending reviews, approve, reject, request changes, mark stale. |
| `Querify.Trust.Portal.Test.IntegrationTests` | Review request, decision, and activation-guard coverage. |

Do not add a Trust worker in the MVP unless stale-review reconciliation or asynchronous review
notifications become a real requirement.

### QnA Project Changes

Prefer extending existing projects:

| Existing project | Change |
|---|---|
| `Querify.QnA.Portal.Business.Space` | Creator Hub space template/onboarding endpoints if existing create/update endpoints are not enough. |
| `Querify.QnA.Portal.Business.Answer` | AI answer draft, activation guard that checks Trust. |
| `Querify.QnA.Portal.Business.Question` | Create draft question from Direct/Broadcast origin. |
| `Querify.QnA.Public.Business.Space` | Public Creator Hub query shape if current public space/question endpoints are too chatty. |
| `Querify.Tools.Seed` | Creator sample space, questions, answers, tags, and optionally Direct/Broadcast/Trust sample data once those seed services exist. |

Avoid adding `Querify.QnA.Portal.Business.CreatorHub` unless the Creator Hub develops behavior that
cannot be owned cleanly by Space, Question, Answer, Source, or Tag.

## Events And Integration Contracts

Use events for cross-module or async work. Do not use events as a replacement for command/query
behavior inside a module.

### Required MVP Events

| Event | Publisher | Consumer | Purpose |
|---|---|---|---|
| `BroadcastImportSubmittedIntegrationEvent` | Broadcast Portal API | Broadcast Worker | Process a durable CSV/paste import batch. |
| `BroadcastImportCompletedIntegrationEvent` | Broadcast Worker | Broadcast Portal notification service, optional | Notify Portal that classification/clustering finished. |
| `QnAAnswerContentChangedIntegrationEvent` | QnA Portal Answer command | Trust review feature | Mark matching approval stale when reviewed content changes. |
| `TrustReviewDecisionRecordedIntegrationEvent` | Trust Portal API | QnA Portal notification service, optional | Notify answer screen that review state changed. |

### Optional Events

| Event | Use when |
|---|---|
| `DirectQuestionSubmittedIntegrationEvent` | The public ask form should send notification email, trigger async suggestion generation, or feed analytics. |
| `DirectResponseDraftCreatedIntegrationEvent` | Saved Direct suggestions need Portal real-time notification. |
| `QnADraftCreatedFromDirectIntegrationEvent` | Product analytics needs to track private questions promoted to QnA. |
| `QnADraftCreatedFromBroadcastIntegrationEvent` | Product analytics needs to track comment clusters promoted to QnA. |

### Event Rules

- Put integration event contracts in the owning `Querify.Models.<Module>/Events` project folder.
- Name event exchanges and queues in an event-name constants file, following the existing QnA source
  upload pattern.
- Consumers remain adapters. Use `*Consumer` in API/worker host layer and `*ConsumerService` in the
  business feature layer.
- Consumer services start telemetry and dispatch one MediatR command/query.
- Commands own EF reads/writes, validation, retry/finalization decisions, and side effects.

## Workers And Background Processing

### Keep Existing QnA Worker Focused

`Querify.QnA.Worker.Api` already owns source-upload verification. Do not add Direct, Broadcast, or
Trust processing to the QnA worker just because it already exists.

Use the QnA worker for Creator MVP only if the work is QnA-owned, for example:

- source verification for FAQ/product material uploads;
- future reconciliation of QnA draft generation jobs if draft generation becomes async.

### Add Broadcast Worker For Imports

Recommended pipeline:

```text
Portal submits paste/CSV
  -> Broadcast Portal command creates ImportBatch(Pending)
  -> Broadcast Portal publishes BroadcastImportSubmittedIntegrationEvent
  -> Broadcast Worker consumer calls ImportBatchConsumerService
  -> ConsumerService dispatches ProcessBroadcastImportBatchCommand
  -> command parses, validates, creates Items, classifies, clusters, creates response drafts
  -> command marks ImportBatch Completed or Failed
  -> worker publishes BroadcastImportCompletedIntegrationEvent
```

Add a low-frequency reconciliation hosted service only if lost broker messages are a practical risk:

```text
BroadcastImportReconciliationHostedService
  -> BroadcastImportReconciliationProcessorService
  -> RequeueStaleBroadcastImportBatchesCommand
```

The hosted service and processor service must not parse CSV, call AI, or update item state directly.

### Avoid Direct Worker In MVP

Direct can be synchronous at MVP scale:

- public form creates a conversation and first message;
- Portal inbox query lists conversations;
- suggestion endpoint reads QnA and calls AI on demand;
- creator copies or records a reply;
- creator promotes a question to QnA draft.

Add `Querify.Direct.Worker.Api` only if notification delivery, async suggestion generation, or email
reply sending becomes durable product behavior.

### Avoid Trust Worker In MVP

Trust approval is a human workflow and can be request-driven:

- request review;
- list review queue;
- record decision;
- QnA activation checks review state.

Add a Trust worker only when stale-review reconciliation, scheduled expiration, or notification
delivery becomes a concrete requirement.

## API Surface Plan

### QnA Portal

Use existing routes where possible:

| Capability | Endpoint direction |
|---|---|
| Create Creator Hub space | Existing `api/qna/space` create, optionally with creator template endpoint |
| Create/edit questions | Existing `api/qna/question` |
| Create/edit answers | Existing `api/qna/answer` |
| Generate draft answer | New action under `api/qna/answer/generate-draft` or `api/qna/question/{id}/answer-draft` |
| Activate answer | Existing `api/qna/answer/{id}/activate`, with Trust guard added |

### QnA Public

Use current public QnA endpoints first. Add a public hub endpoint only if the frontend needs one
round trip:

```text
GET api/qna/creator-hub/{spaceSlug}
GET api/qna/creator-hub/{spaceSlug}/question
GET api/qna/creator-hub/{spaceSlug}/question/{questionSlugOrId}
```

If implemented, the owning feature should still read `Space`, `Question`, and `Answer`; do not add a
Creator Hub persistence model.

### Direct Public

```text
POST api/direct/ask
```

Request fields:

- client key or tenant public route context;
- creator hub or space id/slug when the ask form is attached to a hub;
- requester name;
- requester contact or handle;
- question body;
- optional context;
- consent accepted.

The command creates a `Conversation` and a first `ConversationMessage`.

### Direct Portal

```text
GET    api/direct/conversation
GET    api/direct/conversation/{id}
POST   api/direct/conversation/{id}/reply
POST   api/direct/conversation/{id}/resolve
POST   api/direct/conversation/{id}/suggest-answer
POST   api/direct/conversation/{id}/promote-to-qna
```

### Broadcast Portal

```text
POST   api/broadcast/thread
GET    api/broadcast/thread
GET    api/broadcast/thread/{id}
POST   api/broadcast/thread/{id}/import
GET    api/broadcast/thread/{id}/item
GET    api/broadcast/thread/{id}/cluster
POST   api/broadcast/cluster/{id}/suggest-response
POST   api/broadcast/cluster/{id}/promote-to-qna
```

### Trust Portal

```text
POST   api/trust/review
GET    api/trust/review
GET    api/trust/review/{id}
POST   api/trust/review/{id}/approve
POST   api/trust/review/{id}/reject
POST   api/trust/review/{id}/request-changes
POST   api/trust/review/{id}/mark-stale
GET    api/trust/review/by-target
```

`by-target` lets QnA answer detail screens show review state for a specific answer without loading
the whole review queue.

## AI Service Placement

AI is a shared technical capability, but the business decision remains in the owning module command.

| AI use case | Owning command | Shared service |
|---|---|---|
| Generate QnA answer draft from pasted text or URL | QnA Answer or Source command | Text generation |
| Suggest Direct response from QnA | Direct Conversation command | Answer suggestion, optional semantic search |
| Classify Broadcast comments | Broadcast Import command | Classification |
| Cluster Broadcast questions | Broadcast Import command | Embeddings or text grouping |
| Summarize QnA gap from Direct/Broadcast | Direct/Broadcast promote command | Summarization |
| Detect sensitive answer categories | Trust review request command or QnA preflight command | Classification |

Keep generated output traceable:

- store the final generated text in the owning draft entity;
- store confidence and source QnA ids when a response is suggested from existing knowledge;
- do not store provider prompts or raw model metadata unless needed for debugging or compliance;
- enforce AI usage limits through Tenant entitlements before model calls.

## Cross-Module Flows

### Private Question To FAQ

```text
Direct Public Ask
  -> creates Direct Conversation + ConversationMessage
Creator opens Direct Portal Inbox
  -> Direct query lists new conversations
Creator asks for suggested response
  -> Direct command searches QnA active public/internal answers
  -> AI creates Direct response draft
Creator promotes to QnA
  -> Direct command creates QnA draft Question/Answer through QnA service/command
  -> Direct stores PromotedQuestionId/PromotedAnswerId as external references
If sensitive
  -> Trust ReviewRequest is created
Trust approves
  -> QnA answer activation succeeds
```

### Comments To Content

```text
Creator creates Broadcast Thread
  -> Broadcast Thread(Open)
Creator imports comments
  -> Broadcast ImportBatch(Pending)
Broadcast Worker processes batch
  -> Items created
  -> Items classified
  -> ItemClusters created
Creator promotes cluster
  -> QnA draft Question/Answer created
If sensitive
  -> Trust ReviewRequest is created
Trust approves
  -> QnA answer activation succeeds
```

### Public Hub Reduces Repetition

```text
Creator publishes QnA Space as public Creator Hub
Follower searches public QnA
  -> QnA Public API serves active public questions/answers
Follower submits feedback
  -> QnA public feedback activity is recorded
Follower asks privately
  -> Direct Public Ask creates conversation
```

## Implementation Stages

### Stage 1: Package Existing QnA As Creator Hub

Scope:

- no new persistence required;
- seed creator tags and a `Creator Hub` sample space;
- ensure public QnA can render a useful answer hub;
- add minimal Portal navigation for the creator answer hub using existing QnA APIs.

Backend:

- `Querify.QnA.Portal.Business.Space`
- `Querify.QnA.Portal.Business.Question`
- `Querify.QnA.Portal.Business.Answer`
- `Querify.QnA.Public.Business.Space`
- `Querify.QnA.Public.Business.Question`
- `Querify.Tools.Seed`

Validation:

- QnA Portal build/test;
- QnA Public build/test;
- seed tool compile.

Manual migration:

- none expected.

### Stage 2: Direct Ask Me Inbox

Scope:

- extend Direct model;
- add Direct Public and Portal APIs;
- create conversations from public ask form;
- list/reply/resolve in Portal.

Backend:

- update `Querify.Models.Direct`;
- update `Querify.Direct.Common.Persistence.DirectDb`;
- add `Querify.Direct.Public.Api`;
- add `Querify.Direct.Public.Business.Ask`;
- add `Querify.Direct.Portal.Api`;
- add `Querify.Direct.Portal.Business.Conversation`;
- add Direct integration test projects.

Manual migration:

- add/alter Direct tables and indexes for new fields/status values;
- add Direct module connection setup for tenants if not already seeded.

### Stage 3: AI Drafts And Direct Suggestions

Scope:

- add shared AI infrastructure;
- generate QnA draft from pasted text/URL;
- suggest Direct response from active QnA answers;
- promote Direct conversation to QnA draft.

Backend:

- add `Querify.Common.Infrastructure.Ai`;
- extend QnA Answer/Source commands;
- extend Direct Conversation commands;
- enforce AI usage limits through Tenant entitlements.

Manual migration:

- only required if saved Direct suggestion drafts are introduced.

### Stage 4: Broadcast Comment Collector

Scope:

- extend Broadcast model;
- add Broadcast Portal API;
- add import batch and item clustering;
- add Broadcast Worker;
- classify and group imported comments.

Backend:

- update `Querify.Models.Broadcast`;
- update `Querify.Broadcast.Common.Persistence.BroadcastDb`;
- add `Querify.Broadcast.Portal.Api`;
- add `Querify.Broadcast.Portal.Business.Thread`;
- add `Querify.Broadcast.Portal.Business.Import`;
- add `Querify.Broadcast.Worker.Api`;
- add `Querify.Broadcast.Worker.Business.Import`;
- add Broadcast integration test projects.

Manual migration:

- add Broadcast provider/classification/import/cluster/response-draft tables and indexes;
- add Broadcast worker runtime configuration.

### Stage 5: Trust Approval Log

Scope:

- add Trust contracts;
- add review request and decision entities;
- add Trust Portal API;
- block sensitive QnA answer activation without current approval;
- mark approval stale when reviewed content changes.

Backend:

- add `Querify.Models.Trust`;
- update `Querify.Trust.Common.Persistence.TrustDb`;
- add `Querify.Trust.Portal.Api`;
- add `Querify.Trust.Portal.Business.Review`;
- update QnA Answer activation command;
- add Trust and QnA integration tests for approval guard.

Manual migration:

- create Trust review tables and indexes;
- add Trust module connection setup for tenants if not already seeded.

### Stage 6: Pricing, Limits, And Usage

Scope:

- encode Creator Starter/Growth/Pro limits;
- enforce QnA, Direct, Broadcast, AI, user, and space limits;
- expose basic usage summary.

Backend:

- update Tenant entitlement synchronizer to populate `FeatureJson`;
- add a shared entitlement/limit service if one does not exist;
- optionally add `TenantUsageCounter` when usage must be queryable and reset monthly;
- add limit checks to module commands before writes or AI calls.

Manual migration:

- none if using `FeatureJson`;
- required if adding `TenantUsageCounter`.

## Indexing And Query Notes

Recommended indexes once schema work is explicitly requested:

| Module | Index |
|---|---|
| Direct | `TenantId, Status, CreatedDate` for inbox list |
| Direct | `TenantId, PromotedQuestionId` when tracing promoted QnA gaps |
| Broadcast | `TenantId, Status, CreatedDate` for thread list |
| Broadcast | `TenantId, ThreadId, Classification` for item filters |
| Broadcast | `TenantId, ThreadId, ItemCount` for cluster ranking |
| Broadcast | `TenantId, ImportBatchId, Status` if import rows become separate entities |
| Trust | `TenantId, Status, CreatedDate` for review queue |
| Trust | `TenantId, TargetModule, TargetKind, TargetId` for answer activation guard |
| Tenant | `TenantId, FeatureKey, PeriodStartUtc` if `TenantUsageCounter` is added |

## Portal Domains To Add

Add frontend domains in the same staged order as backend APIs:

| Domain | Purpose |
|---|---|
| `creator` | Top-level Creator Hub workspace page that composes QnA, Direct, Broadcast, and Trust summaries. |
| `direct` | Ask Me inbox list/detail/reply/resolve/suggest/promote flows. |
| `broadcast` | Comment collector threads, imports, items, clusters, and response drafts. |
| `trust` | Review queue and approval decision screens. |
| `usage` or billing extension | Plan limits and monthly usage. |

The Portal should use tabs inside the Creator Hub area:

- Answers;
- Inbox;
- Comments;
- Review.

Keep each tab backed by its owning module API. Do not create a frontend type that merges module
contracts into one large Creator DTO unless it is a read-only dashboard query.

## Testing Strategy

Add tests per stage:

| Stage | Test focus |
|---|---|
| QnA Creator Hub | public active space/question/answer visibility, creator seed data, answer activation behavior unchanged |
| Direct | public ask form creates conversation/message, tenant isolation, inbox filters, reply/resolve transitions, promote-to-QnA |
| AI suggestions | entitlement checks before model calls, source answer ids preserved, no suggestion when no eligible QnA answer exists |
| Broadcast | import batch transitions, item validation, classification, cluster generation, failure/retry behavior |
| Trust | review request lifecycle, decision history, stale approval on content change, QnA activation blocked without approval |
| Pricing/limits | limit enforcement across QnA, Direct, Broadcast, AI, spaces, and users |

Keep architecture tests updated when new API, worker, business, model, or persistence project patterns
are introduced.

## Manual Migration Handoff

Do not generate migrations during implementation unless the request explicitly includes migration
work. Each stage that changes persistence must end with a migration handoff listing:

- added tables;
- renamed or changed enum values;
- added columns;
- required backfills;
- indexes;
- constraints;
- tenant module connection setup;
- data migration risks for existing tenants.

## Architecture Decision Summary

1. Use existing QnA entities for the public Creator Answer Hub.
2. Add Direct APIs and extend Direct persistence for the private Ask Me Inbox.
3. Add Broadcast APIs and a Broadcast Worker for durable comment import, classification, and
   clustering.
4. Add real Trust contracts/entities/business only when implementing approval log behavior.
5. Use Trust as the approval source of truth; do not copy Trust state into QnA answers.
6. Use Tenant entitlements for Creator plan limits and keep module commands responsible for checking
   those limits before creating records or calling AI.
7. Use MassTransit events only for cross-module or async work. Keep request-time behavior in the
   owning module command/query path.
8. Keep the first MVP synchronous where user latency allows it; add workers only for durable,
   potentially slow, user-visible processing such as Broadcast imports.
