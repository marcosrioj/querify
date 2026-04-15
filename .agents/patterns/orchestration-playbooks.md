# Orchestration Playbooks

These are the standard multi-skill combinations for common BaseFAQ tasks.

## Mandatory Post-Task Pass

After any playbook completes, run [`agent-system-maintenance.md`](agent-system-maintenance.md).

If the task created reusable routing, logic, standards, skill boundaries, or worker boundaries, update `.agents/` before closing the work.

## Backend Feature Playbook

Use when the prompt is effectively "create API" or "add a backend capability."

1. `build-cqrs-feature-module`
2. `write-real-database-integration-tests`
3. `enforce-cqrs-architecture-rules`
4. Optional worker: `backend-feature-worker`

## Portal Feature Playbook

Use when the prompt is about a new Portal screen or workflow.

1. `build-portal-domain-data-flow`
2. `compose-portal-page-layouts`
3. `design-confirmed-actions-and-stateful-feedback`
4. `implement-portal-localization` when new copy or language logic exists
5. Optional worker: `portal-frontend-worker`

## Knowledge Transition Playbook

Use when the prompt is about moving BaseFAQ toward Q&A semantics.

1. `model-question-thread-domain`
2. `design-provenance-and-trust`
3. `plan-faq-to-qna-upgrade`
4. Optional worker: `domain-model-worker`

## AI Workflow Playbook

Use when the prompt involves queue-backed generation or matching.

1. `publish-asynchronous-ai-request`
2. `process-control-plane-work-items` when durable background processing exists
3. `write-real-database-integration-tests`
4. Optional worker: `ai-workflow-worker`

## Distribution Playbook

Use when the prompt concerns external integrations, embeds, or SDK rollout.

1. `prioritize-integration-rollout`
2. `design-provenance-and-trust` if public content trust is part of the surface
3. Optional worker: `distribution-worker`

## Privacy Compliance Playbook

Use when the prompt concerns privacy rights, consent lifecycle, privacy-law applicability, or multi-jurisdiction privacy handling.

1. Run [`../privacy/privacy-orchestrator.agent.md`](../privacy/privacy-orchestrator.agent.md).
2. Resolve applicability with:
   - [`../privacy/privacy-engine.subagent.md`](../privacy/privacy-engine.subagent.md)
3. Route to one or more jurisdiction specialists:
   - [`../privacy/gdpr.subagent.md`](../privacy/gdpr.subagent.md)
   - [`../privacy/lgpd.subagent.md`](../privacy/lgpd.subagent.md)
   - [`../privacy/ccpa.subagent.md`](../privacy/ccpa.subagent.md)
   - [`../privacy/pipl.subagent.md`](../privacy/pipl.subagent.md)
4. Execute the minimum required operational skills:
   - [`../privacy/dsar.skill.md`](../privacy/dsar.skill.md)
   - [`../privacy/consent.skill.md`](../privacy/consent.skill.md)
   - [`../privacy/data-classification.skill.md`](../privacy/data-classification.skill.md)
   - [`../privacy/audit.skill.md`](../privacy/audit.skill.md)
5. De-duplicate overlapping obligations and preserve the strictest compatible deadline.

## Security Analysis Playbook

Use when the prompt is about vulnerability review or security-focused static analysis.

1. Run [`../security-orchestrator.agent.md`](../security-orchestrator.agent.md).
2. Normalize the input with:
   - [`../shared/code-parser.skill.md`](../shared/code-parser.skill.md)
   - [`../shared/pattern-matcher.skill.md`](../shared/pattern-matcher.skill.md)
3. Execute all specialists:
   - `.subagents/security/injection-detector.subagent.md`
   - `.subagents/security/xss-detector.subagent.md`
   - `.subagents/security/deserialization-detector.subagent.md`
   - `.subagents/security/secrets-detector.subagent.md`
4. De-duplicate and validate the final report inside the orchestrator before returning results.

## Code Review Playbook

Use when the prompt is about code quality, PR review, diff review, snippet review, maintainability, architecture, performance, or best-practices review.

1. Run [`../code-review-orchestrator.agent.md`](../code-review-orchestrator.agent.md).
2. Normalize the input with:
   - [`../shared/code-parser.skill.md`](../shared/code-parser.skill.md)
   - [`../shared/code-diff-parser.skill.md`](../shared/code-diff-parser.skill.md)
   - [`../shared/complexity-analyzer.skill.md`](../shared/complexity-analyzer.skill.md)
3. Execute all specialists:
   - `.subagents/code-review/readability-reviewer.subagent.md`
   - `.subagents/code-review/architecture-reviewer.subagent.md`
   - `.subagents/code-review/performance-reviewer.subagent.md`
   - `.subagents/code-review/best-practices-reviewer.subagent.md`
4. Always run [`../security-orchestrator.agent.md`](../security-orchestrator.agent.md) when available.
5. De-duplicate, validate, score, and render the final review inside the orchestrator.

## Agent System Maintenance Playbook

Use when the prompt explicitly asks to add or update:

- pattern
- logic
- agent behavior
- skill
- subagent
- reusable orchestration rules

1. Apply the main domain playbook first if the change is tied to a repository feature.
2. Run [`agent-system-maintenance.md`](agent-system-maintenance.md).
3. Update the minimum correct files in:
   - `AGENTS.md`
   - `patterns/`
   - `shared/`
   - `glossary/`
   - `skills/`
   - `subagents/`
   - `templates/`
4. Prefer updating the existing catalog before creating new artifacts.
