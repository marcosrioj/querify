# BaseFAQ Glossary

- `Portal`
  - The authenticated tenant-facing React application in `apps/portal`.

- `BackOffice`
  - The global administrative surface for tenants, users, and provider setup.

- `Public API`
  - Anonymous or client-key-driven surfaces for public FAQ or billing ingress.

- `TenantDbContext`
  - The global control-plane database for tenant metadata, client keys, billing state, AI provider configuration, and background work items.

- `FaqDbContext`
  - The tenant product-data database for FAQs, FAQ items, answers, tags, content references, votes, and feedback.

- `X-Tenant-Id`
  - The tenant-scoping header used by authenticated tenant APIs.

- `X-Client-Key`
  - The public tenant-resolution credential used by FAQ public APIs.

- `QuestionSpace`
  - The future top-level Q&A operating context for a collection of threads.

- `Question`
  - The future user-facing thread aggregate.

- `Answer`
  - A candidate response to a question, including official, community, imported, or AI-assisted variants.

- `Accepted Answer`
  - The answer currently promoted as the canonical resolution for a question thread.

- `ThreadActivity`
  - Append-only history describing moderation, revision, or lifecycle events on a question thread.

- `KnowledgeSource`
  - A reusable artifact record used for provenance, trust, and evidence linkage.

- `Control-plane work item`
  - A retryable background-processing record stored in `TenantDbContext`, such as webhook inbox or email outbox rows.
