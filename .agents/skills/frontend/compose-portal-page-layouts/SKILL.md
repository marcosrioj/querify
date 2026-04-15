---
name: compose-portal-page-layouts
description: Structure Portal pages with BaseFAQ shared layout primitives so screens stay coherent across domains.
category: frontend
priority: high
triggers:
  - compose page
  - portal layout
  - detail page
  - settings page
owned_paths:
  - apps/portal/src/domains/*
  - apps/portal/src/shared/layout/*
collaborates_with:
  - build-portal-domain-data-flow
  - design-confirmed-actions-and-stateful-feedback
---

# Compose Portal Page Layouts

## When to Use

- A Portal list, detail, or settings page needs to be created or refactored.
- A feature needs shared metrics, sidebars, or layout consistency.

## Responsibilities

- Choose the correct shared layout primitive.
- Place primary workflows in the main column and supporting context in the sidebar.
- Preserve mobile and desktop readability.

## Workflow

1. Choose `ListLayout`, `DetailLayout`, or `SettingsLayout`.
2. Start with `PageHeader` and concise task-oriented copy.
3. Place metrics, summaries, and next actions before dense tables or forms.
4. Reuse shared cards, hints, and key-value patterns.
5. Verify the layout hierarchy on both wide and narrow screens.

## BaseFAQ Domain Alignment

- Portal UI should feel like one product, not independent admin screens.
- Shared layout patterns in `apps/portal/src/shared/layout/page-layouts.tsx` are the default baseline.
- Settings pages keep the navigation rail unless there is a deliberate product-wide change.

## Collaborates With

- [`build-portal-domain-data-flow`](../build-portal-domain-data-flow/SKILL.md)
- [`design-confirmed-actions-and-stateful-feedback`](../design-confirmed-actions-and-stateful-feedback/SKILL.md)

## Done When

- The page uses shared layout primitives.
- Primary actions, metrics, and context are placed consistently.
- The page reads as a BaseFAQ Portal screen, not a one-off implementation.
