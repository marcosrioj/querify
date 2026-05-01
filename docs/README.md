# BaseFAQ Documentation

This folder is the canonical knowledge base for the repository. The root [`../README.md`](../README.md) is the short bootstrap summary. Every other repository-owned `.md` file belongs under `docs/`.

---

## Cross-cutting guides

These guides apply across the entire repository, regardless of whether the work is backend, frontend, or both.

- [`execution-guide.md`](execution-guide.md) — work-routing guide: which workflow to follow, which documents own each boundary, and how to frame requests correctly.
- [`behavior-change-playbook.md`](behavior-change-playbook.md) — 12-step workflow for changes that cross model, persistence, APIs, seed, tests, Portal UI, and translations.

---

## Business

Documents for understanding what the product does, why each module exists, and where behavioral ownership lives. Read this before making decisions about which module should own a new behavior.

- [`business/value_proposition.md`](business/value_proposition.md) — module split (Tenant, QnA, Direct, Broadcast, Trust), value proposition, ownership rules, cross-module handoff model, and 20 business flow scenarios.

> This document is written in Portuguese (pt-BR). It describes the product domain, not the technical implementation.

---

## Backend

### Architecture

Technical reference for how the .NET backend is organized, which patterns are mandatory, and what each module boundary owns.

- [`backend/architecture/solution-architecture.md`](backend/architecture/solution-architecture.md) — runtime surfaces, service boundaries, persistence ownership, request model, and architectural patterns.
- [`backend/architecture/dotnet-backend-overview.md`](backend/architecture/dotnet-backend-overview.md) — active project inventory, API catalog, module taxonomy, persistence model, multitenancy, and local backend workflow.
- [`backend/architecture/repository-rules.md`](backend/architecture/repository-rules.md) — non-negotiable architecture guardrails and review checklist enforced by the architecture compliance suite.
- [`backend/architecture/solution-cqrs-write-rules.md`](backend/architecture/solution-cqrs-write-rules.md) — mandatory CQRS write-side rules and module physical-boundary constraints. See `repository-rules.md` for the extended companion.
- [`backend/architecture/qna-domain-boundary.md`](backend/architecture/qna-domain-boundary.md) — QnA domain project ownership for entities, business rules, and the migration process.
- [`backend/architecture/basefaq-tenant-worker.md`](backend/architecture/basefaq-tenant-worker.md) — worker host responsibilities, billing/email processing model, sample data, and control-plane constraints.

### Tools

Operational guides for running, seeding, migrating, and releasing the backend.

- [`backend/tools/local-development.md`](backend/tools/local-development.md) — local runtime model, Docker infrastructure, base/backend/frontend container flows, and troubleshooting.
- [`backend/tools/seed-tool.md`](backend/tools/seed-tool.md) — tenant and module seed workflow, menu options, and sample-data behavior.
- [`backend/tools/migration-tool.md`](backend/tools/migration-tool.md) — tenant-aware module migration workflow for supported module databases.
- [`backend/tools/release-artifacts.md`](backend/tools/release-artifacts.md) — where to keep release plans, rollout checklists, and evidence packages.

### Testing

- [`backend/testing/integration-testing-strategy.md`](backend/testing/integration-testing-strategy.md) — integration-test philosophy, execution tiers, current coverage, and high-risk areas.

---

## Frontend

### Architecture

Technical reference for how `apps/portal` is organized, what it consumes from the backend, and what patterns keep the UI coherent.

- [`frontend/architecture/portal-app.md`](frontend/architecture/portal-app.md) — Portal scope, tech stack, backend contracts, shell architecture, and app-level conventions.
- [`frontend/architecture/portal-app-ui-prompt-guidance.md`](frontend/architecture/portal-app-ui-prompt-guidance.md) — shared UI patterns and composition rules: layouts, forms, relationships, actions, state handling, visual hierarchy, and responsive behavior.
- [`frontend/architecture/portal-localization.md`](frontend/architecture/portal-localization.md) — language resolution order, RTL/LTR behavior, locale ownership, translation implementation, and API error localization.

### Tools

- [`frontend/tools/portal-runtime.md`](frontend/tools/portal-runtime.md) — Portal-specific runtime setup, Auth0 configuration, environment variables, and validation commands.
- [`frontend/tools/local-subdomains.md`](frontend/tools/local-subdomains.md) — local subdomain helper, proxy topology, hostnames, and setup/teardown commands.

### Testing

- [`frontend/testing/validation-guide.md`](frontend/testing/validation-guide.md) — current frontend verification workflow, lint/build checks, and required manual regression matrix.

---

## Recommended reading order

1. [`../README.md`](../README.md) — shortest path to a working local environment.
2. [`execution-guide.md`](execution-guide.md) — choose the correct workstream and owning document boundary.
3. [`business/value_proposition.md`](business/value_proposition.md) — when the work depends on module ownership or cross-module flows.
4. [`behavior-change-playbook.md`](behavior-change-playbook.md) — when the change crosses more than one layer.
5. [`backend/tools/local-development.md`](backend/tools/local-development.md) — to bring up the local stack.
6. The owning architecture document for the boundary you are changing.
7. The matching testing guide before you merge.

---

## Documentation rules

- Only the root repository `README.md` may live outside `docs/`.
- Update the most specific owning document when behavior changes; do not create new files for behavior that belongs in an existing document.
- Use `backend/` and `frontend/` as the primary top-level categories. Root-level docs should exist only for cross-cutting repository guides.
- Keep `business/` documents in pt-BR; they describe domain intent for the business team, not technical implementation.

### Content ownership — do not duplicate

Each type of content has a single owner. Reference that owner; do not copy content into other documents.

| Content type | Single owner | Do not add to |
|---|---|---|
| Portal UI rules: layouts, forms, relationships, actions, state handling, visual hierarchy | `frontend/architecture/portal-app-ui-prompt-guidance.md` | `portal-app.md` or any other doc |
| Backend review checklist | `backend/architecture/repository-rules.md` | `solution-cqrs-write-rules.md` or architecture docs |
| Standard workstream reading order | `execution-guide.md` → Standard workstreams | workflow or architecture docs |
| Document authority / reading order for a cross-layer workflow | `execution-guide.md` | individual playbooks or step docs (point here instead) |
| Local service endpoints (full infra) | `backend/tools/local-development.md` | architecture docs |
| Locale file list | `frontend/architecture/portal-localization.md` | playbooks or tool docs |
| External market research links and reference URLs | nowhere — not operational | any doc |
