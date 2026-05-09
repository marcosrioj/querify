# Querify Portal App

## Purpose

This document is the main frontend guide for `apps/portal`. It covers scope, tech stack, backend contracts, architecture rules, and Portal-specific conventions. For shared UI patterns and page composition rules, use [`portal-app-ui-prompt-guidance.md`](portal-app-ui-prompt-guidance.md). For Getting Started, setup progress, empty states, and recommended-next-action logic, use [`portal-getting-started-guidance.md`](portal-getting-started-guidance.md).

## Scope

`apps/portal` is the tenant-facing web application for Querify. It is responsible for:

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
- `@microsoft/signalr` for Portal realtime notifications from backend hubs

## Repository structure

```text
apps/portal/
  src/
    app/           # bootstrap, router, layouts
    domains/       # feature domains (qna, settings, billing, members, auth, workspace)
    platform/      # auth, permissions, runtime, API client, telemetry, tenant context
    providers/     # React providers (Auth0, i18n, QueryClient, themes)
    shared/
      ui/          # reusable components (tables, forms, placeholders, badges)
      layout/      # page layout primitives, shells, navigation
      lib/         # utilities, i18n, language config
      realtime/    # SignalR Portal notification client/provider/hooks
      constants/   # backend enum UI metadata
      types/       # shared TypeScript types
    components/    # demo/legacy components
    css/           # global styles, Tailwind extensions
```

Reuse the existing structure instead of creating page-specific islands.

## Backend integrations

The Portal currently integrates with:

- `Querify.Tenant.Portal.Api`
- `Querify.QnA.Portal.Api`

Operational constraints reflected in the frontend:

- protected flows require Auth0 JWT authentication
- tenant-scoped backend calls require `X-Tenant-Id`
- Portal realtime connects to `Querify.QnA.Portal.Api` through SignalR at
  `/api/qna/hubs/portal-notifications`, sends the Auth0 access token through the SignalR access
  token factory, and normally connects without a `tenantId` query so the backend can subscribe the
  session to every QnA workspace the user is allowed to access
- tenant summaries expose `module`, backed by `ModuleEnum` values: Tenant, QnA, Direct, Broadcast, and Trust
- pagination contracts use `SkipCount`, `MaxResultCount`, and `Sorting`
- backend error payloads follow `{ ErrorCode, MessageError, Data }`; the frontend also accepts camelCase fields defensively
- Portal UI translation is frontend-owned; backend DTOs do not provide translated labels
- SignalR notifications are UX acceleration only. Domain pages must still load authoritative state
  through normal API queries on page load and refresh.

API errors shown in toasts, confirmation failures, or page placeholders must go through `src/platform/api/api-error.ts`. That module normalizes `MessageError`, maps dynamic backend messages to stable frontend messages, and sends the result through the Portal i18n catalogs. Do not render raw backend error strings directly in components.

## Shell and navigation architecture

- Keep the tenant/workspace switcher in the sidebar header. Do not move it to the top toolbar.
- Keep primary navigation grouped by user mental model: Workspace, Administration, and Account.
- The top toolbar is for route context and global utilities: breadcrumbs, command search, language, notifications, and user menu.
- Render page location as one toolbar trail: parent navigation links plus the current page title. Page headers register the current title, back target, and hint text instead of rendering a second competing title in the page body.
- Toolbar breadcrumb labels must truncate on one line using `min-w-0`; do not wrap or force page overflow.
- The QnA module navigation is not a primary app navigation replacement. Use it only inside a domain screen for child and relationship management.
- Breadcrumbs for QnA child records must show the full ownership line (e.g., Space name → Question → Activity). Only the Space detail page itself should show the Space name.
- The fixed sidebar is a desktop-only pattern. Below the `xl` breakpoint, use the mobile/tablet header and drawer.
- Keep the JavaScript shell breakpoint in `useIsMobile` aligned with the Tailwind breakpoint used by the sidebar and mobile header. Do not let React render one shell mode while CSS displays another.
- The Portal must work down to a 320 CSS pixel viewport. Root, shell, and page flex containers must allow shrinking with `min-w-0`.

## Realtime Notifications

The Portal notification layer is user-global, not scoped to the currently selected workspace.

- `PortalNotificationsProvider` opens one SignalR connection per authenticated Portal session.
  Tenant switching must not recreate the connection or clear the notification inbox.
- The normal frontend connection calls `buildPortalNotificationsHubUrl()` without `tenantId`. The
  hub authorizes the user, loads the allowed tenant cache, and joins the connection to all allowed
  QnA tenant/module groups for the user. A `tenantId` query is still supported as a scoped
  connection mode, but the hub must reject it when the tenant is not allowed for the current user.
