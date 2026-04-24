# BaseFAQ Execution Guide

## Purpose

This document is the human workflow guide for the repository.

Use it to decide how work should be framed, which repository boundary owns it, and which backend or frontend documents must be used together.

## How to frame work

Good requests and work items usually contain:

- goal
- context
- input
- constraints

Simple template:

```text
I want [goal].
Context: [system, screen, service, domain, or legal/compliance scenario].
Input: [file, diff, route, API, error, or business scenario].
Constraints: [optional restrictions].
```

Examples:

```text
Create a CQRS endpoint to list questions by tenant and add the right integration tests.
```

```text
Add loading, empty, and error states to this Portal page and keep the existing layout pattern.
```

```text
Review this diff and focus on architecture, security, and maintainability risks.
```

```text
An EU user asked to delete personal data. Explain which system boundaries are affected and what needs to change.
```

## Choose the smallest correct workflow

### 1. Backend implementation workflow

Use this workflow when the request changes:

- `.NET` APIs
- handlers, commands, queries, or services
- tenant resolution
- persistence or worker behavior
- migrations, seed data, or control-plane processing

Primary documents:

- [`backend/architecture/solution-architecture.md`](backend/architecture/solution-architecture.md)
- [`backend/architecture/dotnet-backend-overview.md`](backend/architecture/dotnet-backend-overview.md)
- [`backend/architecture/solution-cqrs-write-rules.md`](backend/architecture/solution-cqrs-write-rules.md)
- [`backend/architecture/repository-rules.md`](backend/architecture/repository-rules.md)
- [`backend/testing/integration-testing-strategy.md`](backend/testing/integration-testing-strategy.md)

Use these supporting guides when needed:

- worker behavior: [`backend/architecture/basefaq-tenant-worker.md`](backend/architecture/basefaq-tenant-worker.md)
- local runtime: [`backend/tools/local-development.md`](backend/tools/local-development.md)
- migrations: [`backend/tools/migration-tool.md`](backend/tools/migration-tool.md)
- seed data: [`backend/tools/seed-tool.md`](backend/tools/seed-tool.md)

### 2. Frontend implementation workflow

Use this workflow when the request changes:

- `apps/portal`
- routes, domain hooks, page composition, layout, or shared UI behavior
- user-facing copy, localization, or direction
- Portal runtime configuration

Primary documents:

- [`frontend/architecture/portal-app.md`](frontend/architecture/portal-app.md)
- [`frontend/architecture/portal-app-ui-prompt-guidance.md`](frontend/architecture/portal-app-ui-prompt-guidance.md)
- [`frontend/architecture/portal-localization.md`](frontend/architecture/portal-localization.md)
- [`frontend/testing/validation-guide.md`](frontend/testing/validation-guide.md)

Use these supporting guides when needed:

- runtime and Auth0 setup: [`frontend/tools/portal-runtime.md`](frontend/tools/portal-runtime.md)
- local shared hostnames: [`frontend/tools/local-subdomains.md`](frontend/tools/local-subdomains.md)
- backend dependency alignment: [`backend/tools/local-development.md`](backend/tools/local-development.md)

### 3. Cross-cutting backend plus frontend workflow

Use this workflow when the change crosses the Portal and APIs together, for example:

- a new portal screen that depends on a new API contract
- localization that depends on backend profile fields
- a public or tenant flow that changes both transport and UI behavior

Start in this order:

1. [`backend/architecture/solution-architecture.md`](backend/architecture/solution-architecture.md) to identify ownership boundaries.
2. The owning backend guide in [`backend/architecture`](backend/architecture).
3. The owning frontend guide in [`frontend/architecture`](frontend/architecture).
4. The matching testing guides in [`backend/testing`](backend/testing) and [`frontend/testing`](frontend/testing).
5. [`behavior-change-playbook.md`](behavior-change-playbook.md) when the change also alters model contracts, seed data, tests, or translations.

### 4. Code review workflow

Use this workflow when the task is to review a diff, PR, or implementation risk.

Read the owning implementation docs first:

