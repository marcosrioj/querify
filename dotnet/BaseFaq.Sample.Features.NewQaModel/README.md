# BaseFaq.Sample.Features.NewQaModel

## Purpose

This project is the focused question-and-answer domain sample for BaseFAQ.

It is intentionally not a mirror of the legacy implementation. It models the new product posture directly as a question-and-answer platform, with English domain names and a small, focused entity set.

## Migration posture

This sample is the domain target, not a prescription to replace the repository-wide EF Core infrastructure.

When this model replaces the current FAQ persistence, the preferred production posture is:

- keep `BaseEntity`, `AuditableEntity`, and `IMustHaveTenant`
- keep `BaseDbContext` tenant filters, soft-delete rules, audit stamping, and tenant indexes
- map the responsibility shown here by `DomainEntity` onto those existing shared abstractions instead of introducing a second persistence base hierarchy
- preserve the security behavior of the sample through write-side handlers, domain services, validators, EF configuration, and database constraints where appropriate

In other words:

- `DomainEntity` explains the domain responsibility of shared tenant and audit fields inside the sample project
- the future production implementation should usually continue to use the existing shared EF base classes from the solution

## Design principles

The sample follows five rules:

1. model questions as living threads, not static content blocks
2. treat answers as governed candidates that can be accepted, validated, or retired
3. keep provenance first-class through explicit source links
4. keep taxonomy simple and reusable
5. keep audit and revision behavior append-only through a single activity journal instead of many specialized history tables

## Security posture for migration

The future production migration should preserve these controls even if the final entities remain anemic and continue to use public setters:

- tenant query filters are necessary but not sufficient; cross-tenant attachments must also be rejected on the write side
- public exposure should stay fail-closed for questions, answers, sources, and citations
- source citation and excerpt reuse should require explicit source-level approval, not only a link role
- AI drafts should not become public or official without the expected review and validation path
- thread activity should be treated as append-only operational history, even if the persistence layer still inherits from `AuditableEntity`
- JSON, URL, and similar flexible fields should be validated before persistence, even if that validation lives in handlers or validators rather than entity setters

## Entity overview

### `QuestionSpace`

`QuestionSpace` is the top-level surface where questions live.

Business role:

- groups a coherent body of questions and answers
- defines whether the space is curated, community-driven, or hybrid
- defines visibility and moderation defaults
- defines whether the space accepts new questions and answers
- carries product and journey boundaries for large installations

Why it exists:

- the platform needs something broader than a static page
- a single tenant may run multiple spaces for billing, setup, product areas, or internal operations

### `Question`

`Question` is the main thread aggregate.

Business role:

- stores the actual user-facing question
- stores thread context, ingestion origin, and routing key
- tracks lifecycle from draft or intake through answer, validation, escalation, duplicate handling, and archive
- points to the accepted answer when one exists
- supports contextual segmentation by language, product scope, journey, audience, and context key

Why it exists:

- the new product is built around the loop "question arrives, question is resolved, question becomes reusable knowledge"

### `Answer`

`Answer` represents an answer candidate or answer variant.

Business role:

- stores short and long answer forms
- separates origin from lifecycle through `AnswerKind` and `AnswerStatus`
- supports official answers, community answers, imported answers, AI drafts, and AI-assisted answers
- supports contextual variants through `ContextKey` and `ApplicabilityRulesJson`
- tracks whether the answer is accepted, canonical, official, published, validated, or retired

Why it exists:

- a serious Q&A platform cannot assume one question always has one static answer
- accepted answer and canonical answer are related but not always identical concepts

### `KnowledgeSource`

`KnowledgeSource` is the reference record for the materials that feed the system.

Business role:

- represents articles, web pages, tickets, videos, repositories, community threads, social comments, internal notes, and more
- stores external system identity and locator
- stores verification state and authoritative-source signal
- keeps the raw metadata hook needed for future connectors without redesigning the schema

Why it exists:

- provenance is central to trust, moderation, AI safety, and future enterprise features

### `Tag`

`Tag` is the lightweight classification entity.

