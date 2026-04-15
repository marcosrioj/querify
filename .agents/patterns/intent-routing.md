# Intent Routing

Use this file to map user prompts to the right BaseFAQ skill set.

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

## Conflict Resolution

When multiple skills seem plausible:

1. choose the skill that owns the repository boundary being changed
2. prefer BaseFAQ domain semantics over generic technology terms
3. add supporting skills only for cross-cutting requirements
4. do not use a subagent to break the tie
