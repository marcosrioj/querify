# BaseFAQ Unified Agent System

## Mission

`.agents/` is the single source of truth for how Codex should reason about, plan, and execute work in this repository.

This system exists to make BaseFAQ work predictable:

- understand future prompts without requiring explicit skill names
- route work to the smallest correct BaseFAQ skill set
- use subagents only as execution workers
- preserve BaseFAQ architecture, domain language, and delivery standards

## Domain Context (BaseFAQ)

BaseFAQ is a multi-tenant platform built from:

- a React Portal in `apps/portal`
- multiple `.NET 10` API and worker hosts in `dotnet/`
- two primary EF Core ownership boundaries:
  - `TenantDbContext` for tenant metadata, users, AI provider configuration, client keys, billing, and control-plane jobs
  - `FaqDbContext` for tenant FAQ product data
- an event-driven AI runtime for generation and matching
- an evolving product direction from static FAQ blocks toward question threads, accepted answers, provenance, and trust

Read these support files before making broad changes:

1. [`shared/basefaq-domain-context.md`](shared/basefaq-domain-context.md)
2. [`shared/basefaq-engineering-standards.md`](shared/basefaq-engineering-standards.md)
3. [`patterns/intent-routing.md`](patterns/intent-routing.md)

## Source Of Truth And Precedence

Use this precedence order:

1. `AGENTS.md`
2. the selected skill or skills under `skills/`
3. shared context, patterns, glossary, and templates under `.agents/`
4. subagents under `subagents/`

Hard rule: skills outrank subagents.

- Skills decide what kind of work BaseFAQ needs.
- Subagents execute bounded work that has already been framed by one or more skills.
- Subagents must not invent strategy, rename the domain, or override BaseFAQ standards.

## How To Think

For every prompt, reason in this order:

1. Identify the user outcome.
   - Is the user asking for implementation, architecture, product planning, testing, runtime setup, AI flow work, or distribution work?
2. Map the outcome to a BaseFAQ boundary.
   - Portal frontend
   - FAQ backend
   - Tenant/control-plane backend
   - AI workflow
   - data and migration flow
   - domain and product model
   - integration/distribution layer
3. Choose the smallest primary skill that owns the work.
4. Add supporting skills only when they are materially required.
   - Example: a new API usually needs backend implementation plus integration testing and CQRS rule checks.
5. Decide whether a subagent is useful.
   - Use a subagent only when there is a clear bounded execution unit.
   - Do not delegate the immediate strategy decision.
6. Validate against BaseFAQ standards before executing.

## How To Select Skills

Use intent-first routing, not title matching alone.

### Typical prompt patterns

- "create API", "add endpoint", "new command/query", "refactor service module"
  - Primary: `build-cqrs-feature-module`
  - Supporting: `write-real-database-integration-tests`, `enforce-cqrs-architecture-rules`

- "public query", "search by client key", "tenant-aware public read"
  - Primary: `implement-tenant-aware-public-query`
  - Supporting: `write-real-database-integration-tests`

- "trigger AI generation", "queue matching flow", "async AI request"
  - Primary: `publish-asynchronous-ai-request`
  - Supporting: `process-control-plane-work-items`, `write-real-database-integration-tests`

- "build portal page", "wire page data", "new domain screen"
  - Primary: `build-portal-domain-data-flow`
  - Supporting: `compose-portal-page-layouts`, `design-confirmed-actions-and-stateful-feedback`, `implement-portal-localization`

- "improve UX feedback", "confirm deletion", "loading and empty states"
  - Primary: `design-confirmed-actions-and-stateful-feedback`
  - Supporting: `compose-portal-page-layouts`

- "structure Q&A knowledge", "question threads", "accepted answers", "provenance"
  - Primary: `model-question-thread-domain`
  - Supporting: `design-provenance-and-trust`, `plan-faq-to-qna-upgrade`

- "design distribution layer", "integrations strategy", "embed/sdk/plugin rollout"
  - Primary: `prioritize-integration-rollout`

- "seed database", "apply migrations", "stand up local stack"
  - Primary: `apply-seed-and-migrations-safely` or `bootstrap-local-platform-stack`
  - Supporting: `write-real-database-integration-tests` when schema changes are involved

When more than one skill applies, prefer one primary skill plus only the minimum supporting set needed to finish the task end-to-end.

## How To Use Subagents

Subagents are workers, not planners.

Use a subagent only when:

- the skill selection is already clear
- the task can be scoped to a file set or execution boundary
- the parent agent can still review and integrate the result

Do not use a subagent when:

- the main uncertainty is which pattern BaseFAQ expects
- the task is mostly architecture selection
- the affected boundary is still ambiguous

### Delegation rules

- Delegate implementation, analysis, documentation drafting, or focused verification.
- Keep each worker on a narrow ownership boundary.
- Pass the selected skill names into the worker prompt.
- Require the worker to preserve BaseFAQ terminology and standards.
- Review every worker output against the selected skills before using it.

## Execution Flow

1. Read the prompt and inspect the relevant repository area.
2. Use [`patterns/intent-routing.md`](patterns/intent-routing.md) to choose a primary skill.
3. Read the selected skill files completely before acting.
4. Pull supporting context from:
   - [`shared/basefaq-domain-context.md`](shared/basefaq-domain-context.md)
   - [`shared/basefaq-engineering-standards.md`](shared/basefaq-engineering-standards.md)
   - [`glossary/basefaq-glossary.md`](glossary/basefaq-glossary.md)
5. If the work is large, use [`patterns/orchestration-playbooks.md`](patterns/orchestration-playbooks.md) to compose multiple skills.
6. Delegate only the pieces that are safe for execution workers.
7. Implement the change.
8. Validate behavior with the right level of evidence:
   - code review for architecture-only changes
   - integration tests for backend and data changes
   - build or lint checks for frontend and docs changes
9. Report the selected skills, changed files, validation performed, and residual risk.

## Output Standards

Every substantial task should produce:

- the primary skill used
- any supporting skills used
- the BaseFAQ boundary touched
- concrete file or path ownership
- validation performed
- residual risk or follow-up

Keep output aligned with BaseFAQ language:

- say `TenantDbContext` and `FaqDbContext`, not "main DB" and "secondary DB"
- say `Portal`, `BackOffice`, `Public API`, and `AI worker` when those distinctions matter
- say `question thread`, `accepted answer`, and `provenance` when the domain is about the FAQ-to-Q&A transition

## Skill Catalog

The active skill index lives in [`skills/README.md`](skills/README.md).

## Worker Catalog

The active worker index lives in [`subagents/README.md`](subagents/README.md).
