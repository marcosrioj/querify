---
name: model-question-thread-domain
description: Model BaseFAQ's future knowledge system as question threads with reusable spaces, answer candidates, and lifecycle history.
type: repository-skill
scope: basefaq-repository
category: domain
priority: high
triggers:
  - structure q&a knowledge
  - question thread model
  - accepted answer
  - domain redesign
owned_paths:
  - dotnet/BaseFaq.Sample.Features.NewQaModel
  - docs/architecture/*
  - dotnet/BaseFaq.Models.*
collaborates_with:
  - design-provenance-and-trust
  - plan-faq-to-qna-upgrade
---

# Model Question Thread Domain

## When to Use

- A prompt is about the FAQ-to-Q&A transition.
- The repository needs new domain language for questions, answers, moderation, or thread lifecycle.

## Responsibilities

- Define the smallest durable Q&A core model.
- Preserve extensibility for public, curated, community, and AI-assisted spaces.
- Keep the model aligned with BaseFAQ's additive migration strategy.

## Workflow

1. Define `QuestionSpace` as the operating surface.
2. Model each user-facing thread as a `Question`.
3. Represent response candidates as `Answer` records with explicit kind and status.
4. Add accepted-answer, duplicate, and revision concepts to the thread aggregate.
5. Keep taxonomy lightweight with reusable topics.
6. Capture history through append-only `ThreadActivity`.

## BaseFAQ Domain Alignment

- The target model is a living thread system, not a static FAQ block.
- Accepted answers matter more than first answers.
- The initial entity set should stay small and extensible.

## Collaborates With

- [`design-provenance-and-trust`](../design-provenance-and-trust/SKILL.md)
- [`plan-faq-to-qna-upgrade`](../plan-faq-to-qna-upgrade/SKILL.md)

## Done When

- The model has clear thread, answer, and activity semantics.
- The terminology supports future moderation and provenance work.
- The change can be adopted additively across the current BaseFAQ stack.