- backend changes: use the backend architecture and testing guides
- frontend changes: use the frontend architecture and validation guides
- mixed changes: use both

Review focus should prioritize:

- architecture boundaries
- security and tenancy risks
- behavioral regressions
- missing validation coverage

Primary references:

- [`backend/architecture/repository-rules.md`](backend/architecture/repository-rules.md)
- [`backend/architecture/solution-cqrs-write-rules.md`](backend/architecture/solution-cqrs-write-rules.md)
- [`backend/testing/integration-testing-strategy.md`](backend/testing/integration-testing-strategy.md)
- [`frontend/architecture/portal-app.md`](frontend/architecture/portal-app.md)
- [`frontend/architecture/portal-app-ui-prompt-guidance.md`](frontend/architecture/portal-app-ui-prompt-guidance.md)
- [`frontend/testing/validation-guide.md`](frontend/testing/validation-guide.md)

### 5. Security review workflow

Use this workflow when the task is about:

- injection risks
- XSS
- unsafe deserialization
- hardcoded secrets
- unsafe public ingress
- cross-tenant data exposure

Start with the owning surface:

- API, worker, persistence, or public ingress: backend docs
- Portal rendering, browser behavior, forms, or localization: frontend docs

Primary references:

- [`backend/architecture/solution-architecture.md`](backend/architecture/solution-architecture.md)
- [`backend/architecture/dotnet-backend-overview.md`](backend/architecture/dotnet-backend-overview.md)
- [`backend/architecture/repository-rules.md`](backend/architecture/repository-rules.md)
- [`frontend/architecture/portal-app.md`](frontend/architecture/portal-app.md)
- [`frontend/testing/validation-guide.md`](frontend/testing/validation-guide.md)

### 6. Privacy and compliance workflow

Use this workflow when the request is about:

- GDPR, LGPD, CCPA, CPRA, or PIPL applicability
- DSAR intake and fulfillment
- consent capture or withdrawal
- audit evidence
- personal-data classification

There is no separate privacy documentation area in the current structure.
Use the product and runtime docs to identify the real ownership boundary first, then update the most specific backend or frontend document touched by the change.

Start with:

- [`backend/architecture/solution-architecture.md`](backend/architecture/solution-architecture.md) for system and data ownership
- [`backend/architecture/dotnet-backend-overview.md`](backend/architecture/dotnet-backend-overview.md) for concrete API and persistence surfaces
- [`frontend/architecture/portal-app.md`](frontend/architecture/portal-app.md) when the Portal UI, profile settings, or consent-facing flows are affected
- [`backend/tools/release-artifacts.md`](backend/tools/release-artifacts.md) when rollout evidence, decision notes, or audit support material must be recorded

## Standard workstreams

### Backend feature or CQRS refactor

Follow:

1. [`backend/architecture/dotnet-backend-overview.md`](backend/architecture/dotnet-backend-overview.md)
2. [`backend/architecture/solution-cqrs-write-rules.md`](backend/architecture/solution-cqrs-write-rules.md)
3. [`backend/architecture/repository-rules.md`](backend/architecture/repository-rules.md)
4. [`backend/testing/integration-testing-strategy.md`](backend/testing/integration-testing-strategy.md)

Use this for new commands, queries, controllers, services, feature decomposition, and QnA project ownership changes.

### Product behavior change

Follow:

1. [`behavior-change-playbook.md`](behavior-change-playbook.md)
2. [`backend/architecture/repository-rules.md`](backend/architecture/repository-rules.md)
3. [`backend/architecture/solution-cqrs-write-rules.md`](backend/architecture/solution-cqrs-write-rules.md)
4. [`backend/testing/integration-testing-strategy.md`](backend/testing/integration-testing-strategy.md)
5. [`frontend/architecture/portal-app.md`](frontend/architecture/portal-app.md), when the Portal is affected
6. [`frontend/architecture/portal-localization.md`](frontend/architecture/portal-localization.md), when user-facing copy changes

