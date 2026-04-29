# BaseFAQ Portal App

## Purpose

This document is the main frontend guide for `apps/portal`. It explains what the app is responsible for, how it connects to the backend, and how to reason about Portal-specific implementation work.

## Scope

`apps/portal` is the tenant-facing web application for BaseFAQ. It is responsible for:

- authenticated workspace access
- Tenant workspace context, settings, member, profile, and billing flows
- QnA management screens for spaces, questions, answers, sources, tags, and activity
- frontend-owned localization and direction handling

It is not the BackOffice UI and it does not own BackOffice API concerns.

## Technology stack

- React 19
- Vite
- Tailwind CSS v4
- TanStack Query and TanStack Table
- Auth0 SPA authentication with `@auth0/auth0-spa-js`
- `react-hook-form` plus `zod` for forms and validation
- `lucide-react` for icons
- `sonner` for toast notifications
- `react-intl` for frontend-owned localization and RTL or LTR handling

## Repository structure

```text
apps/portal/
  src/
    app/
    domains/
    platform/
    providers/
    shared/
    components/
    css/
```

The app already uses a shared UI baseline and domain-oriented organization. Reuse the existing structure instead of creating page-specific islands.

## Backend integrations

The Portal currently integrates with:

- `BaseFaq.Tenant.Portal.Api`
- `BaseFaq.QnA.Portal.Api`

Operational constraints reflected in the frontend:

- protected flows require Auth0 JWT authentication
- tenant-scoped backend calls require `X-Tenant-Id`
- tenant summaries expose `module`, backed by `ModuleEnum` values: Tenant, QnA, Direct, Broadcast, and Trust
- pagination contracts use `SkipCount`, `MaxResultCount`, and `Sorting`
- backend error payloads follow `{ ErrorCode, MessageError, Data }`; the frontend also accepts
  camelCase fields defensively
- Portal UI translation is frontend-owned, and backend DTOs do not provide translated labels

API errors shown in toasts, confirmation failures, or page placeholders must go through
`src/platform/api/api-error.ts`. That module normalizes `MessageError`, maps dynamic backend
messages such as records with ids to stable frontend messages, and then sends the result through the
Portal i18n catalogs. Do not render raw backend error strings directly in components.

## Architecture and implementation rules

### Product shell

The Portal shell uses a stable left sidebar for primary navigation and workspace context.

- Keep the tenant/workspace switcher in the sidebar header.
- Do not move the workspace switcher into the top toolbar.
- Keep primary navigation grouped by user mental model: Workspace, Administration, and Account.
- The top toolbar is for route context and global utilities: breadcrumbs, command search, language, notifications, and user menu.
- Render page location as one toolbar trail: parent navigation links plus the current page title. Page headers register the current title, back target, and hint text for that trail instead of rendering a second competing title in the page body.
- Toolbar breadcrumb labels must stay on one line, use `min-w-0`, and truncate long record names instead of wrapping or forcing page overflow at mobile, tablet, or desktop widths.
- The QnA module navigation pattern is not a primary app navigation replacement. Use it only inside a domain screen when the user is managing children or related records in the current context.
- The fixed sidebar is a desktop pattern only. Below the `xl` breakpoint, including tablet widths, use the mobile/tablet header and drawer so content keeps the full viewport width.
- Keep the JavaScript shell breakpoint in `useIsMobile` aligned with the Tailwind breakpoint used by the sidebar and mobile header. Do not let React render one shell mode while CSS displays another.
- The Portal must work down to a 320 CSS pixel viewport. Root, shell, and page flex containers must allow shrinking with `min-w-0`; do not introduce a page-level minimum width such as 414px.

### Shared layouts and primitives

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

### Layout standards

#### List pages

