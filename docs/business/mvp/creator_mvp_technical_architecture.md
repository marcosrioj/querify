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
Querify Creator = QnA Answer Hub + Direct Ask Me Inbox + Instagram/TikTok/YouTube Channel Integration + Enterprise Vector Search + MCP Server + Multi-Agent Runtime + Trust Policy Log
```

The architecture goal is to ship the MVP without turning QnA into a catch-all module. QnA owns
canonical questions, answers, sources, tags, activity, and public QnA signals. Direct owns private
1:1 conversations. Broadcast owns public and community interaction capture. Trust owns policy
evaluation, decision history, and auditability. Tenant owns entitlements, billing state, tenant
users, and module connection routing.

The MVP is not accepted with operator-driven channel work or a single-provider social cut. The
minimum release must include Instagram, TikTok, and YouTube integrations that cover connection
setup, credential persistence, inbound listening through webhooks or incremental polling,
persistence of channel events and comments, response generation, Trust/policy gating, and outbound
write-back to the connected channel when the provider allows it. The MVP also includes enterprise
vector search, the native `Querify.MCP.Server`, and a complete module-scoped multi-agent runtime.

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
| Social channel integrations | No Instagram, TikTok, or YouTube provider connection, webhook listener, polling cursor, raw event log, or outbound channel writer exists yet |
| Trust persistence | `Querify.Trust.Common.Persistence.TrustDb` exists but has no entities yet |
| Trust contracts/business | No `Querify.Models.Trust`, API host, or business feature projects yet |
| Tenant billing | Tenant subscriptions and `TenantEntitlementSnapshot.FeatureJson` already exist |
| MCP | TypeScript proxy exists as a prototype; native `Querify.MCP.Server` is designed but not built |
| Multi-agent runtime | Design exists in future MCP docs; product runtime and module agents are not built |
| Enterprise retrieval | No QnA enterprise vector search, chunk index, re-ranking, or retrieval service exists yet |
| Portal frontend | QnA-oriented domains exist; Direct, Broadcast, Trust, and Creator-specific domains do not |

## Non-Negotiable Architecture Rules

These rules come from the behavior-change playbook and should be treated as implementation
constraints:

- Do not store Direct conversation, inbox, resolution, or agent-assist workflow state in QnA.
- Do not store Broadcast comment, public thread, social, campaign, or grouping workflow state in QnA.
- Do not store Trust policy, validation, governance, decision, or auditability state in QnA, Direct,
  Broadcast, or Tenant.
- Do not add placeholder entities or empty projects just to complete a taxonomy. Add a project only
  in the stage that introduces real behavior.
- Do not run or generate EF migrations during behavior implementation unless migration work is
  explicitly requested.
- Do not define MVP product flows that require copy/paste, CSV upload, external reply copying, or
  human data shuttling between a social platform and Querify.
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
| Enterprise vector search and retrieval | QnA owns corpus semantics; shared retrieval infrastructure owns index adapters | `QnADbContext` plus vector/search index storage | QnA Public/Portal APIs, QnA Worker, MCP tools, and module agents |
| Private Ask Me form | Direct | `DirectDbContext` | New Direct Public API |
| Creator private inbox | Direct | `DirectDbContext` | New Direct Portal API |
| Direct suggested response from QnA | Direct owns suggestion record; QnA owns source answer | `DirectDbContext` plus read from `QnADbContext` | Direct Portal API |
| Promote private question to QnA draft | Direct owns origin trace; QnA owns new question/answer draft | `DirectDbContext` and `QnADbContext` | Direct Portal API coordinating module services |
| Instagram/TikTok/YouTube connection setup | Broadcast owns public-channel workflow; Tenant owns entitlement checks | `BroadcastDbContext` plus secure credential reference | Broadcast Portal API plus Broadcast Integration API |
| Automatic inbound social listening for Instagram/TikTok/YouTube | Broadcast | `BroadcastDbContext` | Broadcast Integration API plus Broadcast Worker |
| Automatic outbound social reply for Instagram/TikTok/YouTube | Broadcast | `BroadcastDbContext` | Broadcast Worker plus provider clients |
| Comment classification and grouping | Broadcast | `BroadcastDbContext` | Broadcast Worker |
| Suggested public comment response | Broadcast owns response draft; QnA owns reused answer | `BroadcastDbContext` plus read from `QnADbContext` | Broadcast Portal API/Worker |
| Policy gate before publishing or sending sensitive content | Trust | `TrustDbContext` | New Trust Portal API; QnA activation and channel write-back check Trust state |
| Policy decision, rationale, and history | Trust | `TrustDbContext` | Trust Portal API |
| Native MCP server and module tools | Integration layer | Module databases through owning commands/queries; no independent MCP persistence | `Querify.MCP.Server` |
| Multi-agent runtime | AI infrastructure coordinates agents; modules own side effects | Agent run state in owning module when durable; no QnA catch-all state | MCP server, module APIs, workers, and agent runtime services |
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
| `ConversationMessage` | Has parent conversation, actor kind, body, sent timestamp, tenant | Reuse for follower question, generated response, provider delivery record, and system timeline entries. |

### Broadcast

Reuse and extend:

| Entity | Current state | MVP direction |
|---|---|---|
| `Thread` | Has channel family, status, title, tenant, items | Extend for provider/source metadata and campaign/post context. |
| `Item` | Has parent thread, item kind, actor kind, body, captured timestamp, tenant | Extend for classification, spam-ignore state, grouping, external author, and source metadata. |

### Trust

`TrustDbContext` exists but has no entities. The MVP should add real Trust entities only when the
policy gate and decision log are implemented.

## Entity Changes By Module

### QnA Entity Changes

Prefer no new QnA entity for stage 1. Add only small fields if a concrete Creator Hub behavior cannot
be represented by existing QnA state.

Recommended QnA behavior changes:

| Change | Location | Notes |
|---|---|---|
| Add creator seed/template data | `Querify.Tools.Seed` QnA seed catalog | Seed a `Creator Hub` space and creator tags. |
| Add QnA draft generation command | `Querify.QnA.Portal.Business.Answer` or `Source` depending on input | If the draft is from captured channel content or URL, create/update `Source` and create a draft `Answer`. |
| Add enterprise retrieval query | `Querify.QnA.Public.Business.Search` and `Querify.QnA.Portal.Business.Search` | Hybrid lexical + vector search with tenant, space, visibility, status, language, source, and validity filters. |
| Add source chunking/reindex command | `Querify.QnA.Worker.Business.Ingestion` | Extract chunks, generate embeddings, update vector index, and preserve evidence references for agents. |
| Add answer activation guard for Trust | `Querify.QnA.Portal.Business.Answer` activate command | Query Trust policy decision state before allowing sensitive answers to become active/public. |
| Add public hub query shape if needed | `Querify.QnA.Public.Business.Space` | Prefer composing existing space/question endpoints before adding a Creator-specific endpoint. |

Potential enum use:

| Dimension | Existing enum |
|---|---|
| Public/internal exposure | `VisibilityScope` |
| QnA lifecycle | `SpaceStatus`, `QuestionStatus`, `AnswerStatus` |
| Direct/Broadcast origin as QnA source | `ChannelKind` |
| Official or AI-assisted answer type | `AnswerKind` |

Do not add Direct inbox state, Broadcast grouping state, or Trust policy state to QnA entities.

### Direct Entity Changes

Extend `Conversation` for ask-form metadata:

| Field | Type | Reason |
|---|---|---|
| `RequesterName` | `string?` | Name submitted in the public ask form. |
| `RequesterContact` | `string?` | Email or handle. Keep generic for MVP. |
| `RequesterHandle` | `string?` | Optional social/community handle when separate from email. |
| `ConsentAcceptedAtUtc` | `DateTime?` | Records basic consent from public submission. |
| `AnsweredAtUtc` | `DateTime?` | Time a response was recorded or queued for provider delivery. |
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
| `Answered = 11` | A response has been recorded or queued for provider delivery, but the conversation has not been resolved. |
| `Resolved = 16` | The creator marked the conversation complete. |

Add a `ConversationResponseDraft` entity when suggestions need to persist:

| Field | Type | Reason |
|---|---|---|
| `ConversationId` | `Guid` | Parent Direct conversation. |
| `Body` | `string` | Suggested response text shown in the inbox. |
| `Status` | `ResponseDraftStatus` | Draft, accepted, dismissed, queued, sent, failed, or promoted. If Direct and Broadcast use the exact same lifecycle, put this enum in `Querify.Models.Common`; otherwise keep a Direct-specific enum. |
| `SourceQuestionId` | `Guid?` | QnA question used as evidence, external reference only. |
| `SourceAnswerId` | `Guid?` | QnA answer used as evidence, external reference only. |
| `AiConfidenceScore` | `int?` | Confidence returned by the suggestion service. |
| `GeneratedAtUtc` | `DateTime` | Domain timestamp for model output. |
| `AcceptedAtUtc` | `DateTime?` | Time the draft became the selected response. |
| `SentAtUtc` | `DateTime?` | Time the response was delivered by the configured channel. |
| `TenantId` | `Guid` | Tenant ownership. |

If stage 2 only returns suggestions on demand and does not need history, skip
`ConversationResponseDraft` until the UI needs saved suggestions.

### Broadcast Entity Changes

Add `ChannelConnection` for the connected social account:

| Field | Type | Reason |
|---|---|---|
| `Provider` | `BroadcastProviderKind` | Distinguishes Instagram, TikTok, YouTube, or later providers such as LinkedIn, X, community, or other. |
| `ExternalAccountId` | `string` | Provider account/page/channel id. |
| `ExternalAccountName` | `string?` | Display name shown in Portal. |
| `Status` | `ChannelConnectionStatus` | Pending, connected, degraded, reconnect required, disabled, or revoked. |
| `CredentialReference` | `string` | Reference to encrypted token storage or secret manager entry. Do not store raw tokens in module tables. |
| `GrantedScopesJson` | `string?` | Provider scopes granted during OAuth/API setup. |
| `ReadCapability` | `ChannelCapabilityStatus` | Whether the connection can read comments/messages. |
| `WriteCapability` | `ChannelCapabilityStatus` | Whether the connection can write replies. |
| `WebhookStatus` | `ChannelWebhookStatus` | Not supported, pending verification, active, failed, or disabled. |
| `LastSyncedAtUtc` | `DateTime?` | Last successful incremental read or webhook processing timestamp. |
| `LastHealthCheckedAtUtc` | `DateTime?` | Last provider health/permission check. |
| `LastErrorMessage` | `string?` | Stable user-facing connection problem. |
| `TenantId` | `Guid` | Tenant ownership. |

Add `ChannelEvent` for idempotent inbound processing:

| Field | Type | Reason |
|---|---|---|
| `ChannelConnectionId` | `Guid` | Connected account that produced the event. |
| `Provider` | `BroadcastProviderKind` | Provider that sent or was polled for the event. |
| `ExternalEventId` | `string?` | Provider event id when available. |
| `IdempotencyKey` | `string` | Stable key based on provider, account, event kind, and external item id. |
| `EventKind` | `ChannelEventKind` | Comment created, comment updated, comment deleted, direct message received, thread discovered, or permission changed. |
| `PayloadJson` | `string?` | Small normalized payload snapshot. Use object storage for large payloads. |
| `PayloadStorageKey` | `string?` | Object storage pointer for full provider payload when needed. |
| `Status` | `ChannelEventProcessingStatus` | Received, processing, processed, ignored, failed, or poison. |
| `ReceivedAtUtc` | `DateTime` | Time Querify received or polled the event. |
| `ProcessedAtUtc` | `DateTime?` | Time the event created or updated module state. |
| `ErrorMessage` | `string?` | Stable failure reason for retries and support. |
| `TenantId` | `Guid` | Tenant ownership. |

Extend `Thread`:

| Field | Type | Reason |
|---|---|---|
| `ChannelConnectionId` | `Guid` | Connected social account that owns the thread. |
| `Provider` | `BroadcastProviderKind` | Provider source for the thread. |
| `ExternalThreadUrl` | `string?` | Link to the post, video, live, lesson, campaign, or community thread. |
| `ExternalThreadId` | `string` | Provider id for the post, video, live, lesson, campaign, or community thread. |
| `CampaignLabel` | `string?` | Launch, live, lesson, or campaign grouping label. |
| `LastSyncedAtUtc` | `DateTime?` | Last time comments were synchronized for this thread. |
| `ClosedAtUtc` | `DateTime?` | Time the thread was completed. |

Extend `Item`:

| Field | Type | Reason |
|---|---|---|
| `ChannelConnectionId` | `Guid` | Connected account that captured the item. |
| `ChannelEventId` | `Guid?` | Inbound event that created or last updated the item. |
| `ExternalItemId` | `string` | Provider item/comment id. |
| `ExternalAuthorId` | `string?` | Provider author id when available. |
| `ExternalAuthorLabel` | `string?` | Public author label or handle from the provider. |
| `ExternalPermalink` | `string?` | Link to the source comment/message when available. |
| `Classification` | `BroadcastItemClassification` | Question, objection, praise, complaint, suggestion, spam, or other. |
| `IgnoreReason` | `string?` | Optional reason when item is ignored as spam/noise. |
| `ClusterId` | `Guid?` | Group assignment for recurring themes. |
| `SuggestedResponseId` | `Guid?` | Optional response draft reference inside Broadcast. |
| `ProcessedAtUtc` | `DateTime?` | Time AI processing classified the item. |

Add `ChannelSyncCursor` for providers that require polling:

| Field | Type | Reason |
|---|---|---|
| `ChannelConnectionId` | `Guid` | Connected account being synchronized. |
| `ScopeKind` | `ChannelSyncScopeKind` | Account, thread discovery, thread comments, or direct messages. |
| `ExternalResourceId` | `string?` | Provider resource id for scoped cursors. |
| `CursorValue` | `string?` | Provider cursor, since id, timestamp, or page token. |
| `LastSuccessfulSyncAtUtc` | `DateTime?` | Last completed synchronization. |
| `NextSyncAfterUtc` | `DateTime?` | Backoff-aware next polling time. |
| `FailureCount` | `int` | Consecutive provider failures. |
| `TenantId` | `Guid` | Tenant ownership. |

Add `BroadcastOutboundReply` for channel write-back:

| Field | Type | Reason |
|---|---|---|
| `ChannelConnectionId` | `Guid` | Connected account used for delivery. |
| `ThreadId` | `Guid` | Parent thread. |
| `ItemId` | `Guid?` | Specific comment/message being answered, when applicable. |
| `ItemClusterId` | `Guid?` | Cluster the response answers, when generated for a theme. |
| `ResponseDraftId` | `Guid?` | Draft selected for delivery. |
| `Provider` | `BroadcastProviderKind` | Provider receiving the reply. |
| `Body` | `string` | Outbound reply body. |
| `Status` | `OutboundReplyStatus` | Pending policy check, queued, sending, sent, blocked, failed, or canceled. |
| `ExternalReplyId` | `string?` | Provider id returned after successful write. |
| `RequestedAtUtc` | `DateTime` | Time delivery was requested by product flow. |
| `SentAtUtc` | `DateTime?` | Time provider accepted the reply. |
| `ErrorMessage` | `string?` | Stable failure reason for retry/support. |
| `TenantId` | `Guid` | Tenant ownership. |

Add `ItemCluster` for recurring themes:

| Field | Type | Reason |
|---|---|---|
| `ThreadId` | `Guid` | Parent thread. |
| `Title` | `string` | Human-readable cluster theme. |
| `Summary` | `string?` | AI summary of repeated audience question. |
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
| `Status` | `ResponseDraftStatus` | Draft, allowed, queued, sent, dismissed, failed, or promoted. Reuse a common enum only if the lifecycle is identical to Direct response drafts. |
| `SourceQuestionId` | `Guid?` | QnA question used as evidence, external reference only. |
| `SourceAnswerId` | `Guid?` | QnA answer used as evidence, external reference only. |
| `GeneratedAtUtc` | `DateTime` | Domain timestamp for model output. |
| `QueuedForDeliveryAtUtc` | `DateTime?` | Time the draft created an outbound reply. |
| `TenantId` | `Guid` | Tenant ownership. |

### Trust Entity Changes

Add `Querify.Models.Trust` before adding Trust DTOs or enums. Then add entities in
`Querify.Trust.Common.Persistence.TrustDb`.

Add `PolicyEvaluation`:

| Field | Type | Reason |
|---|---|---|
| `TargetModule` | `ModuleEnum` | QnA, Direct, or Broadcast asset being evaluated. |
| `TargetKind` | `PolicyTargetKind` | QnA answer, QnA question, Direct response draft, Broadcast response draft, or outbound reply. |
| `TargetId` | `Guid` | External target id. No EF navigation across module databases. |
| `Status` | `PolicyEvaluationStatus` | Pending, allowed, blocked, stale, failed, or canceled. |
| `RiskCategory` | `PolicyRiskCategory` | Pricing, promise, guarantee, health, finance, legal, partnership, community rule, discount, or other. |
| `RuleKey` | `string?` | Policy rule that produced the decision. |
| `Reason` | `string?` | Why the policy allowed or blocked the content. |
| `ContentHash` | `string` | Hash of evaluated content to detect stale decisions after edits. |
| `ContentSnapshotJson` | `string?` | Minimal evaluated content snapshot for decision history. |
| `RequestedByUserId` | `Guid?` | User or system actor that requested evaluation, when known. |
| `RequestedAtUtc` | `DateTime` | Domain timestamp for policy evaluation request. |
| `ResolvedAtUtc` | `DateTime?` | Time final policy decision was recorded. |
| `TenantId` | `Guid` | Tenant ownership. |

Add `PolicyDecision`:

| Field | Type | Reason |
|---|---|---|
| `PolicyEvaluationId` | `Guid` | Parent policy evaluation. |
| `Decision` | `PolicyDecisionKind` | Allowed, blocked, stale, or failed. |
| `RuleKey` | `string?` | Policy rule that made the decision. |
| `Rationale` | `string?` | Short machine-readable or user-facing rationale. |
| `DecidedByUserId` | `Guid?` | User identity when an authenticated action requested the evaluation; otherwise null for system decisions. |
| `DecidedAtUtc` | `DateTime` | Domain timestamp for decision. |
| `ContentHash` | `string` | Content hash allowed or blocked by this decision. |
| `TenantId` | `Guid` | Tenant ownership. |

Use Trust as the blocking source of truth:

- QnA `ActivateAnswer` checks Trust when the answer is sensitive or has a policy evaluation.
- Channel outbound reply delivery checks Trust before writing to a provider.
- Trust policy state does not become a QnA or Broadcast boolean.
- If answer content changes after a policy decision, mark the Trust evaluation stale through a QnA event or an
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
    "enabledSocialProviders": ["instagram", "tiktok", "youtube"],
    "socialChannelConnections": 3,
    "broadcastCapturedItemsPerMonth": 100,
    "broadcastOutboundRepliesPerMonth": 100,
    "aiSuggestionsPerMonth": 100,
    "mcpEnabled": true,
    "multiAgentRuntimeEnabled": true,
    "enterpriseVectorSearchEnabled": true,
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
| `Querify.Common.Infrastructure.Retrieval` | Enterprise retrieval stage | Hybrid lexical/vector search abstractions, vector index adapter, re-ranking, evidence shaping, and query telemetry. |
| `Querify.Common.Infrastructure.Agents` | Multi-agent stage | Agent orchestration, tool execution boundaries, run telemetry, guardrails, and module-scoped agent policies. |
| `Querify.Common.Infrastructure.ChannelIntegrations` | Social channel stage | Provider-neutral OAuth/API clients, webhook validation, incremental read, outbound write, token refresh, and provider retry policy. |
| `Querify.MCP.Server` | MCP stage | Native .NET MCP server exposing QnA, Direct, Broadcast, Trust, and Tenant tools/prompts. |

`Querify.Common.Infrastructure.Ai` should expose abstractions such as:

- `IAiTextGenerationService`
- `IAiAnswerSuggestionService`
- `IAiClassificationService`
- `IAiEmbeddingService`
- `IAgentRuntimeService`

Keep prompt templates close to the owning feature when they encode product behavior. Keep AI
provider clients and retry/timeout configuration in the shared AI infrastructure project. Keep
social provider clients, token handling, webhook signature validation, and provider write/read
contracts in the channel integration infrastructure project. Keep retrieval index details behind
`IQnARetrievalService` so Direct, Broadcast, Trust, MCP tools, and agents do not know whether the
MVP uses PostgreSQL/pgvector or a dedicated vector search service.

### Direct Projects

| Project | Purpose |
|---|---|
| `Querify.Direct.Public.Api` | Public Ask Me form ingress. |
| `Querify.Direct.Public.Business.Ask` | Public ask-form command that creates a conversation and first message. |
| `Querify.Direct.Portal.Api` | Authenticated creator inbox APIs. |
| `Querify.Direct.Portal.Business.Conversation` | Inbox list/detail, channel reply, delivery status, resolve, create QnA gap, suggest answer. |
| `Querify.Direct.Portal.Test.IntegrationTests` | Direct Portal API and command/query coverage. |
| `Querify.Direct.Public.Test.IntegrationTests` | Public form tenant resolution and conversation creation coverage. |

Add `Querify.Direct.Worker.Api` when Instagram, TikTok, or YouTube Direct-message capabilities are
enabled for the MVP scope and require durable inbound or outbound delivery. Public ask-form
submission can remain synchronous, but provider delivery should use the same retry/idempotency
expectations as Broadcast.

### Broadcast Projects

| Project | Purpose |
|---|---|
| `Querify.Broadcast.Portal.Api` | Authenticated comment collector APIs. |
| `Querify.Broadcast.Integration.Api` | Provider OAuth callbacks, webhook verification, and inbound webhook endpoints. |
| `Querify.Common.Infrastructure.ChannelIntegrations.Instagram` or adapter folder | Instagram OAuth/API, webhook validation, incremental read, and reply writer. |
| `Querify.Common.Infrastructure.ChannelIntegrations.TikTok` or adapter folder | TikTok OAuth/API, webhook validation, incremental read, and reply writer. |
| `Querify.Common.Infrastructure.ChannelIntegrations.YouTube` or adapter folder | YouTube OAuth/API, webhook validation, incremental read, and reply writer. |
| `Querify.Broadcast.Portal.Business.ChannelConnection` | Start connection, complete connection, list connection health, disconnect, and reconnect flows. |
| `Querify.Broadcast.Portal.Business.Thread` | Thread list/detail, item list/detail, cluster list, response draft and outbound reply status queries. |
| `Querify.Broadcast.Worker.Api` | Durable processing for channel events, polling cursors, classification, clustering, and outbound replies. |
| `Querify.Broadcast.Worker.Business.ChannelEvent` | Normalize provider events, create/update threads and items, and maintain idempotency. |
| `Querify.Broadcast.Worker.Business.ChannelSync` | Poll providers when webhook coverage is unavailable or incomplete. |
| `Querify.Broadcast.Worker.Business.OutboundReply` | Apply Trust/policy gate and send replies through the provider client. |
| `Querify.Broadcast.Portal.Test.IntegrationTests` | Connection, thread, item, cluster, and reply-status command/query coverage. |
| `Querify.Broadcast.Worker.Test.IntegrationTests` | Webhook, polling, idempotency, retry/failure, classification, clustering, and outbound delivery coverage. |

Broadcast should have a worker because provider event processing, classification, clustering,
token refresh, polling, and outbound write-back need retries and idempotency outside normal request
latency.

### Trust Projects

| Project | Purpose |
|---|---|
| `Querify.Trust.Portal.Api` | Authenticated policy evaluation and decision APIs. |
| `Querify.Trust.Portal.Business.Policy` | Evaluate target content, record allow/block decisions, list decisions, and mark stale. |
| `Querify.Trust.Portal.Test.IntegrationTests` | Policy evaluation, decision history, stale decision, and activation/write-back guard coverage. |

Do not add a Trust worker in the MVP unless stale-policy reconciliation, scheduled expiration, or
notification delivery becomes a real requirement.

### MCP And Multi-Agent Projects

| Project | Purpose |
|---|---|
| `Querify.MCP.Server` | Native MCP server with stdio/SSE transports, tenant context, tools, and prompts. |
| `Querify.MCP.Server.Test.IntegrationTests` | Tool authorization, tenant isolation, command/query dispatch, and JSON-RPC contract coverage. |
| `Querify.Common.Infrastructure.Agents` | Module-scoped agent runtime, tool policies, guardrails, run telemetry, and handoff contracts. |
| `Querify.Common.Infrastructure.Retrieval` | Hybrid retrieval, vector index adapter, re-ranking, citation/evidence shaping, and retrieval telemetry. |

The complete MVP tool surface must include `qna_*`, `direct_*`, `broadcast_*`, `trust_*`, and
`tenant_*` tools. The complete MVP prompt surface must include QnA, Direct, Broadcast, Trust, and
Tenant agents. QnA search is shared across agents and must be backed by enterprise retrieval before
agents can send or suggest channel responses.

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
| `BroadcastChannelConnectedIntegrationEvent` | Broadcast Portal API or Integration API | Broadcast Worker | Start provider health check, initial thread discovery, and cursor setup. |
| `BroadcastChannelEventReceivedIntegrationEvent` | Broadcast Integration API or Channel Sync worker | Broadcast Worker | Normalize and persist an inbound provider event. |
| `BroadcastItemClassifiedIntegrationEvent` | Broadcast Worker | Broadcast clustering worker/service | Group recurring audience questions after item classification. |
| `BroadcastOutboundReplyRequestedIntegrationEvent` | Broadcast Worker or Trust policy gate | Broadcast Worker | Send a reply through the connected provider. |
| `BroadcastOutboundReplyDeliveredIntegrationEvent` | Broadcast Worker | Broadcast Portal notification service, optional | Notify Portal that provider write-back succeeded. |
| `BroadcastOutboundReplyFailedIntegrationEvent` | Broadcast Worker | Broadcast Portal notification service, optional | Notify Portal that provider write-back failed or needs reconnection. |
| `QnAAnswerContentChangedIntegrationEvent` | QnA Portal Answer command | Trust policy feature | Mark matching policy decision stale when evaluated content changes. |
| `TrustPolicyDecisionRecordedIntegrationEvent` | Trust Portal API | QnA/Broadcast notification services, optional | Notify answer or outbound reply screens that policy state changed. |

### Optional Events

| Event | Use when |
|---|---|
| `DirectQuestionSubmittedIntegrationEvent` | The public ask form should send notification email, trigger async suggestion generation, or feed analytics. |
| `DirectChannelMessageReceivedIntegrationEvent` | The selected provider supports private messages and Direct owns the conversation state. |
| `DirectResponseDraftCreatedIntegrationEvent` | Saved Direct suggestions need Portal real-time notification or provider write-back. |
| `DirectOutboundReplyDeliveredIntegrationEvent` | Direct provider delivery needs notification or analytics. |
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

### Add Broadcast Worker For Channel Integration

Recommended pipeline:

```text
Portal starts provider connection
  -> Broadcast Portal command creates ChannelConnection(Pending)
  -> Provider OAuth/API callback completes ChannelConnection(Connected)
  -> Broadcast publishes BroadcastChannelConnectedIntegrationEvent
  -> Broadcast Worker discovers threads and creates ChannelSyncCursor records
