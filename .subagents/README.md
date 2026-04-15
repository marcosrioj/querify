# Generic Subagents

`.subagents/` contains reusable low-priority specialists that are not BaseFAQ-specific.

Rules:

- they do not orchestrate
- they do not define business logic
- they focus on one reusable concern
- they are called by primary agents under `.agents/`

Naming standards:

- generic specialist files use `kebab-case.subagent.md`
- front matter `name` must match the filename stem exactly
- markdown specialists use `type: reusable-specialist`
- each generic specialist should expose `Purpose`, `Inputs`, `Outputs`, `Behavior`, and `Example Usage` as the core operator-facing sections

The human-facing usage guide for these specialists lives in [`../docs/agents/catalog.md`](../docs/agents/catalog.md).

## Security Specialists

- `security/injection-detector.subagent.md`
- `security/xss-detector.subagent.md`
- `security/deserialization-detector.subagent.md`
- `security/secrets-detector.subagent.md`

## Code Review Specialists

- `code-review/readability-reviewer.subagent.md`
- `code-review/architecture-reviewer.subagent.md`
- `code-review/performance-reviewer.subagent.md`
- `code-review/best-practices-reviewer.subagent.md`
