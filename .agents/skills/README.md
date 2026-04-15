# BaseFAQ Skills

All BaseFAQ skill selection starts here.

Each skill is stored as `skills/<category>/<skill-name>/SKILL.md` and follows one normalized shape:

- metadata front matter
- `When to Use`
- `Responsibilities`
- `Workflow`
- `BaseFAQ Domain Alignment`
- `Collaborates With`
- `Done When`

## Categories

### Backend

- `build-cqrs-feature-module`
  - Add or refactor a backend feature into the repository-standard command/query shape.
- `implement-tenant-aware-public-query`
  - Build public read paths that resolve tenant context from `X-Client-Key`.

### Frontend

- `build-portal-domain-data-flow`
  - Wire Portal domain APIs, hooks, queries, mutations, and route modules.
- `compose-portal-page-layouts`
  - Structure Portal pages with shared layout primitives and consistent information hierarchy.
- `design-confirmed-actions-and-stateful-feedback`
  - Add confirmations, pending states, empty states, and error states.
- `implement-portal-localization`
  - Keep copy, language resolution, and RTL/LTR behavior aligned.

### Data

- `apply-seed-and-migrations-safely`
  - Use BaseFAQ tools to seed and migrate tenant and FAQ databases safely.
- `bootstrap-local-platform-stack`
  - Bring up the local runtime and observability stack.
- `process-control-plane-work-items`
  - Implement safe lease-based worker processors in `TenantDbContext`.
- `write-real-database-integration-tests`
  - Validate behavior against real PostgreSQL-backed integration flows.
- `enforce-cqrs-architecture-rules`
  - Protect repository-wide command/query rules with automated checks.

### Domain

- `model-question-thread-domain`
  - Define the future Q&A thread model.
- `design-provenance-and-trust`
  - Model reusable evidence, citations, and trust semantics.
- `plan-faq-to-qna-upgrade`
  - Sequence additive product and engineering changes from FAQ to Q&A.

### AI

- `publish-asynchronous-ai-request`
  - Start AI generation or matching flows through events and correlation ids.

### Distribution

- `prioritize-integration-rollout`
  - Plan BaseFAQ integrations, embeds, SDKs, and platform delivery layers.

## Fast Routing Hints

- Prompt contains "API", "command", "query", "controller", "MediatR"
  - Start in `backend/`
- Prompt contains "Portal", "page", "hook", "toast", "localization"
  - Start in `frontend/`
- Prompt contains "migration", "seed", "worker", "PostgreSQL", "integration test"
  - Start in `data/`
- Prompt contains "question thread", "answer", "knowledge", "provenance"
  - Start in `domain/`
- Prompt contains "generation", "matching", "provider", "RabbitMQ"
  - Start in `ai/`
- Prompt contains "integrations", "embed", "SDK", "WordPress", "distribution"
  - Start in `distribution/`