Provider webhook or polling returns a comment/message
  -> Broadcast Integration API or Channel Sync worker creates ChannelEvent(Received)
  -> Broadcast publishes BroadcastChannelEventReceivedIntegrationEvent
  -> Broadcast Worker normalizes event, creates/updates Thread and Item, classifies, clusters, and creates response draft
  -> Trust/policy gate allows or blocks delivery
  -> allowed response creates BroadcastOutboundReply(Queued)
  -> Broadcast Worker writes reply through provider client
  -> worker marks outbound reply Sent or Failed and stores ExternalReplyId when available
```

Add a low-frequency reconciliation hosted service only if lost broker messages are a practical risk:

```text
BroadcastChannelReconciliationHostedService
  -> BroadcastChannelReconciliationProcessorService
  -> RequeueStaleChannelEventsCommand
  -> RequeueStaleOutboundRepliesCommand
```

The hosted service and processor service must not call providers, call AI, or update item/reply
state directly.

### Direct Worker Trigger

Direct can be synchronous at MVP scale:

- public form creates a conversation and first message;
- Portal inbox query lists conversations;
- suggestion endpoint reads QnA through enterprise retrieval and calls AI on demand;
- response endpoint records and queues provider delivery when Instagram, TikTok, or YouTube
  supports the required Direct capability;
- creator promotes a question to QnA draft.

Add `Querify.Direct.Worker.Api` only if notification delivery, async suggestion generation, provider
DM listening, or provider reply sending becomes durable product behavior in the selected MVP
providers.

### Avoid Trust Worker In MVP

Trust policy evaluation can be request-driven:

- evaluate target content;
- list policy decisions;
- mark stale decisions;
- QnA activation and channel write-back check policy state.

Add a Trust worker only when stale-policy reconciliation, scheduled expiration, or notification
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
| Search QnA | New `api/qna/search` backed by enterprise retrieval, lexical + vector index, filters, and re-ranking |
| Reindex source chunks | New Portal/worker command surface for chunk extraction, embeddings, and vector index updates |
| Activate answer | Existing `api/qna/answer/{id}/activate`, with Trust guard added |

### QnA Public

Use current public QnA endpoints first. Add a public hub endpoint only if the frontend needs one
round trip:

```text
GET api/qna/creator-hub/{spaceSlug}
GET api/qna/creator-hub/{spaceSlug}/question
GET api/qna/creator-hub/{spaceSlug}/question/{questionSlugOrId}
GET api/qna/search?q={query}
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
POST   api/broadcast/channel-connection/start
GET    api/broadcast/channel-connection
GET    api/broadcast/channel-connection/{id}
POST   api/broadcast/channel-connection/{id}/reconnect
POST   api/broadcast/channel-connection/{id}/disconnect
GET    api/broadcast/thread
GET    api/broadcast/thread/{id}
GET    api/broadcast/thread/{id}/item
GET    api/broadcast/thread/{id}/cluster
GET    api/broadcast/outbound-reply
GET    api/broadcast/outbound-reply/{id}
POST   api/broadcast/cluster/{id}/promote-to-qna
```

### Broadcast Integration

```text
GET    api/broadcast/integration/{provider}/oauth/callback
POST   api/broadcast/webhook/{provider}
```

The webhook endpoint validates provider signatures, stores `ChannelEvent`, returns provider-required
acknowledgements quickly, and leaves normalization/classification/write-back to the worker.

### MCP Server

```text
stdio Querify.MCP.Server
GET/POST /mcp when hosted over HTTP/SSE
```

The MVP MCP surface must expose QnA, Direct, Broadcast, Trust, and Tenant tool groups plus matching
module prompts. MCP tools call module commands/queries through MediatR and must use the same tenant,
policy, retrieval, and entitlement rules as HTTP and worker flows.

### Trust Portal

```text
POST   api/trust/policy/evaluate
GET    api/trust/policy/decision
GET    api/trust/policy/decision/{id}
POST   api/trust/policy/decision/{id}/mark-stale
GET    api/trust/policy/decision/by-target
```

`by-target` lets QnA answer and Broadcast outbound reply screens show policy state for a specific
target without loading the whole decision history.

## AI Service Placement

AI is a shared technical capability, but the business decision remains in the owning module command.

| AI use case | Owning command | Shared service |
|---|---|---|
| Generate QnA answer draft from captured content or URL | QnA Answer or Source command | Text generation |
| Search and ground answers from QnA | QnA Search query or `IQnARetrievalService` caller | Enterprise retrieval, embeddings, lexical search, re-ranking |
| Suggest Direct response from QnA | Direct Conversation command or Direct agent | Answer suggestion plus enterprise retrieval |
| Classify Broadcast comments | Broadcast ChannelEvent command | Classification |
| Cluster Broadcast questions | Broadcast ChannelEvent command or Broadcast agent | Embeddings, vector similarity, or text grouping |
| Generate outbound Broadcast reply | Broadcast OutboundReply command or Broadcast agent | Answer suggestion, enterprise retrieval, and text generation |
| Summarize QnA gap from Direct/Broadcast | Direct/Broadcast promote command | Summarization |
| Detect sensitive answer categories | Trust policy evaluation command or QnA preflight command | Classification |

Keep generated output traceable:

- store the final generated text in the owning draft entity;
- store confidence and source QnA ids when a response is suggested from existing knowledge;
- store retrieval evidence ids and citation metadata used by agents;
- do not store provider prompts or raw model metadata unless needed for debugging or compliance;
- enforce AI usage limits through Tenant entitlements before model calls.

## Cross-Module Flows

### Private Question To FAQ

```text
Direct connected channel or Direct Public Ask
  -> creates Direct Conversation + ConversationMessage
