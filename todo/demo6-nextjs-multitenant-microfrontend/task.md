# Implementation Task For Future Agent

## Mission

Build `frontend/microfrontends/demo6-tenant-commerce/` as a Demo6-based Next.js TypeScript micro-frontend that works in `mock` mode first and can progressively switch to `live` BaseFaq APIs.

## Required Inputs

- `todo/demo6-nextjs-multitenant-microfrontend/spec.md`
- `todo/demo6-nextjs-multitenant-microfrontend/backend-api-inventory.md`
- `todo/demo6-nextjs-multitenant-microfrontend/mock-apis/demo6-multitenant-mock.openapi.yaml`
- `docs/architecture/basefaq-multi-agent-system.md`
- `frontend/README.md`

## Delivery Constraints

- Do not edit the demo reference source under `frontend/demos/`.
- Use the Demo6 layout as the authenticated shell baseline.
- Keep all new frontend work inside the micro-frontend folder unless a shared abstraction is clearly justified.
- Use explicit adapters for each backend service domain.
- Stop for human review if the task requires backend contract changes, multitenant persistence changes, or auth infrastructure changes.

## Phase 1: Scaffold The App

- Create the Next.js TypeScript app structure under `frontend/microfrontends/demo6-tenant-commerce/`.
- Bring over the required Demo6 layout pieces by copying and adapting local components.
- Set up linting, TypeScript, environment handling, and a minimal README.
- Add route groups for public, onboarding, workspace, and preview flows.

### Definition Of Done

- The app starts locally.
- The Demo6 shell renders for workspace routes.
- Public routes render without the workspace shell.

## Phase 2: Build The Integration Foundation

- Implement configuration for `mock` and `live` modes.
- Add route handlers that act as the BFF layer.
- Add adapter modules for:
  - tenant portal
  - FAQ portal
  - FAQ public
  - commerce
  - provisioning
  - identity
- Add Zod schemas for all adapter inputs and outputs.
- Add tenant slug resolution and active tenant state management.

### Definition Of Done

- No page calls upstream services directly from client components.
- FAQ Portal calls can receive `X-Tenant-Id` through the BFF path.
- FAQ Public calls can receive `X-Client-Key` through the BFF path.
- Switching between `mock` and `live` mode is environment-driven.

## Phase 3: Implement Public Acquisition

- Build marketing home, pricing, and plan comparison pages.
- Build checkout initiation and success pages using the mock commerce contract.
- Show plan-to-edition mapping clearly in the UI.
- Add empty, loading, and failure states.

### Definition Of Done

- A user can select a plan and create a mock checkout session.
- The selected plan persists through the purchase flow.
- The UI exposes whether it is running with mock billing.

## Phase 4: Implement Onboarding And Tenant Entry

- Build sign-in and sign-up placeholders or mock session entry.
- Build workspace onboarding and provisioning status pages.
- Create the initial tenant in mock mode and map it to a slug.
- Redirect to `/t/[tenantSlug]/overview` after provisioning succeeds.

### Definition Of Done

- The end-to-end path from plan selection to workspace entry works in mock mode.
- Tenant selection is explicit and route-driven.
- The app supports more than one tenant in the session model.

## Phase 5: Implement Tenant Workspace

- Build overview, FAQ list, FAQ detail, and FAQ item editing screens.
- Build profile settings, AI settings, public access settings, and billing pages.
- Wire the FAQ authoring pages to live-ready adapters.
- Wire public preview to the FAQ Public adapter through the client key.
- Add tenant switcher and top-level navigation using the Demo6 shell.

### Definition Of Done

- A tenant can browse FAQs, edit FAQ metadata, and create or edit FAQ items.
- A tenant can view or rotate the client key from the settings flow.
- A tenant can view AI provider status and submit credentials.
- Public preview works through the preview route and not through hard-coded fixture rendering.

## Phase 6: Quality And Handoff

- Add unit tests for adapters and validation.
- Add integration tests for route handlers.
- Add a smoke E2E flow for pricing to checkout to onboarding to FAQ list.
- Document local run steps, env vars, and mode switching.

### Definition Of Done

- `npm run lint` passes.
- `npm run build` passes.
- The smoke flow passes in mock mode.
- The micro-frontend README explains route ownership, adapters, auth assumptions, and validation steps.

## Suggested Environment Variables

- `NEXT_PUBLIC_BASEFAQ_MODE=mock`
- `BASEFAQ_TENANT_PORTAL_API_URL=http://localhost:5002`
- `BASEFAQ_FAQ_PORTAL_API_URL=http://localhost:5010`
- `BASEFAQ_FAQ_PUBLIC_API_URL=http://localhost:5020`
- `BASEFAQ_AUTH_MODE=mock`

## High-risk Areas

- Token handling
- Tenant id resolution and propagation
- Public client key exposure
- Mixing mock and live data on the same page
- Any attempt to push billing logic into current tenant APIs without a reviewed contract

## Final Acceptance Criteria

- The demo is visually grounded in Demo6.
- The architecture is adapter-driven and multitenant-safe.
- The app can run without unfinished backend services.
- The live-ready seams for Tenant Portal, FAQ Portal, and FAQ Public are already in place.
