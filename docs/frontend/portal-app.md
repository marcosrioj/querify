# BaseFAQ Portal App

## Purpose

This document is the main frontend guide for `apps/portal`. It explains what the app is responsible for, how it connects to the backend, and how to run it locally.

## Scope

`apps/portal` is the tenant-facing web application for BaseFAQ. It is responsible for:

- authenticated workspace access
- FAQ management screens
- tenant settings and profile flows
- member-management flows
- tenant-side AI/provider settings where the backend already supports them

It is not the BackOffice UI and it does not own BackOffice API concerns.

## Technology stack

- React
- Vite
- Tailwind-based UI baseline
- TanStack Query and Table
- Auth0 SPA authentication
- frontend-owned localization and RTL/LTR handling

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

The portal currently integrates with:

- `BaseFaq.Tenant.Portal.Api`
- `BaseFaq.Faq.Portal.Api`

Operational constraints reflected in the frontend:

- protected flows require Auth0 JWT authentication
- tenant-scoped backend calls require `X-Tenant-Id`
- pagination contracts use `SkipCount`, `MaxResultCount`, and `Sorting`
- backend error payloads follow `{ errorCode, messageError, data }`
- portal UI translation is frontend-owned; backend DTOs do not provide translated labels

## Environment variables

Copy `.env.example` to `.env` and adjust as needed.

| Variable | Meaning |
|---|---|
| `VITE_PORTAL_TENANT_API_URL` | Tenant Portal API base URL |
| `VITE_PORTAL_FAQ_API_URL` | FAQ Portal API base URL |
| `VITE_AUTH0_DOMAIN` | Auth0 domain |
| `VITE_AUTH0_AUDIENCE` | Auth0 API audience |
| `VITE_AUTH0_CLIENT_ID` | SPA client id used by the Portal frontend |
| `VITE_AUTH0_LOGOUT_URI` | optional post-logout redirect URI |

## Local setup

### 1. Install dependencies

```bash
cd apps/portal
npm install --legacy-peer-deps
```

### 2. Create the environment file

```bash
cp .env.example .env
```

Set `VITE_AUTH0_CLIENT_ID` before expecting live login to work.

### 3. Run the dev server

```bash
npm run dev
```

The Vite dev server runs on `http://localhost:5500`.

### 4. Build validation

```bash
npm run build
```

## Required backend services for realistic local testing

At minimum, run:

- `BaseFaq.Tenant.Portal.Api`
- `BaseFaq.Faq.Portal.Api`

Depending on the flow, you may also need:

- `BaseFaq.Tenant.BackOffice.Api` to manage tenant metadata or AI provider setup
- `BaseFaq.AI.Api` if you are testing async AI-related workflows

## Auth0 local configuration

For `http://localhost:5500`, the Portal SPA client should allow:

- callback URL: `http://localhost:5500/login`
- logout URL: `http://localhost:5500/login`
- web origin: `http://localhost:5500`

If you use the local subdomain helper, also allow:

- callback URL: `http://dev.portal.basefaq.com/login`
- logout URL: `http://dev.portal.basefaq.com/login`
- web origin: `http://dev.portal.basefaq.com`

Do not assume the Swagger UI client id used by backend APIs is also valid for the Portal app. The Portal needs its own SPA client configuration unless the same Auth0 application was explicitly set up for both use cases.

The frontend logout handler redirects to `VITE_AUTH0_LOGOUT_URI` when set, otherwise it falls back to `{origin}{BASE_URL}login`. That final URL must be present in the Auth0 application's `Allowed Logout URLs`.

## Localization and direction

Portal language and direction now resolve in this order:

1. `User.Language` from the authenticated profile
2. browser language
3. English (`en-US`)

Direction (`ltr` or `rtl`) is applied at the document level by the frontend.

Implementation references:

- [`portal-localization.md`](portal-localization.md)
- `apps/portal/src/shared/lib/language.ts`
- `apps/portal/src/shared/lib/i18n/locales/*.json`
- `apps/portal/src/shared/lib/i18n/messages.ts`
- `apps/portal/src/shared/lib/i18n-core.ts`
- `apps/portal/src/shared/lib/i18n.tsx`

User-facing language controls are available in:

- profile settings
- the top toolbar beside notifications

## Local subdomain option

If you want a host-based experience that resembles shared subdomains, use the helper documented in [`../devops/local-development.md`](../devops/local-development.md). When active, the same app is also reachable at `http://dev.portal.basefaq.com`.

## Known functional gaps

The repository README for the portal already calls out important product gaps that still matter:

- member creation requires an already-existing BaseFAQ user email
- billing and invoice flows remain placeholder areas where the backend surface is missing
- AI job/progress listing is not yet fully exposed
- some FAQ search/filter behavior is still client-side because backend list contracts do not yet expose search parameters

## UI implementation rules

For page composition, shared components, and UX consistency, use [`portal-app-ui-prompt-guidance.md`](portal-app-ui-prompt-guidance.md).
