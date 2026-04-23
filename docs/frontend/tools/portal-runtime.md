# Portal Runtime

## Purpose

This document is the operational guide for running `apps/portal` locally and validating its runtime dependencies.

## Environment variables

Copy `apps/portal/.env.example` to `.env` and configure the Portal client values.

| Variable | Meaning |
|---|---|
| `VITE_PORTAL_TENANT_API_URL` | Tenant Portal API base URL |
| `VITE_PORTAL_QNA_API_URL` | QnA Portal API base URL |
| `VITE_AUTH0_DOMAIN` | Auth0 domain |
| `VITE_AUTH0_AUDIENCE` | Auth0 API audience |
| `VITE_AUTH0_CLIENT_ID` | Portal SPA client id |
| `VITE_AUTH0_REDIRECT_URI` | optional Auth0 callback override |
| `VITE_AUTH0_LOGOUT_URI` | optional logout redirect override |

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

Set `VITE_AUTH0_CLIENT_ID` before expecting live sign-in to work.

### 3. Run the dev server

```bash
npm run dev
```

The Vite dev server runs on `http://localhost:5500`.

### 4. Polling fallback for Linux watcher limits

If Linux returns `ENOSPC: System limit for number of file watchers reached`, use the polling fallback instead:

```bash
npm run dev:polling
```

The same fallback is also enabled when either `CHOKIDAR_USEPOLLING=1` or `VITE_USE_POLLING=1` is set in the environment.

### 5. Build validation

```bash
npm run build
```

### 6. Linting

```bash
npm run lint
```

## Backend services required for realistic local testing

At minimum, run:

- `BaseFaq.Tenant.Portal.Api`
- `BaseFaq.QnA.Portal.Api`

Depending on the flow, you may also need:

- `BaseFaq.Tenant.BackOffice.Api` to manage tenant metadata in administrative scenarios
- `BaseFaq.Tenant.Worker.Api` when a flow depends on background processing side effects

Use [`../../backend/tools/local-development.md`](../../backend/tools/local-development.md) for the supported backend bootstrap flow.

## Auth0 local configuration

For `http://localhost:5500`, the Portal SPA client should allow:

- callback URL: `http://localhost:5500/login`
- logout URL: `http://localhost:5500/login`
- web origin: `http://localhost:5500`

If you use the local subdomain helper, also allow:

- callback URL: `http://dev.portal.basefaq.com/login`
- logout URL: `http://dev.portal.basefaq.com/login`
- web origin: `http://dev.portal.basefaq.com`

For refresh-token based SPA sessions, also configure:

- the Portal client as a Single Page Application with Refresh Token Rotation enabled
- the `https://basefaq.com` API audience with offline access enabled so Auth0 can issue refresh tokens for `offline_access`

Do not assume the Swagger UI client id used by backend APIs is also valid for the Portal app. The Portal needs its own SPA client configuration unless the same Auth0 application was explicitly set up for both use cases.

The frontend logout handler redirects to `VITE_AUTH0_LOGOUT_URI` when set, otherwise it falls back to `{origin}{BASE_URL}login`. That final URL must be present in the Auth0 application's `Allowed Logout URLs`.

## Localization and direction

Portal language and direction now resolve in this order:

1. `User.Language` from the authenticated profile, when present
2. the locally stored Portal language in `localStorage`
3. browser language
4. English `en-US`

On unauthenticated routes such as `/login`, the frontend skips the profile step and resolves language from local storage first, then the browser language, then English.

User-facing language controls are available in:

- the login screen header
- profile settings
- the top toolbar beside notifications

For the full localization model, use [`../architecture/portal-localization.md`](../architecture/portal-localization.md).

## Local subdomain option

If you want a host-based experience that resembles shared subdomains, use the helper documented in [`local-subdomains.md`](local-subdomains.md). When active, the same app is also reachable at `http://dev.portal.basefaq.com`.

## Containerized alternative

If you want only the frontend in Docker:

```bash
./devops/local/docker/frontend.sh
```

If you also want the backend APIs and worker in containers, start `./devops/local/docker/backend.sh` first or use `./devops/local/docker/docker.sh` for the full stack.

PowerShell equivalents live beside these scripts under `devops/local/docker/*.ps1`.

The Portal-only compose file is `devops/local/docker/docker-compose.frontend.yml`. The full-stack helper `./devops/local/docker/docker.sh` combines `devops/local/docker/docker-compose.backend.yml` and `devops/local/docker/docker-compose.frontend.yml`. The Portal is exposed on `http://localhost:5500`.

## Current gaps

- member adds require an already-existing BaseFAQ user email; invitation acceptance is not exposed yet
- billing and invoice flows remain placeholder areas where the backend surface is still limited

## Useful validation commands

```bash
npm run build
npm run lint
```

For the required manual regression pass before merge, use [`../testing/validation-guide.md`](../testing/validation-guide.md).