- use `ListLayout`
- keep metrics in `SectionGrid`
- keep the main record surface in the shared table pattern
- Top-level list pages must remain fully usable from mobile through tablet and desktop. Use card rows below `xl` and the table surface from `xl` upward unless a page has a deliberate alternate responsive pattern.
- List headers, filter bars, toolbar actions, pagination, and API-backed selects must wrap or stack instead of forcing horizontal page overflow.
- Cells, card rows, badges, long URLs, ids, checksums, user agents, and generated tokens must use `min-w-0` plus explicit word breaking where needed.
- Horizontal scrolling is acceptable only inside an intentionally scrollable component such as a table wrapper. The page, shell, and card surface must not become wider than the viewport.

#### Detail pages

- use `DetailLayout`
- keep primary narrative, rich content, and relationship tabs in the main column
- keep the right rail for actions, summary, workflow rules, publishing state, settings, metadata, and timing context
- do not place relationship tab selectors above the detail header
- do not use anchor links for child or relationship areas
- keep relationship management scoped to the origin screen instead of redirecting users to global lists whenever the relationship can be managed in place
- keep onboarding or progress checklists in the main content flow unless the page specifically needs a compact right-rail status summary

#### Settings pages

- use `SettingsLayout`
- preserve the existing navigation rail unless the task is a deliberate settings redesign

### Relationship section standards

Relationship sections are local management surfaces for child records and related entities.

- Render relationship areas as tabs with the current QnA module visual language.
- Tabs should reveal one scoped list at a time.
- Do not render them as anchor links.
- Do not include broken or speculative create links.
- Do not show repeated section titles such as "Manage child records from this Space" when the page context is already clear.
- A concise contextual hint is allowed when it helps explain the scope.
- Filter child lists by the current parent entity, including tags and sources.
- Use `ChildListPagination` for child lists with more than five items, with page sizes 5, 10, and 20.
- Do not apply this local child pagination rule to top-level list pages unless that page already owns its own pagination contract.

### Form standards

- break large forms into named sections
- prefer tooltip-based contextual help over large paragraphs under every heading
- reuse shared field components before introducing custom wrappers
- keep copy concise and action-oriented
- every editable field must include a concise field-level explanation, not only a label
- for shared form fields, pass `description`; use `hint` only for secondary caveats or consequences
- when a native input is necessary, pair its visible label with `ContextHint` so the field purpose is still discoverable
- any select or dropdown whose options come from a backend list endpoint must use `SearchSelect` or `SearchSelectField`
- use the same searchable pattern whether the field accepts one selection or participates in a multi-selection/linking flow
- keep static enum fields on the normal `Select` primitive unless they are backed by an API list
- when a dedicated Add/Edit form has required fields that define whether the record is ready to save, include or update the shared `FormSetupProgressCard`
- form setup progress belongs below the main form card in the primary content column, and it should use `react-hook-form` values so it reflects the current unsaved state
- form setup progress should keep the default behavior of disappearing when every step is complete; do not add a separate completed-state card unless the product requirement explicitly asks for one

### Confirmation standards

- destructive actions should use `ConfirmAction`
- long-running or other meaningful run actions should also use an explicit confirmation step
- do not fire irreversible or expensive actions immediately on click

### Action standards

- Use `ActionPanel` for screen-level and right-rail actions.
- Action controls should use the shared square, lightly rounded identity from `ActionButton`.
- Keep labels explicit and action-oriented.
- Use panel descriptions as compact hints, not as loose explanatory paragraphs.
- Avoid mixing unrelated action styles inside the same surface.

### Domain-specific UI rules

#### QnA workflow and relationship surfaces

- Spaces, questions, answers, sources, tags, and activity should expose child and related records in local relationship tabs where practical.
- Question detail should keep accepted answer, duplicate routing, source links, tags, answers, and activity scoped to the thread.
- Space detail should keep questions, tags, sources, and activity scoped to the space.
- Answer detail should keep source evidence scoped to the answer.
- Accepted answer selection should use the searchable selection pattern and should only expose answers that are eligible to be accepted.

#### First-run and progress guidance

