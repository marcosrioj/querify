# Demo6 Next.js Multitenant Micro-frontend Specification

## Objective

Create a Next.js TypeScript micro-frontend, visually based on the Demo6 layout, that supports the BaseFaq SaaS acquisition and workspace journey end to end:

- public discovery and pricing
- plan selection and checkout
- tenant provisioning and onboarding
- tenant profile and AI configuration
- FAQ authoring
- public FAQ preview

The first delivery must run in `mock` mode without depending on unfinished services. The same UI must also be able to switch to `live` mode against the current BaseFaq APIs where contracts already exist.

## Repo Inputs That Shaped This Spec

- The multi-agent document requires explicit boundaries, English deliverables, PR-ready artifacts, and human review for high-risk tenant or API changes.
- The frontend workspace document sets the Demo6 Next.js TypeScript demo as the implementation baseline and requires explicit API adapters.
- The root README documents the current API hosts, local ports, Auth0 assumptions, `X-Tenant-Id`, and `X-Client-Key`.
- The current `dotnet/` controllers and DTOs define tenant management, FAQ management, and public FAQ consumption, but they do not yet define billing, checkout, subscription, or provisioning APIs.

## Product Scope

### Included

- Marketing pages for value proposition, pricing, and plan comparison
- Self-serve plan purchase flow
- Mock checkout success and provisioning status flow
- Authenticated tenant workspace using the Demo6 shell
- Tenant switcher driven by tenant slug in the URL
- User profile settings
- Tenant settings for name, edition display, client key, and AI provider key configuration
- FAQ list, FAQ editor, FAQ item authoring, and generation-request action
- Public FAQ preview using the current public API contract shape
- Dual adapter mode: `mock` and `live`

### Excluded From The First Demo

- Real payment processor integration
- Real webhook processing
- Full back-office admin operations
- Tag, content-reference, and advanced workflow authoring
- Subdomain routing as the primary tenancy mechanism

## Recommended UX And Route Map

### Public Route Group

- `/`
- `/pricing`
- `/plans`
- `/checkout`
- `/checkout/success`
- `/signin`
- `/signup`

### Onboarding Route Group

- `/onboarding/workspace`
- `/onboarding/provisioning`

### Tenant Workspace Route Group

- `/t/[tenantSlug]/overview`
- `/t/[tenantSlug]/faq`
- `/t/[tenantSlug]/faq/[faqId]`
- `/t/[tenantSlug]/settings/profile`
- `/t/[tenantSlug]/settings/ai`
- `/t/[tenantSlug]/settings/public-access`
- `/t/[tenantSlug]/billing/plan`
- `/t/[tenantSlug]/billing/history`
- `/t/[tenantSlug]/billing/payment-methods`

### Public Preview Route Group

- `/preview/[tenantSlug]`

## Architecture

### Shell Strategy

- Use a lighter marketing shell for public acquisition pages.
- Use the Demo6 layout for authenticated tenant workspace pages.
- Keep the Demo6 shell copy local to the micro-frontend so the demo source remains unchanged.

### Runtime Strategy

- Use the Next.js App Router.
- Prefer Server Components for initial page data and shell composition.
- Use Client Components only where interactivity is required: forms, filters, editors, billing actions, and preview interactions.
- Use TanStack Query for mutation flows and client-side refresh.

### Integration Strategy

- Do not call the BaseFaq services directly from browser components when tenant or client-key headers are required.
- Implement a thin BFF layer with Next route handlers.
- The BFF layer owns:
  - Bearer token forwarding
  - tenant slug to tenant id resolution
  - `X-Tenant-Id` injection for FAQ Portal calls
  - `X-Client-Key` injection for public FAQ preview calls
  - adapter switching between `mock` and `live`

### Tenancy Strategy

- URL-facing tenancy uses `tenantSlug`.
- Server-side tenant resolution uses `GET /api/tenant/tenants/GetAll` and caches the mapping in the session layer.
- Only the BFF layer should know and forward the tenant GUID.
- Path-based tenancy is the first delivery.
- The implementation should keep a clear seam for later subdomain resolution because the repo already contains simulated subdomain tooling in `local/env/simulatedev/`.

