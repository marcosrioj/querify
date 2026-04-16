# Agent Ecosystem Catalog

This catalog is the human-readable reference for every orchestrator, subagent, skill, and worker currently defined in the repository.

## Orchestrators

| Artifact | Path | Use When | Composes |
|---|---|---|---|
| `code-review-orchestrator` | `.agents/code-review-orchestrator.agent.md` | The prompt includes code, diffs, PR-style review input, or requests code-quality analysis. | Shared review skills, code-review specialists, and security review. |
| `security-orchestrator` | `.agents/security-orchestrator.agent.md` | The prompt asks for vulnerability analysis over code, config, templates, or code-like text. | `code-parser`, `pattern-matcher`, and all security detectors. |
| `privacy-orchestrator` | `.agents/privacy/privacy-orchestrator.agent.md` | The prompt asks for privacy-law applicability, consent changes, DSAR handling, or multi-law privacy routing. | `privacy-engine`, law specialists, and privacy flat skills. |

## Privacy Subagents

| Artifact | Path | Use When | Called By |
|---|---|---|---|
| `privacy-engine` | `.agents/privacy/privacy-engine.subagent.md` | A privacy request needs jurisdiction resolution and shared control planning before law interpretation. | `privacy-orchestrator` |
| `gdpr` | `.agents/privacy/gdpr.subagent.md` | The privacy engine determines GDPR or EU/EEA rights are relevant. | `privacy-orchestrator` |
| `lgpd` | `.agents/privacy/lgpd.subagent.md` | The privacy engine determines Brazil LGPD obligations are relevant. | `privacy-orchestrator` |
| `ccpa` | `.agents/privacy/ccpa.subagent.md` | The privacy engine determines California resident rights under CCPA or CPRA are relevant. | `privacy-orchestrator` |
| `pipl` | `.agents/privacy/pipl.subagent.md` | The privacy engine determines China PIPL obligations are relevant. | `privacy-orchestrator` |

## Generic Reusable Subagents

| Artifact | Path | Use When | Called By |
|---|---|---|---|
| `readability-reviewer` | `.agents/subagents/code-review/readability-reviewer.subagent.md` | Code review needs clarity, naming, duplication, or scanability findings. | `code-review-orchestrator` |
| `architecture-reviewer` | `.agents/subagents/code-review/architecture-reviewer.subagent.md` | Code review needs separation-of-concerns or architectural anti-pattern findings. | `code-review-orchestrator` |
| `performance-reviewer` | `.agents/subagents/code-review/performance-reviewer.subagent.md` | Code review needs visible hot-path, loop, or repeated-I/O analysis. | `code-review-orchestrator` |
| `best-practices-reviewer` | `.agents/subagents/code-review/best-practices-reviewer.subagent.md` | Code review needs missing error handling, async misuse, or framework misuse findings. | `code-review-orchestrator` |
| `injection-detector` | `.agents/subagents/security/injection-detector.subagent.md` | Security analysis needs command injection, SQL injection, eval, or path traversal detection. | `security-orchestrator` |
| `xss-detector` | `.agents/subagents/security/xss-detector.subagent.md` | Security analysis needs HTML or DOM sink review for XSS risk. | `security-orchestrator` |
| `deserialization-detector` | `.agents/subagents/security/deserialization-detector.subagent.md` | Security analysis needs unsafe deserialization review. | `security-orchestrator` |
| `secrets-detector` | `.agents/subagents/security/secrets-detector.subagent.md` | Security analysis needs hardcoded-secret detection. | `security-orchestrator` |

## Flat Operational Skills

### Shared

| Artifact | Path | Use When | Used By |
|---|---|---|---|
| `code-parser` | `.agents/shared/code-parser.skill.md` | An orchestrator or specialist must normalize code-like, diff-like, or config-like input. | Review and security orchestrators and specialists. |
| `code-diff-parser` | `.agents/shared/code-diff-parser.skill.md` | Review work must isolate changed blocks from diffs or patch-like input. | Code-review orchestrator and review specialists. |
| `complexity-analyzer` | `.agents/shared/complexity-analyzer.skill.md` | Review work needs visible complexity and deep-nesting signals. | Code-review orchestrator and review specialists. |
| `pattern-matcher` | `.agents/shared/pattern-matcher.skill.md` | Security specialists need conservative matching over risky sinks and secret-like literals. | Security orchestrator and security specialists. |

### Privacy

| Artifact | Path | Use When | Used By |
|---|---|---|---|
| `dsar` | `.agents/privacy/dsar.skill.md` | A privacy specialist has already framed a rights request and needs intake validation and fulfillment planning. | Privacy orchestrator and law specialists. |
| `consent` | `.agents/privacy/consent.skill.md` | A privacy workflow needs consent capture, refresh, withdrawal, or proof. | Privacy orchestrator and law specialists. |
| `audit` | `.agents/privacy/audit.skill.md` | A privacy workflow needs decision logging, evidence capture, or traceability. | Privacy orchestrator, privacy engine, and law specialists. |
| `data-classification` | `.agents/privacy/data-classification.skill.md` | Privacy logic needs personal-data or sensitivity classification before deciding controls. | Privacy orchestrator, privacy engine, and law specialists. |

## Repository-Owned Skills