Direct agent searches QnA through enterprise retrieval
  -> AI creates Direct response draft
If sensitive
  -> Trust PolicyEvaluation is created
Trust/policy gate allows or blocks delivery
  -> Direct queues provider reply when a connected write channel exists
  -> Direct stores delivery state and optional PromotedQuestionId/PromotedAnswerId
```

### Comments To Content

```text
Creator connects Instagram, TikTok, and YouTube
  -> Broadcast ChannelConnection(Connected) for each provider account
Provider webhook or polling captures comment
  -> ChannelEvent(Received)
Broadcast Worker processes event
  -> Thread and Item created or updated
  -> Items classified
  -> ItemClusters created
  -> BroadcastResponseDraft created
If sensitive
  -> Trust PolicyEvaluation is created
Trust/policy gate allows or blocks delivery
  -> BroadcastOutboundReply writes response through provider
Recurring cluster may promote
  -> QnA draft Question/Answer created
```

### Public Hub Reduces Repetition

```text
Creator publishes QnA Space as public Creator Hub
Follower searches public QnA
  -> QnA Public API serves active public questions/answers
Follower submits feedback
  -> QnA public feedback activity is recorded
Follower asks privately
  -> Direct connected channel or Direct Public Ask creates conversation
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

Schema migration handoff:

