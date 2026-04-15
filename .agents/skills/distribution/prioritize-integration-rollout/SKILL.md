---
name: prioritize-integration-rollout
description: Plan BaseFAQ external delivery channels around canonical HTML, tenant-safe public contracts, and phased integration rollout.
category: distribution
priority: high
triggers:
  - design distribution layer
  - integration strategy
  - embed sdk plugin roadmap
  - wordpress react nextjs rollout
owned_paths:
  - docs/architecture/basefaq-integrations-architecture.md
  - integrations/*
  - docs/*
collaborates_with:
  - design-provenance-and-trust
---

# Prioritize Integration Rollout

## When to Use

- A prompt is about BaseFAQ embeds, SDKs, framework adapters, CMS plugins, or distribution strategy.
- The question is which external channels should ship first and what rules they must follow.

## Responsibilities

- Build a phased rollout across integration categories.
- Preserve canonical HTML, SEO, accessibility, and structured-data ownership.
- Keep browser-delivered integrations tenant-safe.

## Workflow

1. Start from canonical HTML and normalized public render contracts.
2. Define the integration category and expected customer value.
3. Make schema and rendering ownership explicit for SEO-capable pages.
4. Treat embed delivery as the universal fallback.
5. Prioritize the highest-ROI platforms first.
6. Attach release, compatibility, testing, and security expectations to each phase.

## BaseFAQ Domain Alignment

- Canonical HTML is the rendering source of truth.
- Wrappers are conveniences, not rendering authorities.
- Public integrations must use tenant-safe credentials and versioned compatibility discipline.

## Collaborates With

- [`design-provenance-and-trust`](../../domain/design-provenance-and-trust/SKILL.md)

## Done When

- The rollout is phased by integration value and maintenance cost.
- Shared render contracts and SEO rules are explicit.
- Each integration category has clear testing and ownership expectations.
