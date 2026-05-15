# Querify Documentation Map

This map is a token-light index for choosing what to read. It summarizes ownership; it does not replace the docs.

## Root

- `README.md`: repository snapshot, product summary, service ports, local startup, stack, endpoints.
- `docs/README.md`: canonical docs index, recommended reading order, documentation ownership rules.
- `docs/execution-guide.md`: workstream routing, request framing, doc ownership, done criteria.
- `docs/behavior-change-playbook.md`: staged cross-layer behavior-change process.

## Backend architecture

- `docs/backend/architecture/solution-architecture.md`: runtime surfaces, module boundaries, CQRS, persistence, multitenancy, workers, shared infrastructure.
- `docs/backend/architecture/dotnet-backend-overview.md`: backend project inventory, service catalog, standard request flow, persistence model, development conventions.
- `docs/backend/architecture/repository-rules.md`: non-negotiable architecture rules, folder ownership, command/query slicing, API error conventions, review checklist.
- `docs/backend/architecture/solution-cqrs-write-rules.md`: command return rules, no read-after-write, read query performance, HTTP write mapping.
- `docs/backend/architecture/qna-domain-boundary.md`: QnA domain entity and business-rule ownership.
- `docs/backend/architecture/querify-tenant-worker.md`: Tenant Worker responsibilities, processing, telemetry, billing/email outbox model, QnA worker boundary.

## Backend tools and testing

- `docs/backend/tools/local-development.md`: local backend runbook, Docker base services, endpoints, Auth0 notes, shutdown.
- `docs/backend/tools/seed-tool.md`: seed menu, essential data, sample data, safety behavior, order of operations.
- `docs/backend/tools/migration-tool.md`: tenant-aware module migration runner for QnA.
- `docs/backend/tools/hangfire-qna-db.md`: QnA Hangfire storage boundary and direct EF commands.
- `docs/backend/tools/release-artifacts.md`: release plan and evidence package location.
- `docs/backend/testing/integration-testing-strategy.md`: integration test philosophy, risk areas, tiers, commands, practical test rules.

## Frontend

- `docs/frontend/architecture/portal-app.md`: Portal scope, tech stack, structure, backend integration, shell, SignalR, Fast Refresh, localization, time rendering.
- `docs/frontend/architecture/portal-app-ui-prompt-guidance.md`: shared UI primitives, layouts, forms, relationships, actions, visual hierarchy, state handling.
- `docs/frontend/architecture/portal-getting-started-guidance.md`: setup progress, next-action ranking, guidance surfaces, page hint inventory.
- `docs/frontend/architecture/portal-localization.md`: language/timezone source of truth, RTL/LTR, locale ownership, API error localization.
- `docs/frontend/tools/portal-runtime.md`: Portal env vars, Auth0 config, dev server, build/lint, SignalR runtime.
- `docs/frontend/tools/local-subdomains.md`: local subdomain proxy, hosts, nginx mapping, setup/teardown.
- `docs/frontend/testing/validation-guide.md`: lint/build gates and manual regression matrix.

## Integrations and AI

- `docs/integrations/mcp-server.md`: current TypeScript MCP proxy, available tools, Claude Code/Desktop config, production path.
- `docs/future/integrations/mcp.md`: target native .NET MCP server, tool groups, agent prompts, gaps, roadmap.
- `docs/future/integrations/mcp-source-to-qna.md`: Source-to-QnA AI generation pipeline, tools, command design, gaps.
- `docs/business/value_proposition/ai_product_modules_strategy.md`: AI as cross-cutting capability, not a product module; retrieval, guardrails, evals, roadmap.

## Product and Creator MVP

- `docs/business/value_proposition/value_proposition.md`: pt-BR module ownership, handoffs, business flows, value proposition.
- `docs/business/value_proposition/pricing_strategy.md`: pt-BR pricing model and packaging.
- `docs/business/mvp/creator_mvp_plan.md`: pt-BR Creator MVP package, roadmap, positioning, metrics, risks.
- `docs/business/mvp/creator_mvp_technical_architecture.md`: backend implementation plan for Creator MVP, module ownership, entities, APIs, stages.

## Future backend

- `docs/future/README.md`: how future docs differ from operational docs.
- `docs/future/backend/source-upload.md`: source upload design and staged prompts.
- `docs/future/backend/source-upload-implementation-handoff.md`: current source-upload staged implementation state.
- `docs/future/backend/source-upload-event-driven-signalr-prompt.md`: event-driven source upload verification and Portal SignalR prompt.
