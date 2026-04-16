# BaseFAQ Skills

All BaseFAQ skill selection starts here.

The skill catalog is also self-maintained:

- if a future task creates a new recurring capability, add or update the corresponding skill
- if a skill boundary, trigger, ownership path, or collaboration rule changes, update the skill in the same task
- do not finish reusable work while leaving the skill catalog stale

Shared generic skills can also live in `.agents/shared/*.skill.md` when they are:

- cross-domain
- non-business
- reusable by multiple agents or specialists

Privacy operational skills may live in `.agents/privacy/*.skill.md` when they are:

- reusable across privacy orchestrators and privacy specialists
- stateless or close to stateless
- focused on one operational action such as consent, DSAR handling, audit, or classification

Repository-owned skills are stored as `skills/<category>/<skill-name>/SKILL.md`. Flat operational skills may live in `.agents/shared/*.skill.md` or `.agents/privacy/*.skill.md`.

Repository-owned skills follow this normalized shape:

- metadata front matter
- `When to Use`
- `Responsibilities`
- `Workflow`
- `BaseFAQ Domain Alignment`
- `Collaborates With`
- `Done When`

Flat operational skills under `.agents/shared/` and `.agents/privacy/` follow this normalized shape:

- metadata front matter
- `Purpose`
- `Inputs`
- `Outputs`
- `Behavior`
- `Example Usage`

When a skill is created or changed, also review:

- [`../AGENTS.md`](../AGENTS.md)
- [`../patterns/intent-routing.md`](../patterns/intent-routing.md)
- [`../patterns/orchestration-playbooks.md`](../patterns/orchestration-playbooks.md)

## Shared Generic Skills

- `code-parser.skill.md`
  - Classify input as code, config, or text and normalize language-aware evidence.
- `code-diff-parser.skill.md`
  - Extract changed code blocks from diffs and normalize review scope.
- `complexity-analyzer.skill.md`
  - Detect long methods, deep nesting, and high cognitive complexity.
- `pattern-matcher.skill.md`
  - Match conservative vulnerability patterns and return evidence-backed candidates.

## Privacy Flat Skills

- `privacy/dsar.skill.md`
  - Execute data subject or consumer rights workflows after a law-specific specialist has framed the request.
- `privacy/consent.skill.md`
  - Manage consent capture, refresh, withdrawal, and proof.
- `privacy/audit.skill.md`
  - Record traceability and decision evidence for privacy workflows.
- `privacy/data-classification.skill.md`
  - Classify personal data, sensitivity, and downstream control needs.

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
- `migrate-faqdb-to-qnadb`
  - Drive FAQ-to-QnA parity work across models, APIs, UI, runtime, and tests.

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
- Prompt contains "FaqDb", "QnADb", "FAQ parity", "port FAQ to QnA"
  - Start in `domain/` with `migrate-faqdb-to-qnadb`
- Prompt contains "generation", "matching", "provider", "RabbitMQ"
  - Start in `ai/`
- Prompt contains "integrations", "embed", "SDK", "WordPress", "distribution"
  - Start in `distribution/`
