---
name: build-cqrs-feature-module
description: Build or refactor a BaseFAQ backend capability into the repository-standard CQRS module shape.
type: repository-skill
scope: basefaq-repository
category: backend
priority: high
triggers:
  - create api
  - add endpoint
  - new command
  - new query
  - refactor backend feature
owned_paths:
  - dotnet/BaseFaq.Faq.*
  - dotnet/BaseFaq.Tenant.*
collaborates_with:
  - write-real-database-integration-tests
  - enforce-cqrs-architecture-rules
---

# Build CQRS Feature Module

## When to Use

- A prompt asks to create or extend a `.NET` API capability in BaseFAQ.
- A backend workflow needs to be reshaped into controllers, services, commands, queries, and DI wiring.

## Responsibilities

- Choose the correct bounded-context project.
- Preserve BaseFAQ's thin-controller, thin-service, MediatR-first structure.
- Keep write contracts aligned with repository CQRS rules.

## Workflow

1. Identify whether the feature belongs to `BaseFaq.Faq.*` or `BaseFaq.Tenant.*`.
2. Add or extend the smallest feature module that owns the use case.
3. Keep writes in `Commands/<Action>/` and reads in `Queries/<Action>/`.
4. Keep controller code HTTP-focused and service code orchestration-thin.
5. Register the feature through `Extensions/ServiceCollectionExtensions.cs` and host `AddFeatures(...)`.
6. Pair the change with repository-rule validation and integration coverage.

## BaseFAQ Domain Alignment

- Command handlers return simple values only.
- Write flows do not shape read DTOs after persistence.
- `TenantDbContext` and `FaqDbContext` stay separate ownership boundaries.
- Async write requests return a correlation `Guid` and `202 Accepted`.

## Collaborates With

- [`write-real-database-integration-tests`](../../data/write-real-database-integration-tests/SKILL.md)
- [`enforce-cqrs-architecture-rules`](../../data/enforce-cqrs-architecture-rules/SKILL.md)

## Done When

- The feature lives in the right bounded-context project.
- Command and query responsibilities are separated cleanly.
- The controller/service/handler flow matches BaseFAQ standards.
- Tests or rule checks exist for the new behavior.