- none expected.

### Stage 2: Enterprise Retrieval, Native MCP, And Multi-Agent Runtime

Scope:

- add enterprise QnA retrieval before any agent can answer from knowledge;
- add source chunking, embeddings, vector index writes, lexical search, filters, and re-ranking;
- add native `Querify.MCP.Server`;
- add complete multi-agent runtime foundations, prompts, tool policies, and tenant-aware execution;
- expose initial QnA and Tenant tools, then keep Direct/Broadcast/Trust tools on the same MCP
  contract as their module APIs land.

Backend:

- add `Querify.Common.Infrastructure.Retrieval`;
- add `Querify.Common.Infrastructure.Agents`;
- add `Querify.MCP.Server`;
- add `Querify.QnA.Public.Business.Search`;
- add `Querify.QnA.Portal.Business.Search`;
- add `Querify.QnA.Worker.Business.Ingestion` chunking/reindexing commands;
- add MCP and retrieval integration test projects.

Schema migration handoff:

- add vector index storage or external index provisioning;
- add source chunk/evidence tables if chunks are persisted in QnA;
- add indexes for tenant, space, visibility, status, language, source, and vector lookup;
- add Tenant entitlement keys for MCP, agents, retrieval, and provider access.

### Stage 3: Direct Ask Me Inbox

