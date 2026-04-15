# BaseFAQ Subagents

Subagents are execution workers. They are intentionally lower priority than skills.

The worker catalog is also self-maintained:

- if a future task reveals a new recurring execution boundary, add or update the corresponding worker
- if a worker becomes obsolete or its ownership changes, update this catalog in the same task
- worker changes should also trigger review of routing and orchestration files through [`../patterns/agent-system-maintenance.md`](../patterns/agent-system-maintenance.md)

Generic reusable specialists that are not BaseFAQ-specific belong in the root `.subagents/` tree, not here.

Reasoning-depth rule:

- when a worker definition supports `model_reasoning_effort`, use `xhigh` as the default across the BaseFAQ agent system

Use them only after:

1. the parent agent has chosen the relevant skill or skills
2. the file ownership boundary is clear
3. the worker can operate without redefining strategy

## Worker Catalog

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

## Guardrails

- Workers do not select skills.
- Workers do not invent new BaseFAQ terminology.
- Workers do not widen scope unless the parent agent explicitly expands it.
- Workers must report changed files, validation performed, and residual risk.
