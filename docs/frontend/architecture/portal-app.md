# BaseFAQ Portal App

## Purpose

This document is the main frontend guide for `apps/portal`. It explains what the app is responsible for, how it connects to the backend, and how to reason about Portal-specific implementation work.

## Scope

`apps/portal` is the tenant-facing web application for BaseFAQ. It is responsible for:

- authenticated workspace access
- QnA management screens for spaces, questions, answers, sources, tags, and activity
- tenant settings and profile flows
- member-management flows
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
- pagination contracts use `SkipCount`, `MaxResultCount`, and `Sorting`
- backend error payloads follow `{ errorCode, messageError, data }`
- Portal UI translation is frontend-owned, and backend DTOs do not provide translated labels

## Architecture and implementation rules

### Shared layouts and primitives

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

### Layout standards

#### List pages

- use `ListLayout`
- keep metrics in `SectionGrid`
- keep the main record surface in the shared table pattern

#### Detail pages

- use `DetailLayout`
- keep the main workflow in the left column
- keep the sidebar limited to overview, notes, or summary information
- keep onboarding or progress checklists in the main column, usually as the last block

#### Settings pages

- use `SettingsLayout`
- preserve the existing navigation rail unless the task is a deliberate settings redesign

### Form standards

- break large forms into named sections
- prefer tooltip-based contextual help over large paragraphs under every heading
- reuse shared field components before introducing custom wrappers
- keep copy concise and action-oriented

### Confirmation standards

- destructive actions should use `ConfirmAction`
- long-running or other meaningful run actions should also use an explicit confirmation step
- do not fire irreversible or expensive actions immediately on click

### Domain-specific UI rules

#### Question CTA inheritance

- question-level CTA fields inherit from the parent space CTA setting when the flow uses shared CTAs
- if the space-level CTA is disabled, question CTA controls should be visibly disabled and explained

#### Progress checklist placement

- space, question, answer, and source pages should keep progress guidance in the main content column
- the sidebar should not become the primary onboarding surface

### Visual hierarchy rules

- show status, key metrics, and compact summaries before dense text
- use muted supporting copy and stronger titles
- avoid flat pages made only of repeated bordered boxes
- do not let decorative icons change the width allocation of card content

### State handling

- use shared loading states where available
- use explicit empty states for empty collections or optional content
- use explicit error states for page-level failures
- never leave blank sections with raw fallback text

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

## Known functional gaps

The repository still has some important Portal gaps:

- member creation requires an already-existing BaseFAQ user email
- billing and invoice flows remain placeholder areas where the backend surface is missing
- some QnA list filtering still depends on backend-specific contracts and may remain page-scoped where the API surface is intentionally narrow

## Vendor baseline note

Vendor or demo assets under `apps/demos` are reference material only.

They are not authoritative BaseFAQ documentation, and outdated demo instructions or vendor-specific setup patterns must not be copied into the Portal implementation standards without first being aligned to the actual project architecture.

## UI implementation companion

For page composition, shared components, and UX consistency, use [`portal-app-ui-prompt-guidance.md`](portal-app-ui-prompt-guidance.md).
