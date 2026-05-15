---
name: querify-portal-frontend
description: "Querify Portal frontend implementation rules. Use for React/Vite pages, routes, hooks, API clients, shared UI, shell, SignalR, localization, and frontend validation."
when_to_use: "Use when editing apps/portal/**, frontend docs, user-facing copy, responsive layout, Portal runtime, or localization catalogs."
paths:
  - "apps/portal/**"
  - "docs/frontend/**"
  - "docs/behavior-change-playbook.md"
---

# Querify Portal Frontend

Read the owning docs before changing code:

1. `docs/frontend/architecture/portal-app.md`
2. `docs/frontend/architecture/portal-app-ui-prompt-guidance.md`
3. `docs/frontend/architecture/portal-getting-started-guidance.md` when setup progress, empty states, or next actions change.
4. `docs/frontend/architecture/portal-localization.md` when copy, API errors, language, or timezones change.
5. `docs/frontend/testing/validation-guide.md`

## App boundaries

- `apps/portal` is the tenant-facing authenticated Portal, not BackOffice.
- Reuse `src/app`, `src/domains`, `src/platform`, `src/providers`, `src/shared/ui`, `src/shared/layout`, `src/shared/lib`, and `src/shared/realtime` structure.
- Portal integrates with Tenant Portal API and QnA Portal API.
- Tenant-scoped backend calls need `X-Tenant-Id`.
- Domain pages must load authoritative state through API queries even when SignalR accelerates UX.

## Shell and layout

- Keep the workspace switcher in the sidebar header.
- The top toolbar owns breadcrumbs, current page title, command search, language, notifications, and user menu.
- Do not render a second competing page title in the page body.
- Fixed sidebar is desktop only. Below `xl`, use mobile/tablet header and drawer.
- Keep `useIsMobile`, `PortalSidebar`, and `MobileHeader` breakpoints aligned.
- The Portal must work at 320 CSS pixels. Add `min-w-0` and wrapping where flex, cards, tables, dialogs, sheets, popovers, filters, and actions can overflow.

## UI composition

- Use shared layout primitives from `src/shared/layout/page-layouts.tsx`.
- Use shared UI before custom markup: `ActionPanel`, `ActionButton`, `ConfirmAction`, `ContextHint`, `SearchSelect`, `SearchSelectField`, `DataTable`, `EmptyState`, `ErrorState`, loading states, status badges, and enum UI metadata.
- List pages use `ListLayout`; filters go in the `filters` slot; records render as stacked cards below `xl` and table from `xl` upward unless intentionally different.
- Detail pages use `DetailLayout`; primary action surface is `ActionPanel layout=\"bar\"` in the main column; right rail should usually be one consolidated overview card.
- Relationship sections use local tabs, scoped pagination, scoped filters, and `ChildListPagination` for more than five items. Do not redirect to global list pages just to inspect related records.
- Destructive or meaningful long-running actions require `ConfirmAction`.

## Forms and state

- Large forms are sectioned.
- Every editable field has a concise field-level explanation via `description`, `hint`, or label plus `ContextHint`.
- Backend-list selects use `SearchSelect` or `SearchSelectField`; enum-only controls use the normal `Select`.
- Use explicit loading, empty, error, pending, success, and confirmation states.
- Use skeletons that preserve final layout footprint.
- Mutation pending state belongs at the affected button or row.

## Localization and time

- UI copy is frontend-owned. Backend DTOs do not provide translated labels.
- Add `en-US` keys first, then keep every locale JSON file aligned with the same key set.
- Do not put Portuguese or other non-English source strings directly into components, tests, docs, or comments except locale values or quoted external material.
- API errors shown to users go through `src/platform/api/api-error.ts` and Portal i18n catalogs.
- Render backend UTC instants through `src/shared/lib/time-zone.ts` or an equivalent formatter with explicit `timeZone`.
- Do not rely on browser local timezone for user-facing timestamps.

## Validation

- Required commands from `apps/portal`: `npm run lint` and `npm run build`.
- Use `npm run dev` or `npm run dev:polling` for browser validation.
- For shell, shared UI, list, filter, action, dialog, popover, table, or pagination changes, check 320, 360, 375, 414, 768, 1024, 1279, 1280, and desktop widths.
- Check light and dark mode when touching colors, layout, cards, tables, forms, actions, or badges.