| Skill | Path | Use When | Common Pairings |
|---|---|---|---|
| `build-cqrs-feature-module` | `.agents/skills/backend/build-cqrs-feature-module/SKILL.md` | Adding or refactoring a `.NET` backend feature in BaseFAQ CQRS shape, including feature-project decomposition and real-file ownership for QnA. | `write-real-database-integration-tests`, `enforce-cqrs-architecture-rules` |
| `implement-tenant-aware-public-query` | `.agents/skills/backend/implement-tenant-aware-public-query/SKILL.md` | Building safe tenant-aware public reads. | `write-real-database-integration-tests` |
| `build-portal-domain-data-flow` | `.agents/skills/frontend/build-portal-domain-data-flow/SKILL.md` | Adding or extending Portal domain APIs, hooks, and routes. | `compose-portal-page-layouts`, `design-confirmed-actions-and-stateful-feedback`, `implement-portal-localization` |
| `compose-portal-page-layouts` | `.agents/skills/frontend/compose-portal-page-layouts/SKILL.md` | Structuring Portal pages with shared layout patterns. | `build-portal-domain-data-flow`, `design-confirmed-actions-and-stateful-feedback` |
| `design-confirmed-actions-and-stateful-feedback` | `.agents/skills/frontend/design-confirmed-actions-and-stateful-feedback/SKILL.md` | Adding confirmations, pending, empty, and error states. | `compose-portal-page-layouts`, `implement-portal-localization` |
| `implement-portal-localization` | `.agents/skills/frontend/implement-portal-localization/SKILL.md` | Managing copy, language precedence, and RTL or LTR rules in Portal. | `build-portal-domain-data-flow`, `design-confirmed-actions-and-stateful-feedback` |
| `apply-seed-and-migrations-safely` | `.agents/skills/data/apply-seed-and-migrations-safely/SKILL.md` | Applying seed and migration workflows through repository tooling. | `write-real-database-integration-tests` |
| `bootstrap-local-platform-stack` | `.agents/skills/data/bootstrap-local-platform-stack/SKILL.md` | Bringing up the supported local runtime and observability stack. | `apply-seed-and-migrations-safely` |
| `enforce-cqrs-architecture-rules` | `.agents/skills/data/enforce-cqrs-architecture-rules/SKILL.md` | Encoding or updating repository-wide CQRS compliance checks. | `build-cqrs-feature-module`, `write-real-database-integration-tests` |
| `process-control-plane-work-items` | `.agents/skills/data/process-control-plane-work-items/SKILL.md` | Implementing lease-based retryable background processing in `TenantDbContext`. | `write-real-database-integration-tests` |
| `write-real-database-integration-tests` | `.agents/skills/data/write-real-database-integration-tests/SKILL.md` | Verifying backend behavior with real PostgreSQL-backed integration coverage. | Backend, data, and worker skills |
| `model-question-thread-domain` | `.agents/skills/domain/model-question-thread-domain/SKILL.md` | Modeling the question-thread domain and core aggregates. | `design-provenance-and-trust` |
| `design-provenance-and-trust` | `.agents/skills/domain/design-provenance-and-trust/SKILL.md` | Designing evidence, citations, confidence, and trust semantics. | `model-question-thread-domain` |
| `publish-asynchronous-ai-request` | `.agents/skills/ai/publish-asynchronous-ai-request/SKILL.md` | Starting generation or matching through async events and correlation ids. | `process-control-plane-work-items`, `write-real-database-integration-tests` |
| `prioritize-integration-rollout` | `.agents/skills/distribution/prioritize-integration-rollout/SKILL.md` | Planning integration and distribution rollout for embeds, SDKs, and plugins. | `design-provenance-and-trust` |

## Execution Workers

| Worker | Path | Use When | Recommended Skills |
|---|---|---|---|
| `backend-feature-worker` | `.agents/subagents/backend-feature-worker.toml` | The parent agent already framed a bounded backend change. | Backend implementation skills. |
| `portal-frontend-worker` | `.agents/subagents/portal-frontend-worker.toml` | The parent agent already framed a bounded Portal change. | Frontend implementation skills. |
| `data-integrity-worker` | `.agents/subagents/data-integrity-worker.toml` | The parent agent already framed migration, persistence, worker, or integration-test work. | Data, migration, and architecture-rule skills. |
| `ai-workflow-worker` | `.agents/subagents/ai-workflow-worker.toml` | The parent agent already framed an async workflow change. | Workflow and worker-processing skills. |
| `domain-model-worker` | `.agents/subagents/domain-model-worker.toml` | The parent agent already framed bounded domain modeling or roadmap work. | Domain and provenance skills. |
| `distribution-worker` | `.agents/subagents/distribution-worker.toml` | The parent agent already framed bounded integration or distribution planning work. | Distribution and trust skills. |
| `quality-review-worker` | `.agents/subagents/quality-review-worker.toml` | The parent agent already framed a review or verification pass. | Review and verification-oriented skills. |

## How To Use This Catalog

1. Pick the smallest valid entry point.
2. Route to an orchestrator if the task spans multiple specialists or skills.
3. Route to a repository-owned skill if one repository boundary clearly owns the work.
4. Use flat skills only for reusable operational actions.
5. Use workers only when the parent agent has already made the strategy decision.