- Dashboard activation should infer progress from real Portal data where possible.
- Dashboard setup progress should disappear completely when it reaches 100%, including compact progress panels and completion cards.
- Dedicated Add/Edit setup progress should be considered whenever a form change adds, removes, renames, or changes the readiness meaning of required fields.
- Dedicated Add/Edit setup progress should describe the form-specific completion path, not the workspace activation path.
- First-run guidance should make the next action obvious without hiding operational surfaces.
- Progress guidance may appear in the main content column or as a compact right-rail status summary when that placement better supports the page.

### Visual hierarchy rules

- show status, key metrics, and compact summaries before dense text
- use muted supporting copy and stronger titles
- avoid flat pages made only of repeated bordered boxes
- do not let decorative icons change the width allocation of card content
- keep cards, action panels, tabs, forms, and tables visually consistent across domains
- support dark mode by using semantic tokens and CSS variables instead of hardcoded light-only colors
- all new UI work must be checked in both light and dark themes

### State handling

- use shared loading states where available
- use explicit empty states for empty collections or optional content
- use explicit error states for page-level failures
- never leave blank sections with raw fallback text
- use skeletons that match the final layout
- keep row-level and button-level pending states visible for mutations

## Localization and direction

Portal language and direction resolve in this order:

1. `User.Language` from the authenticated profile, when present
2. the locally stored Portal language in `localStorage`
3. browser language
4. English `en-US`

On unauthenticated routes such as `/login`, the frontend skips the profile step and resolves language from local storage first, then the browser language, then English.

Direction `ltr` or `rtl` is applied at the document level by the frontend.

Implementation references:

- [`portal-localization.md`](portal-localization.md)
- `apps/portal/src/shared/lib/language.ts`
- `apps/portal/src/shared/lib/i18n/locales/*.json`
- `apps/portal/src/shared/lib/i18n/messages.ts`
- `apps/portal/src/shared/lib/i18n-core.ts`
- `apps/portal/src/shared/lib/i18n-provider.tsx`
- `apps/portal/src/shared/lib/use-portal-i18n.ts`

## Fast Refresh-safe exports

Keep React component modules in `apps/portal/src/**/*.tsx` compatible with Vite Fast Refresh:

- A component file should export React components and TypeScript types only.
- Move shared `cva(...)` helpers to sibling modules such as `button.variants.ts` or `input.variants.ts`.
- Move shared hooks, contexts, constants, and non-JSX helpers to sibling `*.ts` files.
- Do not export runtime helpers like `buttonVariants`, `useDataGrid`, `useFormField`, `dateInputStyles`, or component-local constants from the same `.tsx` file that exports components.

Preferred pattern:

```text
components/ui/button.tsx
components/ui/button.variants.ts
components/ui/data-grid.tsx
components/ui/data-grid-context.ts
```

## Local runtime and operating workflow

Use [`../../execution-guide.md`](../../execution-guide.md) first when you need to decide whether the task is frontend-only, backend-only, or cross-boundary.

For environment variables, Auth0 setup, `npm install`, `npm run dev`, polling mode, and build or lint commands, use [`../tools/portal-runtime.md`](../tools/portal-runtime.md).

For browser-facing local hostnames such as `dev.portal.basefaq.com`, use [`../tools/local-subdomains.md`](../tools/local-subdomains.md).

For the required frontend validation pass before merge, use [`../testing/validation-guide.md`](../testing/validation-guide.md).

## Current implementation notes

The Portal currently has these behavior notes:

- member creation depends on the Tenant Portal member API and may require an already-existing BaseFAQ user account depending on backend validation
- billing is backed by the Tenant Portal billing summary, subscription, invoice, and payment endpoints
- some QnA filtering remains constrained by backend list contracts and should stay page-scoped where the API surface is intentionally narrow

## Vendor baseline note

Vendor or demo assets under `apps/demos` are reference material only.

They are not authoritative BaseFAQ documentation, and outdated demo instructions or vendor-specific setup patterns must not be copied into the Portal implementation standards without first being aligned to the actual project architecture.

## UI implementation companion

For page composition, shared components, and UX consistency, use [`portal-app-ui-prompt-guidance.md`](portal-app-ui-prompt-guidance.md).
