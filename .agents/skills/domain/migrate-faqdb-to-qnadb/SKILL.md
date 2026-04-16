---
name: migrate-faqdb-to-qnadb
description: Guide additive migration from BaseFAQ's FAQ persistence and app surfaces toward QnA persistence and app surfaces without losing tenant safety, trust semantics, or behavioral parity.
type: repository-skill
scope: basefaq-repository
category: domain
priority: high
triggers:
  - faqdb to qnadb
  - migrate faqdb
  - faq parity migration
  - port faq to qna
  - qna completeness
owned_paths:
  - dotnet/BaseFaq.Faq.*
  - dotnet/BaseFaq.QnA.*
  - dotnet/BaseFaq.Models.*
  - apps/portal/*
  - docker/*
  - docs/backend/*
collaborates_with:
  - plan-faq-to-qna-upgrade
  - model-question-thread-domain
  - design-provenance-and-trust
  - apply-seed-and-migrations-safely
  - write-real-database-integration-tests
---

# Migrate FaqDb To QnADb

## When to Use

- The prompt is about moving FAQ behavior, parity, or data ownership onto the QnA model.
- The work spans more than one layer such as persistence, contracts, APIs, frontend, runtime, or tests.
- The goal is operational migration, not only target-state planning.

## Responsibilities

- Map FAQ concepts and behaviors onto the current QnA model without flattening QnA's richer thread semantics.
- Separate already-solved QnA domain concerns from missing application-layer work.
- Keep tenant boundaries, public visibility, provenance, and trust rules explicit during migration.
- Force a clear decision on whether FAQ-style user signals remain activity-only in QnA or become dedicated entities.

## Workflow

1. Inventory the FAQ capability being migrated across persistence, contracts, APIs, frontend, runtime, and tests.
2. Map the FAQ model to QnA explicitly:
   - `Faq` -> `QuestionSpace`
   - `FaqItem` -> `Question`
   - `FaqItemAnswer` -> `Answer`
   - `ContentRef` plus `FaqContentRef` -> `KnowledgeSource` plus source-link entities
   - `Tag` plus `FaqTag` -> `Topic` plus topic-link entities
3. Treat `Vote` and `Feedback` separately from workflow history:
   - FAQ stores them as dedicated operational entities
   - QnA currently correlates them most closely through `ThreadActivity` events such as `VoteReceived` and `FeedbackReceived`
   - do not claim a direct `1:1` entity replacement without an explicit design change
4. Identify what the QnA domain already covers through guards, behaviors, and invariants so migration effort stays focused on missing contracts and app surfaces.
5. Move additively from inner layers outward:
   - shared contracts and DTOs
   - persistence and migrations
   - Portal and Public business modules plus APIs
   - Portal frontend routes, hooks, and pages
   - runtime wiring, Docker, seed flows, and tests
6. Preserve QnA-native rules during the migration:
   - accepted-answer semantics
   - duplicate handling
   - source citation and public-excerpt permissions
   - append-only thread activity
   - tenant-safe visibility and publication rules
7. Verify parity with repository tooling rather than assumption:
   - EF migrations and seed flows
   - Portal and API runtime wiring
   - integration tests for tenant, public, moderation, and provenance behavior

## BaseFAQ Domain Alignment

- QnA is not a rename of FAQ. It is a richer thread model with spaces, questions, answers, provenance, moderation, and lifecycle history.
- FAQ parity should be described as behavior parity, not table parity.
- Migration work should prefer additive compatibility shims and staged rollout over rewrites.
- `ThreadActivity` is the QnA workflow and audit journal. It may correlate with FAQ `Vote` and `Feedback`, but it does not automatically replace their operational responsibilities.
- When a prompt asks whether QnA is "complete," check APIs, business modules, frontend, runtime, tooling, and tests, not only entities.

## Collaborates With

- [`plan-faq-to-qna-upgrade`](../plan-faq-to-qna-upgrade/SKILL.md)
- [`model-question-thread-domain`](../model-question-thread-domain/SKILL.md)
- [`design-provenance-and-trust`](../design-provenance-and-trust/SKILL.md)
- [`apply-seed-and-migrations-safely`](../../data/apply-seed-and-migrations-safely/SKILL.md)
- [`write-real-database-integration-tests`](../../data/write-real-database-integration-tests/SKILL.md)

## Done When

- The FAQ capability is mapped onto QnA using repository terminology instead of generic migration language.
- Remaining gaps are listed by boundary: contracts, persistence, APIs, frontend, runtime, tooling, and tests.
- `Vote` and `Feedback` versus `ThreadActivity` is classified correctly for the current repository state.
- The rollout stays additive and tenant-safe.
