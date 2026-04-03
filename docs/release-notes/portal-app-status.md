# BaseFaq Portal App Status

Owner: BaseFaq Docs and Release Manager Agent

## Current status

`apps/portal` is currently a specification stub, not a runnable app scaffold.
The repository-facing README defines the intended portal app scope, route ownership, adapter domains, and runtime modes, but the implementation has not been created yet.

## Confirmed scope from the portal brief

- Public marketing and pricing pages
- Mock checkout and provisioning flow
- Tenant workspace entry
- Tenant FAQ management
- Tenant profile screens
- Tenant billing screens
- Public FAQ preview

## Confirmed route ownership

- Public: `/`, `/pricing`, `/plans`, `/checkout`, `/checkout/success`
- Auth and onboarding: `/signin`, `/signup`, `/onboarding/workspace`, `/onboarding/provisioning`
- Tenant workspace: `/t/[tenantSlug]/overview`, `/t/[tenantSlug]/faq`, `/t/[tenantSlug]/faq/new`, `/t/[tenantSlug]/faq/[faqId]`
- Tenant settings: `/t/[tenantSlug]/settings/profile`, `/t/[tenantSlug]/settings/ai`, `/t/[tenantSlug]/settings/public-access`
- Tenant billing: `/t/[tenantSlug]/billing/plan`, `/t/[tenantSlug]/billing/history`, `/t/[tenantSlug]/billing/payment-methods`
- Public preview: `/preview/[tenantSlug]`

## Confirmed adapter domains

- Tenant Portal API
- FAQ Portal API
- FAQ Public API
- Mock Identity API
- Mock Commerce API
- Mock Provisioning API
- Mock Billing API

## Delivery expectation

The next implementation step is the creation of the `apps/portal` Next.js TypeScript workspace using the Demo6 baseline under:

- `apps/demos/metronic-tailwind-react-demos/typescript/nextjs`
- `apps/demos/metronic-tailwind-react-demos/typescript/nextjs/app/components/layouts/demo6`

The portal app should preserve server-side handling for `X-Tenant-Id` and `X-Client-Key`, keep tenant secrets out of client state, and make unsupported live features explicit fallback states.

## Validation notes

- Confirmed the portal README exists and describes the intended app boundary.
- Confirmed the implementation brief exists under `todo/demo6-nextjs-multitenant-microfrontend/`.
- Confirmed there are no implementation files for `apps/portal` in this delivery.

## Blockers

- The portal app scaffold is not yet present, so no runnable frontend can be validated from this state.
- Backend contract verification remains dependent on the future scaffold and the task package details.

## Follow-up items

- Create the `apps/portal` app scaffold.
- Translate the task package into route-level and adapter-level implementation work.
- Add release validation notes once the app can be run locally.
