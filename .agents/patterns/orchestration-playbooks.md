# Orchestration Playbooks

These are the standard multi-skill combinations for common BaseFAQ tasks.

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
