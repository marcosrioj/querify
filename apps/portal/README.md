# BaseFAQ Portal

Customer-facing Portal frontend for BaseFAQ, built from the provided Metronic `demo6` Vite baseline and wired to the real Portal-side .NET APIs in this repository.

## What this app uses

Confirmed backend integrations:
- `dotnet/BaseFaq.Tenant.Portal.Api`
- `dotnet/BaseFaq.Faq.Portal.Api`

Confirmed backend constraints reflected in the frontend:
- Auth uses Auth0 JWTs
- FAQ endpoints require `X-Tenant-Id`
- Pagination contract is `SkipCount`, `MaxResultCount`, `Sorting`
- API error contract is `{ errorCode, messageError, data }`

Portal-only boundary:
- No BackOffice endpoints are consumed
- No BackOffice route concerns are exposed in the UI

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
- `domains/members` uses an explicit temporary local adapter because the Portal members API does not exist yet
- `domains/billing` and parts of `domains/ai` remain placeholder shells where the backend surface is missing

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
- Web Origin: `http://localhost:5500`

If you run the local `simulatedev` reverse proxy helper, the same Portal app is also exposed at `http://dev.portal.basefaq.com`, so Auth0 should additionally allow:
- Callback URL: `http://dev.portal.basefaq.com/login`
- Web Origin: `http://dev.portal.basefaq.com`

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

- No Portal members API exists yet
- No Portal billing/invoice API exists yet
- No Portal AI jobs/progress listing API exists yet
- FAQ, FAQ Item, and Content Ref text search/filtering are client-side on the loaded page because the backend list contracts do not expose search parameters yet

## Validation status

- `npm run build`: passing
- `npm run lint`: blocked by the inherited ESLint/AJV toolchain issue in the baseline dependency stack
- `docker compose -p bf_services -f docker/docker-compose.yml up -d --build`: includes `basefaq.portal.app` on `http://localhost:5500`