Scope:

- extend Direct model;
- add Direct Public and Portal APIs;
- create conversations from public ask form;
- list, reply through configured channel, track delivery, and resolve in Portal.
- use enterprise retrieval and Direct agent orchestration for suggestions.

Backend:

- update `Querify.Models.Direct`;
- update `Querify.Direct.Common.Persistence.DirectDb`;
- add `Querify.Direct.Public.Api`;
- add `Querify.Direct.Public.Business.Ask`;
- add `Querify.Direct.Portal.Api`;
- add `Querify.Direct.Portal.Business.Conversation`;
- add Direct MCP tools and Direct agent prompt;
- add Direct integration test projects.

Schema migration handoff:

- add/alter Direct tables and indexes for new fields/status values;
- add Direct module connection setup for tenants if not already seeded.

### Stage 4: AI Drafts And Direct Suggestions

Scope:

- add shared AI infrastructure;
- generate QnA draft from captured content or URL;
- suggest Direct response from active QnA answers through enterprise retrieval;
- promote Direct conversation to QnA draft.

Backend:

- add `Querify.Common.Infrastructure.Ai`;
- extend QnA Answer/Source commands;
- extend Direct Conversation commands;
- extend MCP tools for source import, QnA search, Direct suggestion, and Direct promotion;
- enforce AI usage limits through Tenant entitlements.

