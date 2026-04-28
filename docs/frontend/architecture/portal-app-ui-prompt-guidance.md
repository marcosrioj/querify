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
- `ActionPanel` and `ActionButton`
- `ConfirmAction`
- `ContextHint`
- `SearchSelect` and `SearchSelectField`
- `ChildListPagination`
- `EmptyState` and `ErrorState` from `src/shared/ui/placeholder-state.tsx`
- shared skeletons from `src/shared/ui/loading-states.tsx`
- status badges from `src/shared/ui/status-badges.tsx`
- enum presentation metadata from `src/shared/constants/enum-ui.ts`

If a shared primitive already matches the use case, do not replace it with ad hoc utility-heavy markup.

## Shell standards

- Primary navigation belongs in the left sidebar.
- The workspace switcher belongs in the sidebar header.
- The top toolbar is for breadcrumbs, command search, language, notifications, and the user menu.
- Do not use the QnA module navigation as primary app navigation.
- Use the QnA module visual language only for in-screen child and relationship management.
- The left sidebar belongs to desktop only. Mobile and tablet widths use the header plus drawer through the `xl` breakpoint.
- Keep shell rendering and CSS breakpoints in sync. If `PortalSidebar`, `MobileHeader`, or `useIsMobile` changes, verify all three still switch at the same width.
- The app shell must not impose a minimum content width. Add `min-w-0` to root, shell, main, card, and flex children that need to shrink.

## Layout standards

### List pages

- use `ListLayout`
- keep metrics in `SectionGrid`
- keep the main record surface in the shared table pattern
- keep list pages usable at 320, 360, 375, 414, 768, 1024, 1279, 1280, and desktop widths
- render list records as stacked cards below `xl`; render the table surface at `xl` and above unless the page intentionally owns a different pattern
- make filters, search inputs, sort controls, pagination, and actions stack or wrap before they overflow
- long unbroken values such as URLs, external ids, checksums, user agents, and generated keys must wrap inside their card or cell
- never fix a list overflow only in the page component if the cause is a shared primitive, shell container, or root flex width

### Detail pages

- use `DetailLayout`
- keep primary narrative, content, and relationship tabs in the main column
- keep the right rail for actions, status summaries, workflow rules, publishing state, settings, metadata, and timing context
- do not render relationship sections as top-of-page anchors
- keep onboarding or progress guidance in the location that best supports the current task, usually main content or a compact right-rail summary

### Settings pages

- use `SettingsLayout`
- preserve the existing navigation rail unless the task is a deliberate settings redesign

## Relationship section standards

Relationship sections manage children and related records without leaving the origin screen.

- Use tab-like controls with the current relationship section visual language.
- Reveal one relationship area at a time.
- Keep lists scoped to the current parent entity, including tags and sources.
- Do not redirect to global list pages just to view related records.
- Do not use anchor links.
- Do not include create links unless the create flow actually works in that local context.
- Do not include repeated generic titles when the surrounding page already provides context.
- A short hint is acceptable when it explains scope or consequence.
- Use `ChildListPagination` for local child lists with more than five items, with page sizes 5, 10, and 20.
- Do not retrofit this child-list pagination rule onto top-level list pages.

## Form standards

- break large forms into named sections
- prefer tooltip-based contextual help over large paragraphs under every heading
- reuse shared field components before introducing custom wrappers
- keep copy concise and action-oriented
- any select/dropdown backed by a backend list endpoint must use `SearchSelect` or `SearchSelectField`
- use the searchable pattern for both single-selection and link/multi-selection flows
- keep enum-only controls on the normal `Select` primitive
- `SearchSelect` popovers must be constrained to the viewport on narrow screens and must not impose a desktop minimum width on mobile.

## Confirmation standards

- destructive actions should use `ConfirmAction`
- long-running or other meaningful run actions should also use an explicit confirmation step
- do not fire irreversible or expensive actions immediately on click

## Action standards

- Use `ActionPanel` for screen-level and right-rail action groups.
- Use `ActionButton` for the square, lightly rounded Portal action identity.
- Keep panel descriptions as concise hints, not loose explanatory copy.
- Keep destructive actions visually quiet until the final confirmation action.
- Do not mix unrelated button shapes or densities in the same action surface.

## Domain-specific UI rules

### QnA workflow surfaces

- Use the centralized enum presentation layer for readable labels, badge variants, descriptions, and sorting groups.
- Question detail should manage accepted answer, duplicate routing, answers, sources, tags, and activity in context.
- Space detail should manage questions, tags, sources, and activity in context.
- Answer detail should manage source evidence in context.
- Accepted answer selection should use the searchable select pattern and show only eligible answers.

### Progress guidance

- Dashboard progress should be derived from real data where possible.
- First-run guidance should focus on the next best action.
- Avoid dense enterprise dashboards before there is workspace content to manage.

## Visual hierarchy rules

- show status, key metrics, and compact summaries before dense text
- use muted supporting copy and stronger titles
- avoid flat pages made only of repeated bordered boxes
- do not let decorative icons change the width allocation of card content
- use semantic tokens and CSS variables so light and dark mode both remain readable
- avoid hardcoded light-only backgrounds, borders, text colors, and shadows
- all new page work should be checked in both light and dark themes

## State handling

- use shared loading states where available
- use explicit empty states for empty collections or optional content
- use explicit error states for page-level failures
- never leave blank sections with raw fallback text
- keep mutation pending states visible at the button or row level
- use skeletons that preserve the final layout footprint

## When to update this document

Update this file whenever a shared Portal pattern changes. The guide should stay aligned with the real implementation, not with historical prompts.
