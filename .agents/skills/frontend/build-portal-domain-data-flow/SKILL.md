---
name: build-portal-domain-data-flow
description: Add or extend a Portal domain with BaseFAQ-standard API wiring, TanStack Query hooks, and route integration.
type: repository-skill
scope: basefaq-repository
category: frontend
priority: high
triggers:
  - portal page
  - new domain screen
  - add hook
  - wire mutation
owned_paths:
  - apps/portal/src/domains/*
  - apps/portal/src/shared/*
collaborates_with:
  - compose-portal-page-layouts
  - design-confirmed-actions-and-stateful-feedback
  - implement-portal-localization
---

# Build Portal Domain Data Flow

## When to Use

- A prompt asks for a new Portal feature, domain page, list view, detail view, or form.
- An existing domain needs typed queries, mutations, or route wiring.

## Responsibilities

- Keep transport in `api.ts`.
- Keep query and mutation behavior in `hooks.ts`.
- Flow auth and tenant context through every protected request.

## Workflow

1. Define or update domain types and schemas.
2. Implement transport helpers in `api.ts` with `portalRequest(...)`.
3. Add TanStack Query keys, queries, and invalidating mutations in `hooks.ts`.
4. Pass `accessToken` and `tenantId` consistently through protected calls.
5. Build the consuming page and route module inside the domain folder.
6. Reuse shared empty, loading, and error states rather than page-local fallbacks.

## BaseFAQ Domain Alignment

- Portal work targets `BaseFaq.Tenant.Portal.Api` and `BaseFaq.Faq.Portal.Api`.
- Portal APIs usually require `X-Tenant-Id`.
- Backend DTOs are not responsible for frontend translation.

## Collaborates With

- [`compose-portal-page-layouts`](../compose-portal-page-layouts/SKILL.md)
- [`design-confirmed-actions-and-stateful-feedback`](../design-confirmed-actions-and-stateful-feedback/SKILL.md)
- [`implement-portal-localization`](../implement-portal-localization/SKILL.md)

## Done When

- Domain API, hooks, and page modules are coherent.
- Query invalidation refreshes the right read surfaces.
- The feature fits the existing Portal architecture without creating a parallel pattern.
