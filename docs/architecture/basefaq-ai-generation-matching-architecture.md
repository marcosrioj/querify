# BaseFAQ AI Generation and Matching Architecture

## Purpose

This document describes the AI runtime that currently exists in the repository, how generation and matching flow through RabbitMQ and MassTransit, and which parts are already implemented versus still incomplete.

## Current implementation summary

The repository already contains a real asynchronous AI split:

- `BaseFaq.Faq.Portal.Api` publishes generation requests.
- `BaseFaq.Faq.Public.Api` publishes matching requests when a new FAQ item is created through the public flow.
- `BaseFaq.AI.Api` hosts the consumers and provider-facing execution services.
- Generation and matching both publish completion or failure callbacks after worker execution.

The important constraint is that the AI worker is intentionally stateless for lifecycle tracking. It resolves tenant-specific provider configuration from the tenant database, uses a tenant-specific FAQ database when needed, and publishes events instead of owning a long-lived job store.

## Project map

| Project area | Role |
|---|---|
| `BaseFaq.AI.Api` | worker host and health endpoint |
| `BaseFaq.AI.Business.Common` | shared AI abstractions such as provider resolution and FAQ DbContext factory |
| `BaseFaq.AI.Business.Generation` | content study, prompt building, generation provider integration, generation consumers |
| `BaseFaq.AI.Business.Matching` | candidate loading, ranking provider integration, matching consumers |
| `BaseFaq.Models.Ai` | the contract package currently referenced by the active business code for generation and matching messages |
| `BaseFaq.AI.Common.Contracts` | parallel contracts project that exists in the repository, but is not currently included in `BaseFaq.sln` and is not the primary message-contract source for the active runtime |
| `BaseFaq.AI.Common.VectorStore` | scaffold project reserved for future vector-store integrations; it exists in the repository but is not currently included in `BaseFaq.sln` |
| `BaseFaq.Sample.Ai.Generation` | standalone console sample for prompt and provider experimentation outside the worker runtime |
| `BaseFaq.AI.Test.IntegrationTest` | integration coverage for AI flows |

## Event-driven design

### Generation flow

1. `BaseFaq.Faq.Portal.Business.Faq` validates the FAQ and its content references.
2. It checks whether the tenant has a provider configured for `AiCommandType.Generation`.
3. It publishes `FaqGenerationRequestedV1` through `IPublishEndpoint`.
4. `BaseFaq.AI.Business.Generation.Consumers.FaqGenerationRequestedConsumer` receives the message.
5. `ProcessFaqGenerationRequestedCommandHandler` executes the generation service.
6. The worker publishes either `FaqGenerationReadyV1` or `FaqGenerationFailedV1`.
7. `BaseFaq.Faq.Portal.Api` currently consumes those callbacks and logs them.

### Matching flow

1. `BaseFaq.Faq.Public.Business.FaqItem` creates the FAQ item.
2. It checks whether the tenant has a provider configured for `AiCommandType.Matching`.
3. It publishes `FaqMatchingRequestedV1`.
4. `BaseFaq.AI.Business.Matching.Consumers.FaqMatchingRequestedConsumer` receives the message.
5. `ProcessFaqMatchingRequestedCommandHandler` loads the source question and candidate questions from the tenant FAQ database.
6. The worker publishes either `FaqMatchingCompletedV1` or `FaqMatchingFailedV1`.

## What the worker actually does today

### Generation

The generation worker currently:

- resolves the tenant's generation provider configuration
- loads processable content references from the tenant FAQ database
- builds prompt data
- calls the configured provider
- logs successful completion
- publishes ready or failed callback events

The generation worker does **not** currently persist generated FAQ items or generation lifecycle state back into the FAQ database.

### Matching

The matching worker currently:

- resolves the tenant's matching provider configuration
- loads the newly created FAQ item question
- loads other active FAQ item questions for the same tenant
- asks the provider to rank the candidates
- publishes matching-completed or matching-failed callback events

There is no repository-side consumer yet that stores or applies the matching result after `FaqMatchingCompletedV1` is published.

## Architectural decisions already visible in code

### Stateless AI worker host

The AI host uses:

- background messaging through MassTransit
- provider resolution from tenant configuration
- tenant-specific FAQ database access through `IFaqDbContextFactory`
- callback events instead of an internal job database

This keeps the AI runtime small and avoids mixing FAQ domain ownership into the worker host.

### Tenant-aware provider resolution

Provider credentials are not read from `appsettings.json`. Instead, the worker resolves them per tenant and per command type from tenant data. That makes AI behavior part of tenant configuration, not global host configuration.

### Shared infrastructure alignment

The AI host follows the same shared conventions as the rest of the solution where relevant:

- DI composition in the API host
- MediatR command handlers
- shared telemetry package
- shared contracts for inter-service messaging

## Operational requirements

### Required infrastructure

- RabbitMQ for message transport
- PostgreSQL tenant database for provider and tenant metadata
- PostgreSQL FAQ databases for tenant FAQ data access
- an AI provider account and credentials configured through tenant management

### Required configuration

- `Ai:UserId` in `dotnet/BaseFaq.AI.Api/appsettings.json`
- RabbitMQ configuration for generation and matching exchanges/queues
- tenant AI provider credentials stored through tenant flows

The credential handling model is documented in [`../operations/secret-manager-key-rotation.md`](../operations/secret-manager-key-rotation.md).

### Observability

`BaseFaq.AI.Api` is the one place in the solution already wired to the shared telemetry package. Jaeger and the local observability stack can therefore be used to inspect the AI worker independently of the CRUD APIs.

## Current gaps and implementation status

### Implemented

- asynchronous message publication for generation and matching
- AI worker consumers for both flows
- provider resolution per tenant and AI command type
- generation callback events
- matching callback events
- AI health endpoint
- AI integration test project

### Not yet implemented end-to-end

- persistence of generated FAQ items or draft artifacts from the generation worker
- FAQ-side job or lifecycle state tracking
- downstream consumer for matching-completed events
- operator-facing monitoring document beyond the local observability stack

## Practical guidance

- Treat the AI system as an asynchronous integration boundary, not as a synchronous helper method inside FAQ controllers.
- Do not move tenant provider secrets into host configuration files.
- If generation starts writing FAQ data in the future, document that new ownership model explicitly because it changes the current "stateless + callback only" behavior.
- When adding new AI event types, keep them versioned and explicit in the contracts projects.
- The current code does not yet justify claims about end-to-end idempotency tables, full DLQ validation, prompt governance, or AI-side FAQ writeback. Document those only when they exist in the repository.
