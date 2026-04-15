---
name: apply-seed-and-migrations-safely
description: Initialize and evolve BaseFAQ tenant and FAQ databases through the repository tools rather than ad hoc database changes.
type: repository-skill
scope: basefaq-repository
category: data
priority: high
triggers:
  - seed database
  - apply migration
  - faq schema update
  - tenant metadata setup
owned_paths:
  - dotnet/BaseFaq.Tools.Seed
  - dotnet/BaseFaq.Tools.Migration
  - dotnet/BaseFaq.Common.EntityFramework.*
  - dotnet/BaseFaq.Faq.Common.Persistence.FaqDb
collaborates_with:
  - write-real-database-integration-tests
---

# Apply Seed And Migrations Safely

## When to Use

- A clean environment needs tenant metadata, sample data, or AI prerequisites.
- A schema change in `TenantDbContext` or `FaqDbContext` needs to be applied safely.

## Responsibilities

- Use the repository tools in the right order.
- Keep tenant metadata and FAQ database routing coherent.
- Preserve additive migration strategy during the FAQ-to-Q&A transition.

## Workflow

1. Use `BaseFaq.Tools.Seed` for clean initialization and essential data.
2. Use `BaseFaq.Tools.Migration` for FAQ database update flows once tenant metadata exists.
3. Keep `TenantDbContext` and `FaqDbContext` changes coordinated but distinct.
4. Record AI prerequisites such as the seeded `Ai:UserId` when required.
5. Re-run integration coverage after schema changes.

## BaseFAQ Domain Alignment

- Seeding and migration tooling is the supported path.
- Tenant metadata must exist before tenant FAQ database updates can fan out.
- The FAQ-to-Q&A transition should stay additive rather than rewrite-oriented.

## Collaborates With

- [`write-real-database-integration-tests`](../write-real-database-integration-tests/SKILL.md)

## Done When

- Databases are migrated through BaseFAQ tooling.
- Required seed data exists for the target workflow.
- Schema changes are backed by integration validation.
