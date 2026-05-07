# Querify Execution Guide

## Purpose

This is the work-routing guide for the repository. Use it to decide how work should be framed, which boundary owns it, and which documents to read together.

**Business context** lives in [`business/value_proposition.md`](business/value_proposition.md) (pt-BR). Read it first when the work touches module ownership, cross-module flows, or the product value proposition.

**Technical context** lives in the `backend/` and `frontend/` document trees. This guide routes you to the right one.

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

- [`business/value_proposition.md`](business/value_proposition.md) when Querify module ownership or cross-module flow is part of the change
- [`backend/architecture/solution-architecture.md`](backend/architecture/solution-architecture.md)
- [`backend/architecture/dotnet-backend-overview.md`](backend/architecture/dotnet-backend-overview.md)
- [`backend/architecture/solution-cqrs-write-rules.md`](backend/architecture/solution-cqrs-write-rules.md)
- [`backend/architecture/repository-rules.md`](backend/architecture/repository-rules.md)
- [`backend/testing/integration-testing-strategy.md`](backend/testing/integration-testing-strategy.md)

Use these supporting guides when needed:

- worker behavior: [`backend/architecture/querify-tenant-worker.md`](backend/architecture/querify-tenant-worker.md)
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
- [`frontend/architecture/portal-getting-started-guidance.md`](frontend/architecture/portal-getting-started-guidance.md)
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

1. [`business/value_proposition.md`](business/value_proposition.md) when the change depends on the Querify module split across Tenant, QnA, Direct, Broadcast, or Trust.
2. [`backend/architecture/solution-architecture.md`](backend/architecture/solution-architecture.md) to identify ownership boundaries.
3. The owning backend guide in [`backend/architecture`](backend/architecture).
4. The owning frontend guide in [`frontend/architecture`](frontend/architecture).
5. The matching testing guides in [`backend/testing`](backend/testing) and [`frontend/testing`](frontend/testing).
6. [`behavior-change-playbook.md`](behavior-change-playbook.md) when the change also alters model contracts, seed data, tests, or translations.

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

For cross-tenant data exposure, inspect the owning module `DbContext/TenantIntegrity` rules as part of the backend review. Tenant-owned relationships should be guarded in the `DbContext`, not only by request handlers.

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
Use the module and runtime docs to identify the real ownership boundary first, then update the most specific backend or frontend document touched by the change.

Start with:

- [`business/value_proposition.md`](business/value_proposition.md) for Querify module boundaries and cross-module flow ownership
- [`backend/architecture/solution-architecture.md`](backend/architecture/solution-architecture.md) for system and data ownership
- [`backend/architecture/dotnet-backend-overview.md`](backend/architecture/dotnet-backend-overview.md) for concrete API and persistence surfaces
- [`frontend/architecture/portal-app.md`](frontend/architecture/portal-app.md) when the Portal UI, profile settings, or consent-facing flows are affected
- [`backend/tools/release-artifacts.md`](backend/tools/release-artifacts.md) when rollout evidence, decision notes, or audit support material must be recorded

## Standard workstreams

Quick-reference reading order per task type. These refine the workflow choices above for the most common sub-patterns.

| Workstream | Reading order |
|---|---|
| Backend feature / CQRS refactor | `dotnet-backend-overview` → `solution-cqrs-write-rules` → `repository-rules` → `integration-testing-strategy` |
| Product behavior change (cross-layer) | `behavior-change-playbook` → `repository-rules` → `solution-cqrs-write-rules` → `integration-testing-strategy` → `portal-app` (if UI) → `portal-localization` (if copy changes) |
| Tenant-aware public query (`X-Client-Key`) | `solution-architecture` → `dotnet-backend-overview` → `integration-testing-strategy` |
| Control-plane worker / async processing | `querify-tenant-worker` → `solution-architecture` → `integration-testing-strategy` |
| Seed, migration, or local stack | `local-development` → `seed-tool` → `migration-tool` |
| Portal domain data flow | `portal-app` → `portal-runtime` → `validation-guide` |
| Portal page composition / UI | `portal-app-ui-prompt-guidance` → `portal-app` → `validation-guide` |
| Portal Getting Started / next action guidance | `portal-getting-started-guidance` → `portal-app-ui-prompt-guidance` → `portal-localization` → `validation-guide` |
| Portal localization | `portal-localization` → `portal-app` → `validation-guide` |
| Specialized domain, AI, or distribution | `solution-architecture` → `dotnet-backend-overview` → `repository-rules` → `integration-testing-strategy`. If the work becomes a stable pattern, extend the owning doc instead of creating a new taxonomy file. |

## Documentation update rules

- Use the smallest correct owning document first.
- If the change is backend-only, update backend docs only.
- If the change is frontend-only, update frontend docs only.
- If the change crosses both, update both sides and keep links between them explicit.
- If the change introduces a new stable cross-cutting workflow, extend this guide and the docs index instead of restoring a separate agent taxonomy.
- Do not copy content from another document into the one you are editing. Reference the owning document instead. The content ownership table in [`docs/README.md`](README.md) defines where each content type lives.

## Done when

The documentation work is complete when:

- the correct backend or frontend boundary was identified
- the most specific owning document was updated
- related runtime or testing guidance was updated when needed
- cross-boundary changes are linked from both sides
- this guide still routes readers to the right practical document set
