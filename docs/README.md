# BaseFAQ Documentation

This folder is the canonical knowledge base for the repository. The root `README` explains the essential bootstrap flow; everything deeper belongs here.

## Recommended reading order

1. [`../README.md`](../README.md) for the shortest path to a working local environment.
2. [`devops/local-development.md`](devops/local-development.md) for the full local runtime model.
3. [`tools/migration-tool.md`](tools/migration-tool.md) and [`tools/seed-tool.md`](tools/seed-tool.md) before touching data.
4. [`backend/dotnet-backend-overview.md`](backend/dotnet-backend-overview.md) and [`frontend/portal-app.md`](frontend/portal-app.md) for implementation context.
5. [`architecture/solution-architecture.md`](architecture/solution-architecture.md) for boundaries and patterns.

## Documentation map

### Architecture

- [`architecture/solution-architecture.md`](architecture/solution-architecture.md): overall solution shape, service boundaries, data ownership, and architectural patterns.
- [`architecture/basefaq-ai-generation-matching-architecture.md`](architecture/basefaq-ai-generation-matching-architecture.md): current AI runtime, message flow, and implementation status.
- [`architecture/basefaq-multi-agent-system.md`](architecture/basefaq-multi-agent-system.md): current status and intended role of the `agents/` area, which is documentation-only today.

### Frontend

- [`frontend/portal-app.md`](frontend/portal-app.md): portal application scope, setup, environment variables, API dependencies, and local validation.
- [`frontend/portal-app-ui-prompt-guidance.md`](frontend/portal-app-ui-prompt-guidance.md): shared UI and UX implementation rules for Portal work.

### Backend

- [`backend/dotnet-backend-overview.md`](backend/dotnet-backend-overview.md): API catalog, project layout, multitenancy, persistence, and backend development conventions.

### DevOps

- [`devops/local-development.md`](devops/local-development.md): Docker base services, local host execution, full Docker flow, local subdomains, troubleshooting, and shutdown.
- [`devops/azure-delivery.md`](devops/azure-delivery.md): stage-based Azure provisioning, deployment, and CI/CD integration.

### Tools

- [`tools/migration-tool.md`](tools/migration-tool.md): tenant-aware migration workflow for FAQ databases.
- [`tools/seed-tool.md`](tools/seed-tool.md): essential and sample-data seeding workflow.

### Operations and Quality

- [`operations/secret-manager-key-rotation.md`](operations/secret-manager-key-rotation.md): tenant AI provider credential storage and rotation runbook.
- [`testing/integration-testing-strategy.md`](testing/integration-testing-strategy.md): test strategy, execution tiers, and coverage priorities.
- [`standards/solution-cqrs-write-rules.md`](standards/solution-cqrs-write-rules.md): solution-wide CQRS write-side rules.

### Release

- [`release/README.md`](release/README.md): where to keep release plans, rollout checklists, and versioned release evidence.

### Project-local docs

- [`../PROJECT_RULES.md`](../PROJECT_RULES.md): repository-wide coding and architecture guardrails enforced by the architecture compliance tests.
- [`../agents/README.md`](../agents/README.md): current status of the `agents/` folder and its intended future role.
- [`../apps/portal/README.md`](../apps/portal/README.md): project-local Portal app readme beside the frontend source.
- [`../azure/README.md`](../azure/README.md): script-local Azure deployment reference.
- [`../local/env/simulatedev/README.md`](../local/env/simulatedev/README.md): local subdomain helper readme beside the helper scripts.

## Documentation rules

- Keep `README.md` focused on project presentation and the minimum steps to run the stack.
- Put operational detail, architecture decisions, and deeper workflows in `docs/`.
- Prefer evergreen documentation over status snapshots that age quickly.
- When the code changes, update the most specific document first, then adjust the index if navigation changed.
