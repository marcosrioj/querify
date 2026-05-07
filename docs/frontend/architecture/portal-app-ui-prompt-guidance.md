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
- list filter controls from `src/shared/ui/list-filter-controls.tsx`
- `ChildListPagination`
- `EmptyState` and `ErrorState` from `src/shared/ui/placeholder-state.tsx`
- shared skeletons from `src/shared/ui/loading-states.tsx`
- status badges from `src/shared/ui/status-badges.tsx`
- enum presentation metadata from `src/shared/constants/enum-ui.ts`

If a shared primitive already matches the use case, do not replace it with ad hoc utility-heavy markup.

## Shell standards

- Primary navigation belongs in the left sidebar.
- The workspace switcher belongs in the sidebar header.
- The top toolbar is for the unified page trail, command search, language, notifications, and the user menu.
- The unified page trail combines parent breadcrumbs, the current page title, optional back navigation, and compact title hints. Do not render a second page title in the content header.
- QnA child breadcrumbs must follow the owning lineage with the generic parent label, for example Space -> Question -> question title or Space -> Question -> Answer -> Activity. Only the Space detail page itself should show the Space name.
- Breadcrumb/title text must truncate inside the toolbar instead of wrapping or widening the shell.
- Do not use the QnA module navigation as primary app navigation.
- Use the QnA module visual language only for in-screen child and relationship management.
- The left sidebar belongs to desktop only. Mobile and tablet widths use the header plus drawer through the `xl` breakpoint.
- Keep shell rendering and CSS breakpoints in sync. If `PortalSidebar`, `MobileHeader`, or `useIsMobile` changes, verify all three still switch at the same width.
- The app shell must not impose a minimum content width. Add `min-w-0` to root, shell, main, card, and flex children that need to shrink.

## Layout standards

### List pages

- use `ListLayout`
- put the primary search and filter command surface in the `ListLayout` `filters` slot, before metrics and before the result table
- avoid full metric-card grids on search-heavy list pages; they add scroll debt before users reach results, especially on mobile
- put result counts and supporting list numbers in a compact `ListResultSummary` rail inside the results card instead of separate `SectionGrid` cards unless the page is primarily analytical; preserve metric explanations as small `ContextHint` icons on each summary item
- `ListResultSummary` must wrap onto left-aligned rows on mobile instead of scrolling or clipping; let summary chips use the remaining space to the left of the primary create/manage action, which stays visually prioritized and aligned right in the same toolbar
- keep page descriptions and table descriptions as `ContextHint` icon affordances whenever the page already has clear labels and actions; avoid extra explanatory paragraphs above search results
- keep the main record surface in the shared table pattern; once filters move to `ListLayout`, `DataTable` should focus on result title, result rows, empty/error states, and pagination
- keep row cells concise on search-heavy lists: show primary identity and durable metadata, not summaries, context notes, or repeated explanatory counts that make mobile cards tall
- keep search, quick filters, advanced filters, table content, mobile cards, and pagination in visually grouped surfaces that match the quiet premium card language used by detail pages
- keep list pages usable at 320, 360, 375, 414, 768, 1024, 1279, 1280, and desktop widths
- render list records as stacked cards below `xl`, with primary record content, supporting fields, and actions separated inside each card; render the table surface at `xl` and above unless the page intentionally owns a different pattern
- make filters, search inputs, sort controls, pagination, and actions stack or wrap before they overflow
- use the shared list filter controls for top-level filter surfaces: keep free-text search visible in the `ListLayout` `filters` slot with `ListFilterSearch`, place high-frequency chips, enum filters, relationship filters, and sort controls inside `ListFilterDisclosure`, put grouped controls in `ListFilterToolbar`, and wrap every select in `ListFilterField`
- top-level list filters should be collapsed by default after the search row; the disclosure trigger sits on the search row, shows the count for active non-search filters, and opens the secondary filter panel without pushing results far down on mobile
- every filter surface must show active filter count and a single clear action when filters are active
- filter controls must show a skeleton loading signal during request refreshes using `isFetching`, not only during first-page `isLoading`
- row actions on top-level lists should be explicit text+icon buttons where space allows; icon-only row actions need accessible labels and should not be the only discoverable way to open or edit a record
- long unbroken values such as URLs, external ids, checksums, user agents, and generated keys must wrap inside their card or cell
- never fix a list overflow only in the page component if the cause is a shared primitive, shell container, or root flex width

### Detail pages

