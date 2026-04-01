# Demo6 Next.js Multitenant Micro-frontend Task Package

This folder contains the handoff package for a future agent to build a Demo6-based Next.js TypeScript micro-frontend for BaseFaq.

## Purpose

The target deliverable is a runnable frontend demo that:

- works as a multitenant SaaS entry point
- lets a user choose and buy a plan
- provisions a tenant workspace
- lets the tenant manage FAQ content and preview the public FAQ experience
- can run in `mock` mode before the missing backend services exist

## Inputs Used

- `docs/architecture/basefaq-multi-agent-system.md`
- `frontend/README.md`
- `README.md`
- current controller and DTO surface in `dotnet/`

## Files

- `spec.md`: high-level frontend specification and architecture
- `task.md`: concrete implementation backlog for the future agent
- `backend-api-inventory.md`: verified inventory of current backend APIs relevant to the demo
- `mock-apis/demo6-multitenant-mock.openapi.yaml`: mock contract set for demo mode and missing services

## Execution Rules

- Keep all new docs in English.
- Preserve the demo source as a reference baseline, not as the implementation target.
- Keep backend integration contracts explicit through adapters and route handlers.
- Treat breaking API changes, tenant persistence changes, and security-sensitive changes as human-review gates.
