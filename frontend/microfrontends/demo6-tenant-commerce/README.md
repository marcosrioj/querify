# Demo6 Tenant Commerce Micro-frontend

This folder is reserved for a Demo6-based Next.js TypeScript micro-frontend that covers self-serve plan purchase, tenant onboarding, and tenant FAQ workspace management.

## Baseline

- UI baseline: `frontend/demos/metronic-tailwind-react-demos/typescript/nextjs`
- Preferred shell: `frontend/demos/metronic-tailwind-react-demos/typescript/nextjs/app/components/layouts/demo6`
- Do not edit the demo source in place. Copy and adapt the required layout pieces into this micro-frontend when implementation starts.

## Route Ownership

- Public acquisition: `/`, `/pricing`, `/plans`, `/checkout`, `/checkout/success`
- Identity and onboarding: `/signin`, `/signup`, `/onboarding/workspace`, `/onboarding/provisioning`
- Tenant workspace: `/t/[tenantSlug]/overview`, `/t/[tenantSlug]/faq`, `/t/[tenantSlug]/faq/[faqId]`
- Tenant settings: `/t/[tenantSlug]/settings/profile`, `/t/[tenantSlug]/settings/ai`, `/t/[tenantSlug]/settings/public-access`
- Tenant billing: `/t/[tenantSlug]/billing/plan`, `/t/[tenantSlug]/billing/history`, `/t/[tenantSlug]/billing/payment-methods`
- Public preview: `/preview/[tenantSlug]`

## API Adapters

- Tenant Portal API for tenant summaries, profile, client key, and AI provider configuration
- FAQ Portal API for authenticated FAQ authoring
- FAQ Public API for public preview and vote/search flows
- Mock Commerce API for plan catalog, checkout, subscription, invoices, and payment methods
- Mock Provisioning API for post-checkout tenant creation state
- Mock Identity API for demo-mode session bootstrapping

## Shared Dependency Expectations

- Next.js App Router with TypeScript
- React 19 and the Demo6 shell components adapted from the baseline
- TanStack Query for server state
- React Hook Form plus Zod for forms and schema validation
- Thin API adapter layer between UI features and route handlers

## Tenant And Auth Assumptions

- Live mode uses Bearer tokens for protected APIs.
- FAQ Portal requests must add `X-Tenant-Id` on the server side only.
- FAQ Public preview requests must add `X-Client-Key` on the server side only.
- Tenant slug is the URL-facing identifier. Tenant GUID stays in the adapter and BFF layers.
- Mock mode must work without Auth0 or live backend services.

## Local Validation Steps

- `npm run lint`
- `npm run build`
- Feature smoke tests for pricing, checkout, onboarding, tenant switch, FAQ list, FAQ edit, and public preview

## Task Package

Use the implementation brief in `todo/demo6-nextjs-multitenant-microfrontend/`.