Schema migration handoff:

- only required if saved Direct suggestion drafts are introduced.

### Stage 5: Automatic Broadcast Channel Integration

Scope:

- extend Broadcast model;
- add Broadcast Portal and Integration APIs;
- add Instagram, TikTok, and YouTube provider connection setup, webhook receivers, polling cursors,
  raw channel event persistence, item clustering, and outbound reply delivery;
- add Broadcast Worker;
- classify, group, and respond to captured comments.
- use Broadcast agent orchestration for retrieval, clustering, policy checks, and routing.

Backend:

- update `Querify.Models.Broadcast`;
- update `Querify.Broadcast.Common.Persistence.BroadcastDb`;
- add `Querify.Common.Infrastructure.ChannelIntegrations`;
- add Instagram, TikTok, and YouTube adapters behind the provider-neutral interface;
- add `Querify.Broadcast.Portal.Api`;
- add `Querify.Broadcast.Integration.Api`;
- add `Querify.Broadcast.Portal.Business.ChannelConnection`;
- add `Querify.Broadcast.Portal.Business.Thread`;
- add `Querify.Broadcast.Worker.Api`;
- add `Querify.Broadcast.Worker.Business.ChannelEvent`;
- add `Querify.Broadcast.Worker.Business.ChannelSync`;
- add `Querify.Broadcast.Worker.Business.OutboundReply`;
- add Broadcast MCP tools and Broadcast agent prompt;
- add Broadcast integration test projects.

