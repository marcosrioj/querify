---
name: querify-behavior-change
description: "Staged Querify behavior-change workflow. Use when a change crosses entities, enums, DTOs, persistence, APIs, seed, tests, Portal UI, translations, or docs."
when_to_use: "Use for product behavior changes, model changes, cross-layer features, enum/property consolidation, Direct/Broadcast/Trust additions, Source upload, Creator MVP stages, or any broad diff."
paths:
  - "dotnet/**"
  - "apps/portal/**"
  - "docs/**"
---

# Querify Behavior Change

This skill summarizes `docs/behavior-change-playbook.md`. Use the full playbook when the change is broad.

## Non-negotiables

- Do not run or generate EF migrations unless the user explicitly asks.
- Do not remove `required` modifiers or add silent defaults to hide construction errors.
- Do not add a new enum, property, DTO field, or UI concept when an existing one represents the same business dimension.
- Do not retain deprecated duplicate fields to preserve behavior.
- Product persistence entities stay state-only.
- Direct, Broadcast, and Trust workflow state must not be stored in QnA.
- Repository artifacts are English by default.
- Portal translation changes must be complete across locale files in the same frontend change.
- Active lifecycle status means available for use/reuse, not unresolved work.

## Staging model

Prefer stages when a change is broad:

1. Model contract: entities, enums, DTOs, EF configuration, read mappings.
2. Backend behavior: commands, queries, handlers, services, controllers, feature registrations.
3. Seed and tests: factories, seed catalogs, scenario coverage, obsolete-test removal.
4. Frontend and i18n: API contracts, domain screens, shared enum labels, locale files.
5. Full validation and migration handoff.

Each stage should end with what builds, what is intentionally pending, tests run, tests not run, and manual migration notes.

## Workflow

1. Inventory existing behavior with `rg` before adding anything.
2. Identify the module owner: Tenant, QnA, Direct, Broadcast, or Trust.
3. Normalize the business concept so one dimension has one representation.
4. Update entities/enums only after confirming no duplicate concept exists.
5. Update EF configuration, indexes, tenant-integrity rules, and query projections.
6. Update DTO contracts in feature-scoped folders.
7. Update backend behavior in the owning feature project.
8. Update seed data only for realistic examples that matter locally or in tests.
9. Update integration tests and architecture tests where contracts or rules change.
10. Update Portal types, schemas, APIs, query keys, pages, shared enum metadata, and UI states.
11. Update every Portal locale file when user-facing copy changes.
12. Verify targeted projects first, then broaden based on touched surfaces.

## Search discipline

- When deleting or consolidating behavior, search for obsolete commands, queries, handlers, services, controller actions, helpers, extensions, validators, factories, seed builders, UI hooks, API clients, presentation metadata, locale keys, and tests.
- Historical EF migrations may mention old schema. Do not edit them unless migration work is explicit.

## Handoff checklist

End staged work with:

- canonical concepts added, changed, or deleted
- module boundary chosen
- old duplicate concepts removed
- projects that build
- tests run
- tests not run and why
- manual migration operations required
- frontend/i18n work remaining
- docs updated or intentionally unchanged