### API Mode Strategy

- `mock` mode is the default implementation target for the first runnable demo.
- `live` mode integrates with:
  - Tenant Portal API
  - FAQ Portal API
  - FAQ Public API
- Missing service domains stay mocked in both the first demo and the initial live integration:
  - commerce
  - checkout
  - subscription lifecycle
  - payment methods
  - invoice history
  - provisioning status

## Recommended App Structure

```text
frontend/microfrontends/demo6-tenant-commerce/
  app/
    (public)/
    (onboarding)/
    (workspace)/
    api/
  components/
    shell/
    shared/
  features/
    auth/
    pricing/
    checkout/
    provisioning/
    tenants/
    faq/
    billing/
    settings/
    preview/
  lib/
    api/
    config/
    session/
    tenancy/
    validation/
  mocks/
    fixtures/
    handlers/
  tests/
    integration/
    e2e/
```

## Screen To Data Mapping

| Screen | Primary Data Sources | Mode Notes |
|---|---|---|
| Pricing | commerce plans | always mock for first delivery |
| Checkout | commerce checkout session | always mock for first delivery |
| Provisioning | provisioning status, tenant summary | mock first, later can mix live tenant portal |
| Tenant overview | tenant summary, profile, subscription summary | mixed live plus mock |
| FAQ list | FAQ Portal `GET /api/faqs/faq` | live-ready |
| FAQ detail/editor | FAQ Portal `GET/PUT /api/faqs/faq/{id}` and FAQ Item CRUD | live-ready |
| Public preview | FAQ Public `GET /api/faqs/faq`, search, vote | live-ready |
| AI settings | tenant portal providers and configured keys | live-ready |
| Billing pages | subscription, invoices, payment methods | mock first |

## Data And Contract Rules

- Validate all request and response shapes with Zod at the adapter boundary.
- Keep response normalization in one place per service domain.
- Use optimistic updates sparingly and only for low-risk UX improvements.
- Never store raw access tokens in local storage.
- Never expose AI provider keys after initial submission.
- Treat tenant ids, client keys, and access tokens as server-owned context.
- Keep generated types and hand-written adapter types separate so the transport layer does not leak into the UI layer.

## Multi-tenant Rules

- A signed-in user may have multiple tenants.
- Tenant selection must be explicit in the UI.
- Changing the active tenant must invalidate tenant-scoped queries and route to the selected `tenantSlug`.
- FAQ authoring depends on `X-Tenant-Id`.
- Public preview depends on a tenant-specific client key.
- Plan purchase should map directly to the existing `TenantEdition` enum:
  - `Free`
  - `Starter`
  - `Pro`
  - `Business`
  - `Enterprise`

## Best Practices Required In The First Delivery

- Preserve explicit service boundaries in the frontend. Do not create one generic `fetchBaseFaq()` wrapper that hides service ownership.
- Keep route handlers thin and push domain logic into feature adapters.
- Favor feature folders over page-local data logic.
- Use loading skeletons, empty states, and inline errors for every async surface.
- Build every mutating form with schema validation, disabled submit states, and server error rendering.
- Keep shell navigation resilient to partial service outage. Billing mocks failing must not break FAQ authoring.
- Make mock fixtures deterministic so screenshots, tests, and local demos remain stable.
- Add an obvious environment banner when the app is using mock mode.

## Future Agent Preconditions

- Work only in the new micro-frontend folder and related shared frontend folders unless the task explicitly expands scope.
- Do not modify the demo reference source under `frontend/demos/`.
- Stop and request human review before changing backend contracts, tenant persistence, or auth infrastructure.
- Produce a runnable local validation path, not only code changes.

## Open Questions To Keep Explicit

- Whether self-serve tenant creation should ultimately call Tenant Portal `CreateOrUpdate` directly or rely on a backend provisioning workflow
- Whether the live app should be path-based, subdomain-based, or hybrid for tenant resolution
- Whether FAQ generation status needs a polling endpoint beyond the existing `generation-request` acceptance flow
- Whether a dedicated gateway service should replace the Next BFF layer later
