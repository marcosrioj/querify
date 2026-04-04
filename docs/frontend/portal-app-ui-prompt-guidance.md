# Portal App UI Prompt Guidance

This document captures the UI, UX, and implementation standards that already exist in `apps/portal` and the additional decisions established during the recent portal refinement work. Future prompts and code changes should follow these rules unless there is an explicit product reason to do otherwise.

## Purpose

Use this guide when asking for Portal frontend changes or implementing them. The goal is to preserve consistency, reduce rework, and stop the UI from drifting into page-specific patterns that compete with the shared system.

## Core Principle

The Portal should feel like one product, not a collection of unrelated admin screens.

That means:
- Prefer shared layout and shared UI primitives over one-off markup.
- Keep pages task-oriented and lightweight.
- Use progressive disclosure instead of dumping instructions inline.
- Make risky actions confirmable.
- Make progress visible.

## Shared Primitives To Reuse

These are the default building blocks for Portal work:

- Layouts:
  - `apps/portal/src/shared/layout/page-layouts.tsx`
  - Use `PageHeader`, `ListLayout`, `DetailLayout`, `SettingsLayout`, `SectionGrid`, and `KeyValueList`.
- Core cards:
  - `apps/portal/src/components/ui/card.tsx`
  - `apps/portal/src/shared/ui/inset-card.tsx`
- Form guidance:
  - `apps/portal/src/shared/ui/form-section-heading.tsx`
  - `apps/portal/src/shared/ui/form-fields.tsx`
- Progress:
  - `apps/portal/src/shared/ui/progress-checklist.tsx`
- Confirmations:
  - `apps/portal/src/shared/ui/confirm-action.tsx`
- Tooltips:
  - `apps/portal/src/shared/ui/context-hint.tsx`

Do not recreate these patterns with ad hoc `div` styling if a shared primitive already exists.

## Page Layout Standards

### Headers

- Use `PageHeader`.
- Prefer `descriptionMode="hint"` when the description is secondary and should live behind the tooltip pattern.
- Use the eyebrow pill only when it adds routing or section context.

### List Pages

- Use `ListLayout`.
- Keep filters inside the built-in filter card area.
- Use `SectionGrid` for the small top metrics row.
- Use `DataTable` for the main records surface.

### Detail Pages

- Use `DetailLayout`.
- Main content belongs in the left column.
- Sidebar is for overview and quick notes only.
- Progress or onboarding checklists belong in the left column as the last block, not in the sidebar.

### Settings Pages

- Use `SettingsLayout`.
- Keep the nav rail unchanged unless a broader settings redesign is requested.

## Card Standards

### Main Cards

- Use the shared `Card` component.
- Do not introduce plain bordered containers as substitutes for the main content surfaces.

### Inset Text Cards

- Use `InsetCard` for nested informational blocks, reference summaries, linked-record previews, CTA summaries, identity summaries, and similar text-heavy inner panels.
- Do not hand-roll nested cards with repeated utility strings like `rounded-2xl border ... bg-muted/... p-4`.

### Metric Cards

- For grids of four small summary cards, use `SectionGrid`.
- `SectionGrid` now supports:
  - title
  - value
  - description
  - title hint
  - icon
  - optional icon tone class
- The icon is decorative and overlayed. It must not consume layout width from the content.
- Choose icons that represent the meaning of the block, in the same spirit as the dashboard metric cards.

## Form Standards

### Section Headings

- Use `FormSectionHeading` for form sections.
- Section descriptions should be tooltip-based through `ContextHint`, not inline paragraph blocks under the `h3`.
- Do not reintroduce the old `h3 + p` section pattern unless specifically requested.

### Field Components

- Use shared field components from `form-fields.tsx`.
- Prefer built-in `description`, `hint`, `placeholder`, and `disabled` support rather than custom wrappers.

### Forms Should Be Structured

Forms should feel guided, not like raw CRUD payload editors.

That means:
- Break large forms into named sections.
- Add placeholders where they improve comprehension.
- Use descriptions only when they clarify a choice or consequence.
- Keep copy concise and action-oriented.

## Progress And Onboarding Standards

Use `ProgressChecklistCard` when a page benefits from a “what should I do next?” layer.

Rules:
- On create pages, the eyebrow can be `Start here`.
- On edit/progress-oriented pages, the eyebrow can be `Progress`.
- The card should sit in the left column, as the last item of the page content.
- The checklist should hide when all steps are complete unless there is a strong reason not to.
- The sidebar should not carry the main onboarding burden.

## FAQ, Q&A Item, And Source Rules

These are product rules already reflected in the portal and should remain consistent:

### Q&A Item CTA Inheritance

- Q&A item `CTA title` and `CTA URL` are governed by the parent FAQ `Enable CTA` setting.
- If FAQ CTA is disabled:
  - Q&A item CTA fields must be disabled.
  - The UI must explain why.
  - The detail page should show the CTA state as locked/inactive.
  - If appropriate, offer a direct link to the FAQ CTA settings.

### Progress Checklist Placement

- FAQ, Q&A item, and source detail/form pages should keep the checklist in the left/main column as the last item.
- The right sidebar should stay focused on overview or quick notes.

## Confirmation Standards

### Destructive Actions

- Use `ConfirmAction` for destructive actions such as delete.

### AI Generation And Similar Explicit Actions

- Also use `ConfirmAction` for AI generation or other meaningful launch/run actions.
- For non-destructive confirmation flows, use the primary-style confirmation variant already supported by the shared component.
- Do not trigger AI generation immediately on click.

## Visual Hierarchy Standards

- Titles should be strong and compact.
- Secondary copy should be muted.
- Tooltips should carry secondary explanations whenever possible.
- Numeric and status summaries should appear before dense content.
- Avoid pages that are just stacked text paragraphs inside undifferentiated boxes.

## Copy Standards

- Use `Delete` for record deletion actions.
- Use `Remove` only when the user is being detached from a relationship rather than deleting the underlying entity.
- Prefer short, direct labels:
  - `New FAQ`
  - `New Q&A item`
  - `New source`
  - `Request generation`
  - `Open source`

## Loading, Empty, And Error States

- Use the shared loading skeletons where available.
- Use `EmptyState` for empty collections and missing optional content.
- Use `ErrorState` for failed page-level data loading.
- Do not leave blank sections or raw fallback text on data failure.

## What To Avoid

- Do not create page-specific card styles when `Card`, `InsetCard`, or `SectionGrid` already fit.
- Do not put explanatory paragraphs under every form section heading.
- Do not move major onboarding or progress guidance into the sidebar.
- Do not let decorative icons change content layout width.
- Do not bypass shared confirmation patterns for delete or AI generation actions.
- Do not introduce inconsistent action naming across similar entities.

## Recommended Prompt Shape For Future Portal Work

When writing a future prompt for Portal UI changes, include:

1. The exact page or domain.
2. Whether the page is list, detail, or form.
3. Whether the change should follow an existing portal pattern.
4. Whether the behavior is visual only or also business-rule-driven.
5. Whether the request affects:
   - checklist placement
   - CTA inheritance
   - metric cards
   - confirmation dialogs
   - tooltip-based guidance

Example:

```md
Update the Portal Q&A item detail page.
Follow the existing Portal detail-page standards.
Keep the overview in the right sidebar.
Keep progress guidance in the left column as the last block.
If the change touches CTA behavior, preserve the FAQ-level CTA inheritance rule.
```

## When To Update This Guide

Update this document whenever a new shared Portal pattern is introduced or an old one is intentionally replaced. Do not let the codebase evolve in one direction and the prompt guidance in another.
