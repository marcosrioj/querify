---
name: design-provenance-and-trust
description: Represent source evidence, citations, confidence, and trust explicitly so BaseFAQ content can be audited and reused safely.
type: repository-skill
scope: basefaq-repository
category: domain
priority: high
triggers:
  - provenance
  - trust
  - source citation
  - evidence model
owned_paths:
  - dotnet/BaseFaq.Sample.Features.NewQaModel
  - docs/architecture/*
  - dotnet/BaseFaq.Models.*
collaborates_with:
  - model-question-thread-domain
  - plan-faq-to-qna-upgrade
---

# Design Provenance And Trust

## When to Use

- AI-generated or imported content needs explicit evidence.
- A prompt asks for citations, source links, authority rules, or trust modeling.

## Responsibilities

- Separate reusable artifact identity from context-specific source meaning.
- Model evidence on questions and answers explicitly.
- Preserve confidence and verification semantics.

## Workflow

1. Store reusable artifacts as `KnowledgeSource`.
2. Attach source relationships to questions through `QuestionSourceLink`.
3. Attach citations and evidence to answers through `AnswerSourceLink`.
4. Distinguish source roles such as origin, evidence, citation, and canonical reference.
5. Keep verification and authority state on the reusable source.
6. Keep context-specific confidence on the question, answer, and link level.

## BaseFAQ Domain Alignment

- Trust must support both human-curated and AI-assisted workflows.
- The same source can support multiple questions or answers with different roles.
- Provenance is a first-class part of the future Q&A platform.

## Collaborates With

- [`model-question-thread-domain`](../model-question-thread-domain/SKILL.md)
- [`plan-faq-to-qna-upgrade`](../plan-faq-to-qna-upgrade/SKILL.md)

## Done When

- The domain distinguishes reusable sources from per-thread evidence links.
- Confidence and verification fields are explicit.
- Public or operator-facing trust summaries can be derived from the model.
