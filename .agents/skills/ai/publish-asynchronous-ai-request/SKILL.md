---
name: publish-asynchronous-ai-request
description: Start BaseFAQ generation or matching work asynchronously through versioned events, tenant-aware provider resolution, and correlation ids.
type: repository-skill
scope: basefaq-repository
category: ai
priority: high
triggers:
  - async ai request
  - generation queue
  - matching event
  - publish rabbitmq event
owned_paths:
  - dotnet/BaseFaq.Faq.Portal.*
  - dotnet/BaseFaq.Faq.Public.*
  - dotnet/BaseFaq.AI.*
  - dotnet/BaseFaq.Models.Ai
collaborates_with:
  - process-control-plane-work-items
  - write-real-database-integration-tests
---

# Publish Asynchronous AI Request

## When to Use

- A user action should trigger generation or matching without blocking the request path.
- A write flow must return a correlation id rather than waiting for provider latency.

## Responsibilities

- Validate prerequisites before queueing AI work.
- Resolve whether the tenant has a compatible provider configured.
- Publish versioned events with tenant metadata and correlation ids.

## Workflow

1. Load the target entity inside the correct data boundary.
2. Validate readiness: language, source material, content references, or matching inputs.
3. Build a request context with `TenantId`, `UserId`, timestamp, and correlation id.
4. Resolve tenant provider capability for the required AI command type.
5. Publish the versioned event through `IPublishEndpoint`.
6. Return the correlation id and map the HTTP response to `202 Accepted`.

## BaseFAQ Domain Alignment

- AI is event-driven and stateless on the worker side.
- Tenant provider secrets do not belong in host configuration files.
- FAQ or public APIs initiate work; the AI worker consumes and calls back asynchronously.

## Collaborates With

- [`process-control-plane-work-items`](../../data/process-control-plane-work-items/SKILL.md)
- [`write-real-database-integration-tests`](../../data/write-real-database-integration-tests/SKILL.md)

## Done When

- The initiating handler returns quickly with a correlation id.
- Prerequisites are validated before publication.
- The published message carries explicit tenant-aware context.
