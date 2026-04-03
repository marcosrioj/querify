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

3. Run the app

```bash
npm run dev
```

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
