# BaseFAQ Documentation

This folder is the canonical knowledge base for the repository.

The root [`../README.md`](../README.md) remains the short bootstrap summary. Every other repository-owned `.md` file belongs under `docs/`.

## Structure

### Cross-cutting

- [`execution-guide.md`](execution-guide.md): repository work-routing guide for cross-cutting work types and the owning backend and frontend documents.

### Backend

#### Architecture

- [`backend/architecture/solution-architecture.md`](backend/architecture/solution-architecture.md): repository and runtime shape, service boundaries, persistence ownership, and request model.
- [`backend/architecture/dotnet-backend-overview.md`](backend/architecture/dotnet-backend-overview.md): active backend project inventory, API catalog, module taxonomy, and local backend workflow.
- [`backend/architecture/solution-cqrs-write-rules.md`](backend/architecture/solution-cqrs-write-rules.md): mandatory CQRS write-side rules and QnA physical-boundary constraints.
- [`backend/architecture/repository-rules.md`](backend/architecture/repository-rules.md): repository-wide architecture guardrails and review checklist enforced by the architecture compliance suite.
- [`backend/architecture/basefaq-tenant-worker.md`](backend/architecture/basefaq-tenant-worker.md): worker host responsibilities, billing/email processing model, and control-plane constraints.

#### Tools

- [`backend/tools/local-development.md`](backend/tools/local-development.md): local runtime model, Docker infrastructure, host-based APIs, full-container mode, and troubleshooting.
- [`backend/tools/migration-tool.md`](backend/tools/migration-tool.md): tenant-aware QnA migration workflow.
- [`backend/tools/seed-tool.md`](backend/tools/seed-tool.md): tenant/QnA seed workflow and sample-data behavior.
- [`backend/tools/release-artifacts.md`](backend/tools/release-artifacts.md): where to keep release plans, rollout checklists, and evidence packages.

#### Testing

- [`backend/testing/integration-testing-strategy.md`](backend/testing/integration-testing-strategy.md): backend integration-test philosophy, execution tiers, and coverage priorities.

### Frontend

#### Architecture

- [`frontend/architecture/portal-app.md`](frontend/architecture/portal-app.md): Portal scope, backend contracts, app structure, and frontend conventions.
- [`frontend/architecture/portal-localization.md`](frontend/architecture/portal-localization.md): language resolution, RTL/LTR behavior, locale ownership, and translation implementation.
- [`frontend/architecture/portal-app-ui-prompt-guidance.md`](frontend/architecture/portal-app-ui-prompt-guidance.md): shared Portal UI patterns and composition rules.

#### Tools

- [`frontend/tools/portal-runtime.md`](frontend/tools/portal-runtime.md): Portal-specific runtime setup, Auth0 configuration, environment variables, and validation commands.
- [`frontend/tools/local-subdomains.md`](frontend/tools/local-subdomains.md): local subdomain helper, proxy topology, hostnames, and setup/teardown commands.

#### Testing

- [`frontend/testing/validation-guide.md`](frontend/testing/validation-guide.md): current frontend verification workflow, lint/build checks, and required manual regression passes.

## Recommended reading order

1. [`../README.md`](../README.md) for the shortest path to a working local environment.
2. [`execution-guide.md`](execution-guide.md) to choose the correct workstream and owning documentation boundary.
3. [`backend/tools/local-development.md`](backend/tools/local-development.md) to bring up the local stack.
4. The owning architecture document for the boundary you are changing.
5. The matching testing guide before you merge.

## Documentation rules

- Only the root repository `README.md` may live outside `docs/`.
- Keep topic boundaries recognizable so documentation stays connected to the codebase and project structure.
- Update the most specific owning document when behavior changes.
- Use `backend/` and `frontend/` as the primary top-level categories. Root-level docs should exist only for cross-cutting repository guides such as this execution guide.
