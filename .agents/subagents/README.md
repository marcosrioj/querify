# BaseFAQ Subagents

Subagents under this tree are lower priority than skills. This tree now hosts both BaseFAQ execution workers and generic reusable markdown specialists in one consolidated location.

The subagent catalog is also self-maintained:

- if a future task reveals a new recurring execution boundary, add or update the corresponding worker
- if a future task reveals a new reusable generic specialist, add or update the corresponding markdown subagent
- if a worker or specialist becomes obsolete or its ownership changes, update this catalog in the same task
- subagent changes should also trigger review of routing and orchestration files through [`../patterns/agent-system-maintenance.md`](../patterns/agent-system-maintenance.md)

Path layout:

- `*.toml` at the root of this folder are BaseFAQ execution workers
- `code-review/*.subagent.md` and `security/*.subagent.md` are reusable generic markdown specialists

Reasoning-depth rule:

- when a worker definition supports `model_reasoning_effort`, use `xhigh` as the default across the BaseFAQ agent system

Use them only after:

1. the parent agent has chosen the relevant skill or skills
2. the file ownership boundary is clear
3. the worker can operate without redefining strategy

## Execution Worker Catalog

- `backend-feature-worker.toml`
  - Execute scoped backend feature work inside BaseFAQ API and business modules.
- `portal-frontend-worker.toml`
  - Execute scoped Portal UI, data flow, and UX feedback work.
- `data-integrity-worker.toml`
  - Execute migrations, worker-processing, and integration-test tasks with strong data-boundary discipline.
- `ai-workflow-worker.toml`
  - Execute event-driven generation and matching changes inside BaseFAQ's AI flow.
- `domain-model-worker.toml`
  - Execute bounded modeling or planning work for the FAQ-to-Q&A transition.
- `distribution-worker.toml`
  - Execute bounded integration and distribution architecture tasks.
- `quality-review-worker.toml`
  - Perform bounded review and verification work after implementation is framed.

## Generic Specialist Catalog

- `code-review/readability-reviewer.subagent.md`
  - Reusable readability and maintainability reviewer for code input.
- `code-review/architecture-reviewer.subagent.md`
  - Reusable architecture and separation-of-concerns reviewer for code input.
- `code-review/performance-reviewer.subagent.md`
  - Reusable performance reviewer for visible hot paths and repeated work.
- `code-review/best-practices-reviewer.subagent.md`
  - Reusable best-practices reviewer for error handling and framework misuse.
- `security/injection-detector.subagent.md`
  - Reusable detector for command injection, SQL injection, eval, and path traversal patterns.
- `security/xss-detector.subagent.md`
  - Reusable detector for HTML, DOM, and templating XSS sinks.
- `security/deserialization-detector.subagent.md`
  - Reusable detector for unsafe deserialization patterns.
- `security/secrets-detector.subagent.md`
  - Reusable detector for hardcoded credentials and secret-like values.

## Naming Standards

- worker files use `kebab-case-worker.toml`
- markdown specialists use `kebab-case.subagent.md`
- `name` must match the filename stem exactly
- worker `role` must be `execution-worker`
- markdown specialists use `type: reusable-specialist`
- `model_reasoning_effort` defaults to `xhigh` when a worker definition supports it

The human-facing usage guide for workers and other agent artifacts lives in [`../../docs/agents/catalog.md`](../../docs/agents/catalog.md).

## Guardrails

- Workers and specialists do not select skills.
- Workers and specialists do not invent new BaseFAQ terminology.
- Workers and specialists do not widen scope unless the parent agent explicitly expands it.
- Workers must report changed files, validation performed, and residual risk.
