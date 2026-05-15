---
name: querify-doc-router
description: "Route Querify tasks to the smallest correct documentation set. Use when starting repo work, choosing backend/frontend/product boundaries, or avoiding unnecessary context loading."
when_to_use: "Use for workstream selection, doc ownership questions, or before large changes that might touch dotnet, apps/portal, docs, devops, AI, MCP, or Creator MVP."
---

# Querify Doc Router

Use this skill to choose documents, not to replace them. Read the smallest document set that owns the task, then inspect the matching implementation.

## First pass

1. Read `README.md` for the current repo shape, service ports, startup flow, and module summary.
2. Read `docs/README.md` for documentation ownership and the canonical doc index.
3. Read `docs/execution-guide.md` to classify the workstream.
4. If the task crosses model, persistence, APIs, seed, tests, Portal, or translations, use `/querify-behavior-change`.

## Workstream routing

- Backend feature or CQRS refactor: `dotnet-backend-overview` -> `solution-cqrs-write-rules` -> `repository-rules` -> `integration-testing-strategy`.
- Product behavior change: `behavior-change-playbook` -> backend rules -> frontend docs if UI changes -> matching tests.
- Tenant-aware public query: `solution-architecture` -> `dotnet-backend-overview` -> `integration-testing-strategy`.
- Control-plane worker or async processing: `querify-tenant-worker` -> `solution-architecture` -> `integration-testing-strategy`.
- Seed, migration, or local stack: `local-development` -> `seed-tool` -> `migration-tool`.
- Portal domain data flow: `portal-app` -> `portal-runtime` -> `validation-guide`.
- Portal page composition or shared UI: `portal-app-ui-prompt-guidance` -> `portal-app` -> `validation-guide`.
- Portal localization: `portal-localization` -> `portal-app` -> `validation-guide`.
- AI, MCP, agents, or Source-to-QnA generation: `ai_product_modules_strategy` -> `mcp.md` -> `mcp-source-to-qna.md` -> backend architecture rules.

## Documentation map

For the complete compact map of all repository markdown files, read [docs-map.md](docs-map.md) only when the exact document owner is unclear.

## Output discipline

- State which owning docs you used when the task depends on architecture or product boundaries.
- Do not paste long documentation into code comments, prompts, or new files.
- Update the most specific owning document when behavior, runtime assumptions, validation, or module ownership changes.