- use `DetailLayout`
- keep primary narrative, content, and relationship tabs in the main column
- place the screen-level `ActionPanel` in the main column as the first action surface, using the `layout="bar"` toolbar treatment
- keep record actions in that first action surface; do not scatter additional record-level commands into the right rail
- keep `RecommendedNextActionCard` in the main column with its own action button; do not move recommended-next-action commands into the screen-level action toolbar
- keep the right rail to one consolidated `DetailOverviewCard` whenever the page is summarizing record status, workflow rules, publishing state, settings, metadata, timing context, and compact metrics
- put high-signal status and metric fields in the `DetailOverviewCard` highlight grid, then put supporting metadata in the same card's key/value list
- render enum values in the overview with the existing badge/status components; do not downgrade enums to plain text
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
- Question, Answer, and Activity do not have top-level list pages. Keep those lists inside their owning relationship tabs: Space -> Questions and Activity, Question -> Answers and Activity, Answer -> Activity.
- Fetch relationship lists with scoped backend pagination and filters. Do not fetch broad child collections into detail DTOs for local filtering or local pagination.
- Put relationship filters behind a compact filter icon button by default. Do not show parent filters that are already implied by the relationship context.
- Do not retrofit this child-list pagination rule onto top-level list pages.

## Form standards

- break large forms into named sections
- prefer tooltip-based contextual help over large paragraphs under every heading
- reuse shared field components before introducing custom wrappers
- keep copy concise and action-oriented
- every editable field must include a concise field-level explanation, not only a label
- for shared form fields, pass `description`; reserve `hint` for secondary consequences or caveats
- when a native input is necessary, pair its visible label with `ContextHint` so the field purpose remains discoverable
- any select/dropdown backed by a backend list endpoint must use `SearchSelect` or `SearchSelectField`
- use the searchable pattern for both single-selection and link/multi-selection flows
- keep enum-only controls on the normal `Select` primitive
- `SearchSelect` popovers must be constrained to the viewport on narrow screens and must not impose a desktop minimum width on mobile.
- For dedicated Add/Edit forms, consider `FormSetupProgressCard` whenever required fields represent a meaningful completion path for creating or updating the record.
- Place form setup progress below the main form card in the primary content column, derive steps from current form values, and keep the default hide-at-100% behavior.
- Keep setup progress steps aligned with the fields that actually determine readiness; update the steps whenever those fields or their meaning change.

## Confirmation standards

- destructive actions should use `ConfirmAction`
- long-running or other meaningful run actions should also use an explicit confirmation step
- do not fire irreversible or expensive actions immediately on click

## Action standards

- Use `ActionPanel` for screen-level and right-rail action groups.
- Use `ActionButton` for the square, lightly rounded Portal action identity.
- On detail pages, use `ActionPanel layout="bar"` for the primary record action surface.
- The detail action bar should feel like a premium SaaS toolbar: compact, horizontally aligned on desktop, visually quiet, and able to wrap elegantly on mobile.
- The action bar label and its `ContextHint` need enough left padding and must not touch the card edge.
- Tooltip content must render through the shared tooltip portal so hints are not clipped by cards, rails, or overflow containers.
- Keep regular actions on the left side of the action bar.
- Keep destructive or destructive-feeling actions grouped on the right side.
- If a red/destructive lifecycle action exists, it anchors the destructive group to the right and must sit immediately to the left of `Delete`.
- `Delete` must be the final action in the row. It should align to the far right, use content width only, and never stretch to fill remaining space.
- If `Delete` is the only destructive action, it anchors itself to the far right.
- Keep panel descriptions as concise hints, not loose explanatory copy.
- Keep destructive actions visually quiet until the final confirmation action.
- Do not mix unrelated button shapes or densities in the same action surface.

## Domain-specific UI rules

### QnA workflow surfaces

- Use the centralized enum presentation layer for readable labels, badge variants, descriptions, and sorting groups.
- Question detail should manage answers, optional accepted answer, sources, tags, and activity in context.
- Space detail should manage questions, tags, sources, and activity in context.
- Answer detail should manage optional source evidence in context.
- Answer detail should manage answer activity in context.
- Optional accepted answer selection should use the searchable select pattern and show only eligible answers.

### Progress guidance

- Dashboard progress should be derived from real data where possible.
- The canonical Getting Started sequence, completion criteria, recommended-next-action ranking, and guidance-surface inventory live in [`portal-getting-started-guidance.md`](portal-getting-started-guidance.md).
- Dashboard setup progress should not render any setup-progress or completion surface after it reaches 100%.
- Dedicated Add/Edit setup progress should be form-specific and should disappear when every step is complete.
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

This is the **single source of truth** for Portal UI implementation rules. `portal-app.md` delegates all shared UI patterns here and must not duplicate content from this file.

Update this file whenever a shared Portal pattern changes. The guide should stay aligned with the real implementation, not with historical prompts.
