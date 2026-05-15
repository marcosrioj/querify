# Querify Claude Code Context

Use the project skills in `.claude/skills/` and project agents in `.claude/agents/` before doing broad repository work. Keep context small: load only the owning docs for the requested boundary instead of reading the whole docs tree into the main conversation.

Start with:

- `/querify-doc-router` when deciding which documents own a task.
- `/querify-backend` for `.NET`, CQRS, persistence, worker, tenancy, API, and backend test work.
- `/querify-portal-frontend` for `apps/portal` UI, runtime, localization, and validation work.
- `/querify-behavior-change` for cross-layer product behavior changes.
- `/querify-product-ai` for module ownership, Creator MVP, and AI product architecture.
- `/querify-mcp-ai` for MCP server, AI tools, agents, and Source-to-QnA generation work.
- `/querify-local-ops` for local stack, seed, migration, and validation commands.

Core rules:

- Follow the existing architecture. Do not introduce parallel module, CQRS, DTO, routing, or UI patterns.
- Treat Tenant, QnA, Direct, Broadcast, and Trust as separate product ownership boundaries.
- Keep repository artifacts in English, including code, tests, docs, comments, and `en-US` copy keys.
- Do not run or generate EF migrations unless the user explicitly asks for migration work.
- Do not weaken production contracts to satisfy tests.
- Prefer targeted validation first, then broaden based on risk and touched surfaces.