Schema migration handoff:

- add Broadcast channel connection/event/cursor/provider/classification/cluster/response-draft/outbound-reply tables and indexes for Instagram, TikTok, and YouTube;
- add Broadcast worker runtime configuration.

### Stage 6: Trust Policy Log

Scope:

- add Trust contracts;
- add policy evaluation and decision entities;
- add Trust Portal API;
- block sensitive QnA answer activation and outbound channel replies without current allowed policy decision;
- mark policy decisions stale when evaluated content changes.

Backend:

- add `Querify.Models.Trust`;
- update `Querify.Trust.Common.Persistence.TrustDb`;
- add `Querify.Trust.Portal.Api`;
- add `Querify.Trust.Portal.Business.Policy`;
- add Trust MCP tools and Trust agent prompt;
- update QnA Answer activation command;
- add Trust, QnA, and Broadcast integration tests for policy guards.

Schema migration handoff:

- create Trust policy evaluation and decision tables and indexes;
- add Trust module connection setup for tenants if not already seeded.

### Stage 7: Pricing, Limits, And Usage

Scope:

- encode Creator Starter/Growth/Pro limits;
- enforce QnA, Direct, Broadcast, AI, MCP, agents, retrieval, provider, user, and space limits;
- expose basic usage summary.

Backend:

