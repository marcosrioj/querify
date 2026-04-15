---
name: plan-faq-to-qna-upgrade
description: Sequence the additive product and engineering work needed to move BaseFAQ from FAQ surfaces toward a question-and-answer platform.
type: repository-skill
scope: basefaq-repository
category: domain
priority: high
triggers:
  - faq to qna
  - migration plan
  - phased rollout
  - roadmap
owned_paths:
  - docs/architecture/*
  - dotnet/BaseFaq.Models.*
  - apps/portal/*
  - dotnet/BaseFaq.*
collaborates_with:
  - model-question-thread-domain
  - design-provenance-and-trust
---

# Plan FAQ-To-Q&A Upgrade

## When to Use

- The prompt is about staged product evolution rather than a single code change.
- Backward compatibility must be preserved while BaseFAQ adopts Q&A semantics.

## Responsibilities

- Build a phased plan across contracts, persistence, APIs, UI, AI flows, and tests.
- Keep changes additive and operationally safe.
- Tie milestones to concrete product capabilities.

## Workflow

1. Treat the Q&A thread model as the target language.
2. Identify additive changes needed across models, APIs, projections, UI, AI, and tests.
3. Plan compatibility shims for legacy FAQ consumers.
4. Prioritize accepted-answer, moderation, provenance, and activity history early.
5. Include seed, migration, and integration-test work in the rollout.

## BaseFAQ Domain Alignment

- The transition is not a global rewrite.
- Shared DTOs and public read models should evolve early so downstream consumers can adapt.
- AI work should move toward answer candidates and ranking, not only FAQ block generation.

## Collaborates With

- [`model-question-thread-domain`](../model-question-thread-domain/SKILL.md)
- [`design-provenance-and-trust`](../design-provenance-and-trust/SKILL.md)

## Done When

- The roadmap is phased and additive.
- Each milestone maps to concrete repository boundaries.
- Compatibility, testing, and migration work are included rather than deferred.
