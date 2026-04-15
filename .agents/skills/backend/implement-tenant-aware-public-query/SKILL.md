---
name: implement-tenant-aware-public-query
description: Build safe public read paths that resolve tenant context from public credentials before touching tenant FAQ data.
type: repository-skill
scope: basefaq-repository
category: backend
priority: high
triggers:
  - public query
  - client key search
  - public faq search
  - tenant aware read
owned_paths:
  - dotnet/BaseFaq.Faq.Public.*
  - dotnet/BaseFaq.Tenant.Public.*
collaborates_with:
  - write-real-database-integration-tests
---

# Implement Tenant-Aware Public Query

## When to Use

- A public API endpoint must read tenant data safely.
- The request is driven by `X-Client-Key` or another public credential instead of JWT plus `X-Tenant-Id`.

## Responsibilities

- Resolve tenant identity before reading tenant FAQ data.
- Keep public handlers read-only and projection-focused.
- Preserve stable ordering, filtering, and public-safe projections.

## Workflow

1. Read the public credential from the request-scoped context.
2. Resolve the tenant through the repository resolver layer.
3. Store the resolved tenant context where downstream code expects it.
4. Filter early by tenant id in `FaqDbContext`.
5. Apply public-safe filters, visibility rules, paging, and deterministic ordering.
6. Project directly to read DTOs or shared projections.

## BaseFAQ Domain Alignment

- Public FAQ access belongs in `BaseFaq.Faq.Public.*`.
- Tenant resolution happens before any tenant-scoped query.
- Public flows stay query-first; they do not smuggle write behavior into the read path.

## Collaborates With

- [`write-real-database-integration-tests`](../../data/write-real-database-integration-tests/SKILL.md)

## Done When

- Missing and invalid public credentials are handled safely.
- Tenant filtering is explicit and early in the query path.
- The handler returns a stable public read model with deterministic behavior.