Use this for behavior that starts in entities or enums and must be applied through persistence, DTO contracts, CQRS flows, API surfaces, seed examples, tests, Portal screens, and locale catalogs. The first decision is whether an existing concept already represents the behavior; duplicated properties and enums should be consolidated before implementation spreads.

### Tenant-aware public query

Follow:

1. [`backend/architecture/solution-architecture.md`](backend/architecture/solution-architecture.md)
2. [`backend/architecture/dotnet-backend-overview.md`](backend/architecture/dotnet-backend-overview.md)
3. [`backend/testing/integration-testing-strategy.md`](backend/testing/integration-testing-strategy.md)

Use this for `X-Client-Key` flows, public reads, and tenant resolution on public APIs.

### Control-plane worker or async processing

Follow:

1. [`backend/architecture/basefaq-tenant-worker.md`](backend/architecture/basefaq-tenant-worker.md)
2. [`backend/architecture/solution-architecture.md`](backend/architecture/solution-architecture.md)
3. [`backend/testing/integration-testing-strategy.md`](backend/testing/integration-testing-strategy.md)

Use this for billing inboxes, email outboxes, retries, leases, and async command processing.

### Seed, migration, and local stack changes

Follow:

1. [`backend/tools/local-development.md`](backend/tools/local-development.md)
2. [`backend/tools/seed-tool.md`](backend/tools/seed-tool.md)
3. [`backend/tools/migration-tool.md`](backend/tools/migration-tool.md)

Use this for environment bootstrap, schema updates, tenant/QnA database setup, and local operational workflows.

### Portal domain data flow

Follow:

1. [`frontend/architecture/portal-app.md`](frontend/architecture/portal-app.md)
2. [`frontend/tools/portal-runtime.md`](frontend/tools/portal-runtime.md)
3. [`frontend/testing/validation-guide.md`](frontend/testing/validation-guide.md)

Use this for domain hooks, API clients, route wiring, page ownership, and backend contract alignment in `apps/portal`.

### Portal page composition and stateful feedback

Follow:

1. [`frontend/architecture/portal-app-ui-prompt-guidance.md`](frontend/architecture/portal-app-ui-prompt-guidance.md)
2. [`frontend/architecture/portal-app.md`](frontend/architecture/portal-app.md)
3. [`frontend/testing/validation-guide.md`](frontend/testing/validation-guide.md)

Use this for layouts, loading states, empty states, error handling, confirmations, and detail/list/settings consistency.

### Portal localization

Follow:

1. [`frontend/architecture/portal-localization.md`](frontend/architecture/portal-localization.md)
2. [`frontend/architecture/portal-app.md`](frontend/architecture/portal-app.md)
3. [`frontend/testing/validation-guide.md`](frontend/testing/validation-guide.md)

Use this for locale catalogs, language precedence, `lang` and `dir`, and profile-language integration.

### Specialized domain, AI, or distribution design

When the request concerns deeper product modeling, async AI flows, provenance, trust, or integration rollout, there is no standalone documentation area yet.

Start with:

1. [`backend/architecture/solution-architecture.md`](backend/architecture/solution-architecture.md)
2. [`backend/architecture/dotnet-backend-overview.md`](backend/architecture/dotnet-backend-overview.md)
3. [`backend/architecture/repository-rules.md`](backend/architecture/repository-rules.md)
4. [`backend/testing/integration-testing-strategy.md`](backend/testing/integration-testing-strategy.md)

If the work becomes a stable repository pattern, add or extend the owning backend or frontend guide instead of creating an isolated taxonomy document.

## Documentation update rules

- Use the smallest correct owning document first.
- If the change is backend-only, update backend docs only.
- If the change is frontend-only, update frontend docs only.
- If the change crosses both, update both sides and keep links between them explicit.
- If the change introduces a new stable cross-cutting workflow, extend this guide and the docs index instead of restoring a separate agent taxonomy.

## Done when

The documentation work is complete when:

- the correct backend or frontend boundary was identified
- the most specific owning document was updated
- related runtime or testing guidance was updated when needed
- cross-boundary changes are linked from both sides
- this guide still routes readers to the right practical document set
