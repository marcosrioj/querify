# Portal App UI Guidance

This document is the implementation companion to [`portal-app.md`](portal-app.md). Use it when changing Portal pages so the UI stays coherent across domains.

## Product rule

The Portal should feel like one product, not a collection of unrelated admin pages.

Practical consequences:

- prefer shared layouts and shared UI primitives
- keep pages task-oriented
- use progressive disclosure instead of long explanatory text blocks
- make risky actions explicit and confirmable

## Shared primitives to reuse

Default building blocks in `apps/portal`:

- layout primitives from `src/shared/layout/page-layouts.tsx`
- `Card`
- `FormSectionHeading` and shared field components from `form-fields.tsx`
- `ProgressChecklistCard`
- `ConfirmAction`
- `ContextHint`
- `EmptyState` and `ErrorState` from `src/shared/ui/placeholder-state.tsx`
- shared skeletons from `src/shared/ui/loading-states.tsx`

If a shared primitive already matches the use case, do not replace it with ad hoc utility-heavy markup.

## Layout standards

### List pages

- use `ListLayout`
- keep metrics in `SectionGrid`
- keep the main record surface in the shared table pattern

### Detail pages

- use `DetailLayout`
- keep the main workflow in the left column
- keep the sidebar limited to overview, notes, or summary information
- keep onboarding or progress checklists in the main column, usually as the last block

### Settings pages

- use `SettingsLayout`
- preserve the existing navigation rail unless the task is a deliberate settings redesign

## Form standards

- break large forms into named sections
- prefer tooltip-based contextual help over large paragraphs under every heading
- reuse shared field components before introducing custom wrappers
- keep copy concise and action-oriented

## Confirmation standards

- destructive actions should use `ConfirmAction`
- long-running or other meaningful run actions should also use an explicit confirmation step
- do not fire irreversible or expensive actions immediately on click

## Domain-specific UI rules

### Question CTA inheritance

- question-level CTA fields inherit from the parent space CTA setting when the flow uses shared CTAs
- if the space-level CTA is disabled, question CTA controls should be visibly disabled and explained

### Progress checklist placement

- space, question, answer, and source pages should keep progress guidance in the main content column
- the sidebar should not become the primary onboarding surface

## Visual hierarchy rules

- show status, key metrics, and compact summaries before dense text
- use muted supporting copy and stronger titles
- avoid flat pages made only of repeated bordered boxes
- do not let decorative icons change the width allocation of card content

## State handling

- use shared loading states where available
- use explicit empty states for empty collections or optional content
- use explicit error states for page-level failures
- never leave blank sections with raw fallback text

## When to update this document

Update this file whenever a new shared Portal pattern is introduced or an old one is intentionally replaced. The guide should stay aligned with the real implementation, not with historical prompts.
