# Generic Subagents

`.subagents/` contains reusable low-priority specialists that are not BaseFAQ-specific.

Rules:

- they do not orchestrate
- they do not define business logic
- they focus on one reusable concern
- they are called by primary agents under `.agents/`

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