Business role:

- classifies spaces and questions
- supports coarse and fine-grained organization
- keeps product, journey, plan, version, or integration labels reusable

Why it exists:

- the platform needs reusable taxonomy, but it does not need a heavy ontology to start

### `QuestionSourceLink`

`QuestionSourceLink` ties a question to its origin or contextual evidence.

Business role:

- explains where the question came from
- supports imported threads and duplicate reasoning
- stores excerpts, scope, primacy, order, and confidence
- blocks public excerpt reuse unless the linked source explicitly allows it

Why it exists:

- the source that created a question is not always the same source that proves the best answer

### `AnswerSourceLink`

`AnswerSourceLink` ties an answer to its evidence.

Business role:

- supports citations and moderation review
- stores excerpts, scope, primacy, order, and confidence
- distinguishes supporting context from direct evidence through `SourceRole`
- blocks public citation and excerpt reuse unless the linked source is explicitly marked safe for it

Why it exists:

- answer trust cannot be left implicit, especially once AI-assisted content enters the workflow

### `ThreadActivity`

`ThreadActivity` is the append-only operational journal for the question thread.

Business role:

- records workflow events such as creation, approval, rejection, escalation, publication, acceptance, validation, feedback, and voting
- records who acted and when
- optionally stores a serialized snapshot to preserve revision history without adding separate revision entities
- keeps the event body immutable after creation so the journal stays append-only in implementation, not only in documentation

Why it exists:

- this keeps the model clean while still supporting moderation, audit, and future replay or timeline views
- it is the main simplification choice in the sample: one journal instead of many specialized history classes

## Why the sample stays small

The sample does not create dedicated entities for every possible future concern.

What was intentionally simplified:

- revision history is handled through `ThreadActivity` snapshots
- tag assignment uses direct collections instead of explicit join entities in the sample
- community reputation, moderation queues, and analytics are not modeled as standalone entities yet
- collection-level source metadata is kept simple through direct `CuratedSources`

This keeps the sample architectural, not bloated.

## What the sample already supports

Even with a small set of entities, the model already supports:

- public and internal Q&A spaces
- internal-by-default visibility for spaces, questions, answers, and sources
- moderated intake from widget, API, help center, tickets, social, or imports
- accepted answers
- canonical answers
- contextual answer variants
- evidence-backed answers
- explicit source verification and public citation/excerpt controls
- duplicate question handling
- append-only workflow history
- future connector and AI expansion without renaming the core domain

## Project-local documentation

The detailed project documentation now lives beside the sample itself:

- [domain-model-reference.md](./domain-model-reference.md)
- [Flows/README.md](./Flows/README.md)

Those documents explain the full domain model and the operating flows for space governance, question intake, answer production, moderation, provenance, duplicate handling, discovery, and audit.

The solution-wide transition backlog that uses this sample as the target now also lives inside the project:

- [basefaq-faq-solution-qna-upgrade-plan.md](./basefaq-faq-solution-qna-upgrade-plan.md)

## Files

- [Domain/DomainEntity.cs](./Domain/DomainEntity.cs)
- `Domain/Enums/` contains one file per enum used by the sample domain.
- [domain-model-reference.md](./domain-model-reference.md)
- [Flows/README.md](./Flows/README.md)
- [basefaq-faq-solution-qna-upgrade-plan.md](./basefaq-faq-solution-qna-upgrade-plan.md)
- [Domain/QuestionSpace.cs](./Domain/QuestionSpace.cs)
- [Domain/Question.cs](./Domain/Question.cs)
- [Domain/Answer.cs](./Domain/Answer.cs)
- [Domain/KnowledgeSource.cs](./Domain/KnowledgeSource.cs)
- [Domain/Tag.cs](./Domain/Tag.cs)
- [Domain/QuestionSourceLink.cs](./Domain/QuestionSourceLink.cs)
- [Domain/AnswerSourceLink.cs](./Domain/AnswerSourceLink.cs)
- [Domain/ThreadActivity.cs](./Domain/ThreadActivity.cs)
