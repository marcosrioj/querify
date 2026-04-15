# Intent Routing

Use this file to map user prompts to the right BaseFAQ skill set.

## Primary Agent Routing

| Prompt Shape | Primary Agent |
|---|---|
| code review, review diff, review pr, analyze snippet, code quality, architecture review, performance review | `code-review-orchestrator.agent.md` |
| security review, vulnerability scan, xss, sql injection, hardcoded secret, unsafe deserialization | `security-orchestrator.agent.md` |
| privacy request, gdpr, lgpd, ccpa, pipl, consent withdrawal, dsar, delete my data | `privacy/privacy-orchestrator.agent.md` |

## Single-Skill Routing

| Prompt Shape | Primary Skill |
|---|---|
| create API, add endpoint, refactor command/query | `build-cqrs-feature-module` |
| public query, client key search, tenant-safe public read | `implement-tenant-aware-public-query` |
| portal page, domain hook, mutation wiring | `build-portal-domain-data-flow` |
| confirmation dialog, loading state, destructive action UX | `design-confirmed-actions-and-stateful-feedback` |
| localization, translated copy, RTL | `implement-portal-localization` |
| question thread, accepted answer, knowledge model | `model-question-thread-domain` |
| provenance, citation, trust score | `design-provenance-and-trust` |
| AI generation request, matching queue, async provider flow | `publish-asynchronous-ai-request` |
| migration, seed, local DB update | `apply-seed-and-migrations-safely` |
| local platform stack, Docker, observability bootstrapping | `bootstrap-local-platform-stack` |
| integration rollout, embed/sdk/plugin strategy | `prioritize-integration-rollout` |

## Agent-System Routing

When the prompt asks to create or update:

- a pattern
- shared logic/rules
- an agent
- a skill
- a subagent
- reusable orchestration behavior

Then:

1. Execute the relevant main domain skill first when the change is tied to a repository boundary.
2. Always run [`agent-system-maintenance.md`](agent-system-maintenance.md) before finishing.
3. Update the exact `.agents/` files that now hold the reusable knowledge.

## Security Routing

When the prompt asks to analyze code, config, or text for vulnerabilities:

1. Route to [`security-orchestrator.agent.md`](../security-orchestrator.agent.md).
2. Use shared skills:
   - [`../shared/code-parser.skill.md`](../shared/code-parser.skill.md)
   - [`../shared/pattern-matcher.skill.md`](../shared/pattern-matcher.skill.md)
3. Run all security specialists under `.subagents/security/`.
4. If the input is not relevant code/config/text, return `No security analysis needed`.

## Code Review Routing

When the prompt supplies code or review-oriented code input:

1. Route to [`code-review-orchestrator.agent.md`](../code-review-orchestrator.agent.md).
2. Use shared skills:
   - [`../shared/code-parser.skill.md`](../shared/code-parser.skill.md)
   - [`../shared/code-diff-parser.skill.md`](../shared/code-diff-parser.skill.md)
   - [`../shared/complexity-analyzer.skill.md`](../shared/complexity-analyzer.skill.md)
3. Run all code-review specialists under `.subagents/code-review/`.
4. Always run [`../security-orchestrator.agent.md`](../security-orchestrator.agent.md) when available.
5. If the input is not code, return `No code review needed`.

## Privacy Routing

When the prompt asks to interpret or fulfill a privacy right, consent change, or jurisdiction-specific privacy rule:

1. Route to [`../privacy/privacy-orchestrator.agent.md`](../privacy/privacy-orchestrator.agent.md).
2. Use the shared privacy engine:
   - [`../privacy/privacy-engine.subagent.md`](../privacy/privacy-engine.subagent.md)
3. Use flat operational skills:
   - [`../privacy/dsar.skill.md`](../privacy/dsar.skill.md)
   - [`../privacy/consent.skill.md`](../privacy/consent.skill.md)
   - [`../privacy/audit.skill.md`](../privacy/audit.skill.md)
   - [`../privacy/data-classification.skill.md`](../privacy/data-classification.skill.md)
4. Route to the correct jurisdiction specialist:
   - [`../privacy/gdpr.subagent.md`](../privacy/gdpr.subagent.md)
   - [`../privacy/lgpd.subagent.md`](../privacy/lgpd.subagent.md)
   - [`../privacy/ccpa.subagent.md`](../privacy/ccpa.subagent.md)
   - [`../privacy/pipl.subagent.md`](../privacy/pipl.subagent.md)
