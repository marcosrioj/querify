# BaseFAQ Domain Context

## Product Shape

BaseFAQ is a multi-tenant FAQ platform evolving toward a broader Q&A knowledge system.

Current and emerging concepts:

- tenants and tenant memberships
- FAQs and FAQ items
- content references and tags
- votes and feedback
- AI-assisted generation and similarity matching
- question threads, accepted answers, provenance, and trust

## Runtime Surfaces

- `apps/portal`
  - authenticated tenant workspace
- `BaseFaq.Tenant.BackOffice.Api`
  - global administration
- `BaseFaq.Tenant.Portal.Api`
  - tenant workspace settings and members
- `BaseFaq.Tenant.Public.Api`
  - public tenant ingress such as billing webhooks
- `BaseFaq.Faq.Portal.Api`
  - authenticated FAQ management
- `BaseFaq.Faq.Public.Api`
  - public FAQ access
- `BaseFaq.AI.Api`
  - event-driven AI worker
- `BaseFaq.Tenant.Worker.Api`
  - control-plane background processing

## Data Ownership

- `TenantDbContext`
  - tenants
  - users and memberships
  - client keys
  - AI provider configuration
  - billing state
  - control-plane work items

- `FaqDbContext`
  - tenant product data
  - FAQs and FAQ items
  - answer variants and feedback
  - content references and tags

## Request Context Rules

- authenticated Portal APIs usually require `X-Tenant-Id`
- public FAQ APIs resolve tenant context from `X-Client-Key`
- public billing webhooks are anonymous ingress endpoints
- AI consumers receive tenant identity from message contracts, not HTTP headers

## Domain Direction

The long-term domain is not a static FAQ page generator. It is moving toward:

- `QuestionSpace` as the operating context
- `Question` as the thread
- `Answer` as a candidate response
- accepted answers instead of single immutable answer bodies
- `ThreadActivity` for append-only lifecycle history
- explicit provenance and trust for AI-assisted or imported content

## Integration Direction

BaseFAQ aims to expose a disciplined `/integrations` layer built around:

- canonical HTML rendering
- embeddable web delivery
- framework adapters
- CMS plugins
- server-side SDKs
- distribution discipline around SEO, accessibility, and versioned contracts
