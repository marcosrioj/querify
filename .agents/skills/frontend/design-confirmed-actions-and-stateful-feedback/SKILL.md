---
name: design-confirmed-actions-and-stateful-feedback
description: Wrap destructive or expensive Portal actions in confirmations and explicit loading, empty, and error states.
category: frontend
priority: high
triggers:
  - confirm delete
  - loading state
  - empty state
  - error state
  - destructive action ux
owned_paths:
  - apps/portal/src/domains/*
  - apps/portal/src/shared/ui/*
collaborates_with:
  - compose-portal-page-layouts
  - implement-portal-localization
---

# Design Confirmed Actions And Stateful Feedback

## When to Use

- A user action is destructive, irreversible, or operationally expensive.
- A page needs resilient loading, empty, and error handling.

## Responsibilities

- Add consequence-focused confirmation flows.
- Keep pending and failure states explicit.
- Reuse shared placeholder, skeleton, and feedback components.

## Workflow

1. Identify actions that can delete, regenerate, or materially alter data.
2. Wrap them in `ConfirmAction` with direct consequence language.
3. Keep failure states actionable and retryable.
4. Add pending state on both the trigger and the confirm action.
5. Render explicit `loading`, `empty`, and `error` states for each query-backed section.
6. Keep copy concise and localization-friendly.

## BaseFAQ Domain Alignment

- AI generation requests deserve confirmation, not only destructive deletes.
- Portal pages should never show blank sections during load or failure.
- Shared UI state components are the first choice over ad hoc spinners or strings.

## Collaborates With

- [`compose-portal-page-layouts`](../compose-portal-page-layouts/SKILL.md)
- [`implement-portal-localization`](../implement-portal-localization/SKILL.md)

## Done When

- Destructive and expensive actions require explicit confirmation.
- Query-backed sections have visible pending, empty, and error behavior.
- Copy remains short, action-oriented, and ready for localization.