5. If more than one law applies, keep the union of obligations and the strictest compatible deadline.

## Multi-Skill Routing

### Create API

- Primary: `build-cqrs-feature-module`
- Supporting:
  - `write-real-database-integration-tests`
  - `enforce-cqrs-architecture-rules`

### Add Portal Feature

- Primary: `build-portal-domain-data-flow`
- Supporting:
  - `compose-portal-page-layouts`
  - `design-confirmed-actions-and-stateful-feedback`
  - `implement-portal-localization`

### Structure Q&A Knowledge

- Primary: `model-question-thread-domain`
- Supporting:
  - `design-provenance-and-trust`
  - `plan-faq-to-qna-upgrade`

### Design Distribution Layer

- Primary: `prioritize-integration-rollout`
- Supporting:
  - `design-provenance-and-trust` when public trust/citation behavior is part of the channel

### Extend Async AI Flow

- Primary: `publish-asynchronous-ai-request`
- Supporting:
  - `process-control-plane-work-items` when durable worker state is involved
  - `write-real-database-integration-tests`

### Evolve Agent System

- Primary: use the domain skill that exposed the reusable change
- Mandatory follow-up:
  - [`agent-system-maintenance.md`](agent-system-maintenance.md)

### Security Analysis

- Primary orchestrator: [`../security-orchestrator.agent.md`](../security-orchestrator.agent.md)
- Shared skills:
  - [`../shared/code-parser.skill.md`](../shared/code-parser.skill.md)
  - [`../shared/pattern-matcher.skill.md`](../shared/pattern-matcher.skill.md)
- Required specialists:
  - `.subagents/security/injection-detector.subagent.md`
  - `.subagents/security/xss-detector.subagent.md`
  - `.subagents/security/deserialization-detector.subagent.md`
  - `.subagents/security/secrets-detector.subagent.md`

### Code Review

- Primary orchestrator: [`../code-review-orchestrator.agent.md`](../code-review-orchestrator.agent.md)
- Shared skills:
  - [`../shared/code-parser.skill.md`](../shared/code-parser.skill.md)
  - [`../shared/code-diff-parser.skill.md`](../shared/code-diff-parser.skill.md)
  - [`../shared/complexity-analyzer.skill.md`](../shared/complexity-analyzer.skill.md)
- Required specialists:
  - `.subagents/code-review/readability-reviewer.subagent.md`
  - `.subagents/code-review/architecture-reviewer.subagent.md`
  - `.subagents/code-review/performance-reviewer.subagent.md`
  - `.subagents/code-review/best-practices-reviewer.subagent.md`
- Mandatory integration:
  - [`../security-orchestrator.agent.md`](../security-orchestrator.agent.md)

### Privacy Compliance

- Primary orchestrator: [`../privacy/privacy-orchestrator.agent.md`](../privacy/privacy-orchestrator.agent.md)
- Shared routing specialist:
  - [`../privacy/privacy-engine.subagent.md`](../privacy/privacy-engine.subagent.md)
- Required flat skills:
  - [`../privacy/dsar.skill.md`](../privacy/dsar.skill.md)
  - [`../privacy/consent.skill.md`](../privacy/consent.skill.md)
  - [`../privacy/audit.skill.md`](../privacy/audit.skill.md)
  - [`../privacy/data-classification.skill.md`](../privacy/data-classification.skill.md)
- Jurisdiction specialists:
  - [`../privacy/gdpr.subagent.md`](../privacy/gdpr.subagent.md)
  - [`../privacy/lgpd.subagent.md`](../privacy/lgpd.subagent.md)
  - [`../privacy/ccpa.subagent.md`](../privacy/ccpa.subagent.md)
  - [`../privacy/pipl.subagent.md`](../privacy/pipl.subagent.md)

## Conflict Resolution

When multiple skills seem plausible:

1. choose the skill that owns the repository boundary being changed
2. prefer BaseFAQ domain semantics over generic technology terms
3. add supporting skills only for cross-cutting requirements
4. do not use a subagent to break the tie
5. if reusable agent knowledge changed, update `.agents/` before closing the task