- The inbox is an in-memory browser inbox for the authenticated user. It is cleared on logout or
  user change, not on workspace change. It is not a durable notification store.
- Every notification item must identify the notification's `tenantId` using the user's known
  workspace summaries. If the workspace cannot be resolved locally, show a safe fallback such as
  `Unknown workspace`.
- Notification list rows should stay concise: title, status/resource metadata, workspace, and time.
  Do not render payload descriptions in the list by default.
- Presentation is centralized in `shared/realtime/portal-notification-presenters.ts`. Source upload
  status is only the first presenter; new notification types should add presenters instead of
  special-casing the shell menu.
- Domain-specific realtime handlers may update tenant-scoped query caches only after checking that
  `notification.tenantId === currentTenantId`. The global inbox should still record notifications
  from every allowed workspace.
- Clicking a notification for the current workspace marks it read and navigates normally. Clicking a
  notification for another workspace must ask for confirmation before changing workspace. On
  confirmation, refresh the allowed-tenant cache, set the notification workspace as current, then
  navigate to the notification target. If the workspace is no longer available to the user, show an
  error and do not navigate.
- Any new notification UI strings must be added to every file in
  `apps/portal/src/shared/lib/i18n/locales/*.json`.

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

## Localization and direction

Portal language and direction resolve in this order:

1. `User.Language` from the authenticated profile, when present
2. the locally stored Portal language in `localStorage`
3. browser language
4. English `en-US`

On unauthenticated routes such as `/login`, the frontend skips the profile step and resolves from local storage, then browser language, then English.

Direction (`ltr` or `rtl`) is applied at the document level by the frontend.

Implementation references:

- [`portal-localization.md`](portal-localization.md)
- `apps/portal/src/shared/lib/language.ts`
- `apps/portal/src/shared/lib/i18n/locales/*.json`

## Date and time rendering

The Portal treats backend timestamps as UTC instants. Backend DTO fields that end in `Utc` must be
rendered through `apps/portal/src/shared/lib/time-zone.ts` or an equivalent formatter that passes an
explicit `timeZone` option to `Intl.DateTimeFormat`.

Timezone resolution for authenticated Portal screens is:

1. `User.TimeZone` from the Tenant Portal profile endpoint, when present and supported by the browser
2. `UTC`

The browser timezone is available as a selectable IANA timezone value in the profile picker, but it
is not an implicit display fallback. Do not rely on `Intl.DateTimeFormat(...)`, `toLocaleString(...)`,
`toLocaleDateString(...)`, or `toLocaleTimeString(...)` without an explicit `timeZone` for user-facing
timestamps, because that silently applies the browser's local timezone. Sorting and comparisons may
use `new Date(value).getTime()` because those operations compare instants rather than render local
time.

## Implementation rules

For shared components, layout standards, form rules, relationship sections, action patterns, visual hierarchy, responsive behavior, state handling, and domain-specific UI patterns, use [`portal-app-ui-prompt-guidance.md`](portal-app-ui-prompt-guidance.md).

For the Portal's Getting Started sequence, setup-progress completion criteria, empty-state inventory, page hints, and recommended-next-action behavior, use [`portal-getting-started-guidance.md`](portal-getting-started-guidance.md).

## Current implementation notes

- Member creation depends on the Tenant Portal member API and may require an already-existing Querify user account depending on backend validation.
- Billing is backed by the Tenant Portal billing summary, subscription, invoice, and payment endpoints.
- Some QnA filtering remains constrained by backend list contracts and should stay page-scoped where the API surface is intentionally narrow.

## Vendor baseline note

Vendor or demo assets under `apps/demos` are reference material only. Outdated demo instructions or vendor-specific setup patterns must not be copied into the Portal implementation standards without first being aligned to the actual project architecture.

## Local runtime and operating workflow

Use [`../../execution-guide.md`](../../execution-guide.md) first when you need to decide whether the task is frontend-only, backend-only, or cross-boundary.

For environment variables, Auth0 setup, `npm install`, `npm run dev`, and build or lint commands, use [`../tools/portal-runtime.md`](../tools/portal-runtime.md).

For browser-facing local hostnames such as `dev.portal.querify.net`, use [`../tools/local-subdomains.md`](../tools/local-subdomains.md).

For the required frontend validation pass before merge, use [`../testing/validation-guide.md`](../testing/validation-guide.md).