- update Tenant entitlement synchronizer to populate `FeatureJson`;
- add a shared entitlement/limit service if one does not exist;
- optionally add `TenantUsageCounter` when usage must be queryable and reset monthly;
- add limit checks to module commands before writes or AI calls.

Schema migration handoff:

- none if using `FeatureJson`;
- required if adding `TenantUsageCounter`.

## Indexing And Query Notes

Recommended indexes once schema work is explicitly requested:

| Module | Index |
|---|---|
| QnA/Retrieval | `TenantId, SpaceId, Visibility, Status, Language` for scoped lexical/vector retrieval |
| QnA/Retrieval | Vector index on chunk embeddings for semantic search |
| QnA/Retrieval | `TenantId, SourceId, ContentHash` for reindex idempotency |
| Direct | `TenantId, Status, CreatedDate` for inbox list |
| Direct | `TenantId, PromotedQuestionId` when tracing promoted QnA gaps |
| Broadcast | `TenantId, Status, CreatedDate` for thread list |
| Broadcast | `TenantId, ThreadId, Classification` for item filters |
| Broadcast | `TenantId, ThreadId, ItemCount` for cluster ranking |
| Broadcast | `TenantId, ChannelConnectionId, Status` for connection health and thread filtering |
| Broadcast | `TenantId, ChannelConnectionId, IdempotencyKey` unique index for inbound event idempotency |
| Broadcast | `TenantId, ChannelConnectionId, NextSyncAfterUtc` for polling cursors |
| Broadcast | `TenantId, Status, RequestedAtUtc` for outbound reply retries |
| Trust | `TenantId, Status, CreatedDate` for policy decision history |
| Trust | `TenantId, TargetModule, TargetKind, TargetId` for activation and write-back guards |
| Tenant | `TenantId, FeatureKey, PeriodStartUtc` if `TenantUsageCounter` is added |

## Portal Domains To Add

Add frontend domains in the same staged order as backend APIs:

| Domain | Purpose |
|---|---|
| `creator` | Top-level Creator Hub workspace page that composes QnA, Direct, Broadcast, and Trust summaries. |
| `direct` | Ask Me inbox list/detail/reply/resolve/suggest/promote flows. |
| `broadcast` | Channel connections, thread sync, items, clusters, response drafts, and outbound reply status. |
| `trust` | Policy decision history and stale-decision screens. |
| `mcp` | MCP server status, tenant tool access, prompt/tool configuration, and test invocation diagnostics. |
| `agents` | Multi-agent runtime status, runs, handoffs, guardrail decisions, and failed tool calls. |
| `usage` or billing extension | Plan limits and monthly usage. |

The Portal should use tabs inside the Creator Hub area:

- Answers;
- Inbox;
- Comments;
- Policy.
- Agents;
- MCP.

Keep each tab backed by its owning module API. Do not create a frontend type that merges module
contracts into one large Creator DTO unless it is a read-only dashboard query.

## Testing Strategy

Add tests per stage:

| Stage | Test focus |
|---|---|
| QnA Creator Hub | public active space/question/answer visibility, creator seed data, answer activation behavior unchanged |
| Enterprise retrieval | chunking, embedding writes, lexical/vector hybrid search, filters, re-ranking, evidence ids, tenant isolation |
| MCP and agents | native MCP tool contracts, prompts, tenant context, tool authorization, agent handoffs, guardrail failures, run telemetry |
| Direct | public ask form creates conversation/message, tenant isolation, inbox filters, channel reply status, resolve transitions, promote-to-QnA |
| AI suggestions | entitlement checks before model calls, source answer ids preserved, no suggestion when no eligible QnA answer exists |
| Broadcast | Instagram/TikTok/YouTube connection state, webhook validation, polling cursor behavior, event idempotency, item validation, classification, cluster generation, outbound reply delivery, failure/retry behavior |
| Trust | policy evaluation lifecycle, decision history, stale decision on content change, QnA activation and channel write-back blocked without allowed policy decision |
| Pricing/limits | limit enforcement across QnA, Direct, Broadcast, AI, MCP, agents, retrieval, providers, spaces, and users |

Keep architecture tests updated when new API, worker, business, model, or persistence project patterns
are introduced.

## Schema Migration Handoff

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
2. Add enterprise retrieval before Direct, Broadcast, MCP, or agents use QnA as grounding.
3. Add native `Querify.MCP.Server` and a complete module-scoped multi-agent runtime in the MVP.
4. Add Direct APIs and extend Direct persistence for the private Ask Me Inbox.
5. Add Broadcast Portal/Integration APIs and a Broadcast Worker for Instagram, TikTok, and YouTube
   connection setup, webhook/polling ingestion, classification, clustering, and outbound write-back.
6. Add real Trust contracts/entities/business only when implementing policy log behavior.
7. Use Trust as the policy source of truth; do not copy Trust state into QnA answers or Broadcast replies.
8. Use Tenant entitlements for Creator plan limits and keep module commands responsible for checking
   those limits before creating records or calling AI.
9. Use MassTransit events only for cross-module or async work. Keep request-time behavior in the
   owning module command/query path.
10. Keep request-time behavior synchronous where user latency allows it; use workers for durable,
   retryable provider processing such as webhook normalization, polling, classification, clustering,
   token refresh, and outbound replies.
