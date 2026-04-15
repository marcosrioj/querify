---
name: process-control-plane-work-items
description: Implement BaseFAQ's lease-based background-processing pattern for retryable work stored in TenantDbContext.
category: data
priority: high
triggers:
  - background worker
  - work item processor
  - webhook inbox
  - email outbox
owned_paths:
  - dotnet/BaseFaq.Tenant.Worker.*
  - dotnet/BaseFaq.Tenant.Public.*
  - dotnet/BaseFaq.Common.EntityFramework.Tenant
collaborates_with:
  - write-real-database-integration-tests
---

# Process Control-Plane Work Items

## When to Use

- A table in `TenantDbContext` represents retryable background work.
- Multiple worker instances may poll the same source and need safe claiming behavior.

## Responsibilities

- Claim work items safely with leases.
- Implement retry, terminal failure, and polling behavior.
- Keep control-plane processing separate from FAQ product-data workflows.

## Workflow

1. Poll eligible `Pending` rows whose retry and lease windows have expired.
2. Claim rows atomically with a processing token and lease fields.
3. Reload each row by id and token before processing.
4. Dispatch a single-item command that owns the unit of work.
5. Finalize the row as `Completed`, `Pending`, or `Failed`.
6. Loop immediately when work exists and wait only when the queue is empty.

## BaseFAQ Domain Alignment

- Control-plane work belongs in `TenantDbContext`, not `FaqDbContext`.
- Billing webhook inboxes and email outbox processing are the reference shapes.
- Lease-based coordination replaces in-memory locks.

## Collaborates With

- [`write-real-database-integration-tests`](../write-real-database-integration-tests/SKILL.md)

## Done When

- Work items are claimed safely across concurrent workers.
- Retry and terminal-failure semantics are explicit.
- The processor stays inside the control-plane boundary.
