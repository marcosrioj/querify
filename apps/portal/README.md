# BaseFAQ Portal

Customer-facing Portal frontend for BaseFAQ, built on the repository's React/Vite frontend baseline and wired to the Portal-side .NET APIs in this solution.

This file is the project-local readme beside the app source. For the broader frontend knowledge base, see [`../../docs/frontend/portal-app.md`](../../docs/frontend/portal-app.md).

## What this app uses

Confirmed backend integrations:
- `dotnet/BaseFaq.Tenant.Portal.Api`
- `dotnet/BaseFaq.Faq.Portal.Api`

Confirmed backend constraints reflected in the frontend:
- Auth uses Auth0 JWTs
- FAQ and tenant-scoped Tenant Portal endpoints require `X-Tenant-Id`
- Pagination contract is `SkipCount`, `MaxResultCount`, `Sorting`
- API error contract is `{ errorCode, messageError, data }`

Portal-only boundary:
- No BackOffice endpoints are consumed
- No BackOffice route concerns are exposed in the UI
- Portal translations remain frontend-owned

## Structure

```text
src/
  app/
  domains/
  platform/
  providers/
  shared/
  components/   # reused Metronic UI baseline
  css/          # reused Metronic styling baseline
```

Notes:
- `domains/faq`, `domains/faq-items`, and `domains/content-refs` use the real CRUD APIs
- `domains/tenants` and `domains/settings/profile` use the real Tenant Portal APIs
- `domains/members` uses the isolated TenantUser API in Tenant Portal
- parts of `domains/ai` remain placeholder shells where the backend surface is missing
- `shared/lib/language.ts` defines the built-in Portal language options and text direction metadata
- `shared/lib/i18n/locales/*.json` stores the frontend-owned locale catalogs
- `shared/lib/i18n/messages.ts`, `shared/lib/i18n-core.ts`, `shared/lib/i18n-provider.tsx`, and `shared/lib/use-portal-i18n.ts` load translations and apply `lang` / `dir`

## Local setup

1. Install dependencies

```bash
npm install --legacy-peer-deps
```

2. Configure environment

```bash
cp .env.example .env
```

Set `VITE_AUTH0_CLIENT_ID` to a real Portal SPA Auth0 client before expecting live sign-in to work.

For local Portal login, the Auth0 application must allow:
- Callback URL: `http://localhost:5500/login`
- Logout URL: `http://localhost:5500/login`
- Web Origin: `http://localhost:5500`

If you run the local `simulatedev` reverse proxy helper, the same Portal app is also exposed at `http://dev.portal.basefaq.com`, so Auth0 should additionally allow:
- Callback URL: `http://dev.portal.basefaq.com/login`
- Logout URL: `http://dev.portal.basefaq.com/login`
- Web Origin: `http://dev.portal.basefaq.com`

The portal logout flow calls Auth0 `/v2/logout` with `returnTo={origin}{BASE_URL}login` by default. If that URL is not listed in the Auth0 application's `Allowed Logout URLs`, Auth0 will reject the redirect after sign-out. Set `VITE_AUTH0_LOGOUT_URI` only when the post-logout target must differ from the default login route.

Do not reuse the backend `SwaggerOptions:swaggerAuth:ClientId` value as the Portal SPA client unless that Auth0 application has also been updated to allow the Portal callback URL above. The backend README documents that client for Swagger UI callback pages on ports `5000`, `5002`, and `5010`.

3. Run the app

```bash
npm run dev
```

The Vite dev server runs on `http://localhost:5500`.

When `local/env/simulatedev/setup-subdomains.sh` or `setup-subdomains.ps1` is active, the app is also reachable through the local hostname `http://dev.portal.basefaq.com`.

4. Build validation

```bash
npm run build
```

## Current gaps

- Member adds require an already-existing BaseFAQ user email; invitation acceptance is not exposed yet
- No Portal AI jobs/progress listing API exists yet
- FAQ, FAQ Item, and Content Ref text search/filtering are client-side on the loaded page because the backend list contracts do not expose search parameters yet

## Localization

Portal localization now resolves in this order:

1. `User.Language` from the authenticated profile, when present
2. the locally stored Portal language in `localStorage`
3. browser language
4. English (`en-US`)

On unauthenticated routes such as `/login`, the app skips the profile lookup and resolves language from local storage first, then the browser language, then English.

Language can be changed from:

- the login screen header selector
- the top toolbar beside notifications
- profile settings

The login selector updates only the frontend-owned stored language. The authenticated toolbar and profile flows also persist the preference through the Tenant Portal user profile endpoint.

Portal UI translation is frontend-owned. Keep UI copy in the frontend and route it through the shared i18n helpers instead of expecting backend-translated DTO labels.

Localization catalogs now live in `src/shared/lib/i18n/locales/`, one JSON file per supported locale. Keep keys aligned across the 20 supported Portal languages and let `en-US` remain the fallback base.

Dynamic UI strings should prefer placeholder keys such as `Delete FAQ "{name}"?` or `Search: {value}` so `shared/lib/i18n-core.ts` can resolve runtime values without hardcoding every rendered variation.

## Useful validation commands

```bash
npm run build
npm run lint
```

The app is also included in the containerized stack exposed by `docker/docker-compose.yml` as `basefaq.portal.app` on `http://localhost:5500`.
