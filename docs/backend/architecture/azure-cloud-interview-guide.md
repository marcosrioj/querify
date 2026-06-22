# Querify On Azure - Interview Guide

## Purpose

This document is a simple interview study guide for explaining how Querify can run on Azure.
It connects the current repository architecture with current Azure services and gives short
talking points for C#/.NET, SaaS, cloud, CQRS, CI/CD, security, observability, and healthcare-style
regulated environments.

Last reviewed against Microsoft Azure documentation: 2026-06-22.

## The Short Story

Querify is a multi-tenant SaaS platform built with .NET 10. The backend is split into small API and
worker hosts, with business logic organized by module and feature. HTTP APIs, message consumers,
workers, and SignalR notifications all enter through thin adapters and then dispatch MediatR
commands or queries.

In Azure, the simplest modern target architecture is:

- Azure Static Web Apps for the React portal.
- Azure Front Door with WAF as the global public edge.
- Azure API Management as the API gateway for routing, JWT validation, throttling, policies, and
  API documentation.
- Azure Container Apps for Querify APIs and workers, because the solution already ships containerized
  services and has background/event-driven workloads.
- Azure Container Registry for container images.
- Azure Database for PostgreSQL Flexible Server for Tenant, QnA, Hangfire, Direct, Broadcast, and
  Trust persistence.
- Azure Service Bus for managed publish/subscribe messaging. The code currently uses RabbitMQ via
  MassTransit, so this is the recommended Azure PaaS target, not the current implementation.
- Azure Blob Storage for source files, replacing the current S3-compatible storage implementation
  behind the existing `IObjectStorage` abstraction.
- Azure Managed Redis for distributed allowed-tenant cache.
- Azure SignalR Service when multiple QnA Portal API instances need real-time notifications.
- Azure Key Vault and Azure App Configuration for secrets and environment settings.
- Azure Monitor, Application Insights, and Log Analytics for traces, logs, metrics, dashboards, and
  alerts.
- GitHub Actions with OpenID Connect to build, test, push images, run migrations, and deploy without
  long-lived Azure secrets.

## Current Querify Runtime Catalog

| Runtime | Responsibility | Azure deployment |
|---|---|---|
| `apps/portal` | Authenticated tenant portal UI | Azure Static Web Apps, optionally behind Azure Front Door |
| `Querify.Tenant.BackOffice.Api` | Global tenant, user, billing, and metadata administration | Azure Container App, private behind API Management |
| `Querify.Tenant.Portal.Api` | Tenant workspace and tenant-member operations | Azure Container App, private behind API Management |
| `Querify.Tenant.Public.Api` | Public tenant ingress such as Stripe webhooks | Azure Container App, public route through Front Door/API Management |
| `Querify.QnA.Portal.Api` | Authenticated QnA management, source upload flow, SignalR notifications | Azure Container App, private behind API Management; Azure SignalR for scale |
| `Querify.QnA.Public.Api` | Public QnA access, votes, feedback, public client-key flows | Azure Container App, public route through Front Door/API Management |
| `Querify.Tenant.Worker.Api` | Billing and email background processing | Azure Container App with no public ingress |
| `Querify.QnA.Worker.Api` | Source upload verification, source generation execution, Hangfire-backed jobs | Azure Container App with no public ingress; scale by queue depth |
| `Querify.Mcp.Server` | MCP integration surface | Azure Container App if enabled for production |
| `Querify.Tools.Migration` | Tenant-aware EF migration runner | CI/CD job or one-off Container Apps job |
| `Querify.Tools.Seed` | Environment seed tool | CI/CD job or one-off Container Apps job |

The repository currently has Dockerfiles for the API and worker hosts and Docker Compose files for
local dependencies. It does not currently contain production Azure IaC or GitHub Actions workflows,
so the Azure deployment flow below is the recommended cloud design.

## Azure Topology

```text
Users / Portal / Integrations
        |
        v
Azure Front Door + WAF + custom domains + TLS
        |
        +--> Azure Static Web Apps
        |
        v
Azure API Management
        |
        v
Azure Container Apps Environment
  - Tenant BackOffice API
  - Tenant Portal API
  - Tenant Public API
  - QnA Portal API
  - QnA Public API
  - Tenant Worker
  - QnA Worker
  - MCP Server, if enabled
        |
        +--> Azure Database for PostgreSQL Flexible Server
        +--> Azure Managed Redis
        +--> Azure Service Bus
        +--> Azure Blob Storage
        +--> Azure SignalR Service
        +--> Key Vault / App Configuration
        +--> Azure Monitor / Application Insights / Log Analytics
```

## Service Mapping

| Current local dependency | Current role | Azure service | Notes |
|---|---|---|---|
| PostgreSQL 16 | EF Core persistence | Azure Database for PostgreSQL Flexible Server | Use private networking, automated backups, HA for production, and PgBouncer when connection pressure grows. |
| RabbitMQ | Source upload events via MassTransit fanout exchanges | Azure Service Bus topics/subscriptions | Keeps pub/sub, DLQ, retry, and durable messaging managed by Azure. Code change is needed because the repo currently configures RabbitMQ transport. |
| MinIO / S3 API | Source file staging, verified files, signed upload/download URLs | Azure Blob Storage | Add a Blob implementation of `IObjectStorage`; use SAS URLs instead of S3 pre-signed URLs. |
| Redis | Distributed allowed-tenant cache | Azure Managed Redis | Keep `IDistributedCache` and configure TLS/password/identity as appropriate. |
| smtp4dev | Local email testing | Azure Communication Services Email or an approved SMTP provider | Tenant worker already isolates email processing behind worker behavior. |
| Jaeger / Prometheus / Grafana | Local observability | Azure Monitor, Application Insights, Log Analytics, dashboards, alerts | Existing OpenTelemetry OTLP wiring makes this a natural fit. |
| Sentry | Error reporting | Keep Sentry, or consolidate into Application Insights | The code already has Sentry integration. In Azure, Application Insights can own most operational telemetry. |
| Docker images | Local service packaging | Azure Container Registry | GitHub Actions builds images and pushes immutable tags. |

## Why Container Apps

Azure App Service is a strong option for simple web APIs. For Querify, Azure Container Apps is the
better interview answer because:

- the repo already has separate containerized APIs and workers;
- Container Apps supports API endpoints, background jobs, event-driven processing, and microservices;
- it can scale on HTTP traffic, CPU/memory, or KEDA-supported event sources;
- it supports revisions and traffic splitting for blue/green or canary releases;
- workers can run without public ingress in the same environment as the APIs.

AKS would only be justified if the team needs Kubernetes-level control. For this solution, Container
Apps gives most of the operational value with less platform overhead.

## Data And Multitenancy

Querify has a clear control-plane/data-plane split:

- `TenantDbContext` is the control plane. It stores tenants, users, memberships, billing state,
  entitlements, module connection strings, client keys, and operational state such as billing webhook
  inboxes and email outbox records.
- `QnADbContext` stores tenant-scoped QnA data: spaces, questions, answers, sources, tags, activity,
  source upload state, and source generation runs.
- `HangfireQnaDbContext` stores QnA worker Hangfire storage. It is operational state, not QnA domain
  state.
- `DirectDbContext`, `BroadcastDbContext`, and `TrustDbContext` are module persistence boundaries for
  staged product areas. `TrustDbContext` exists today but has no entities yet.

In Azure, this maps well to PostgreSQL Flexible Server:

- one shared PostgreSQL server for lower-cost dev/test;
- separate databases or schemas for `TenantDb`, `QnA`, `HangfireQnaDb`, and future modules;
- tenant-specific module connection strings stored in the tenant control plane;
- private endpoint/VNet integration for production;
- zone-redundant HA and backups for production;
- the migration tool runs after deployment to apply tenant-aware module migrations.

Interview phrase:

> The important part is that tenant metadata lives in a control-plane database, while module data is
> behind module-owned DbContexts. That lets us scale, migrate, and isolate tenant module data without
> putting every workflow into one shared database model.

## CQRS And Clean Application Flow

Querify applies CQRS through MediatR:

```text
Controller -> Service -> Command/Query
Consumer -> ConsumerService -> Command/Query
HostedService -> ProcessorService -> Command/Query
Background job -> Service -> Command/Query
Event -> NotificationService -> Command/Query
```

The key standards are:

- Controllers are thin. They handle HTTP concerns and delegate.
- Services coordinate telemetry and dispatch one command/query.
- Commands own writes, validation, persistence, domain workflow, and event publication.
- Commands return simple values such as `Guid`, `bool`, `string`, or `void`.
- Commands do not return DTOs or paged results.
- Queries own read DTO shaping.
- Query handlers use `AsNoTracking()` by default and project directly to DTOs.
- Consumers and hosted services are adapters only.
- Feature behavior lives in feature-scoped projects such as
  `Querify.QnA.Portal.Business.Source`, not in monolithic catch-all modules.

How this connects to Azure:

- HTTP requests enter through Front Door/API Management/Container Apps, but the business logic still
  enters through the same controller-service-command/query pattern.
- Service Bus messages enter through consumers, but consumers still delegate to consumer services and
  MediatR.
- Worker jobs use the same commands/queries, so request-time and background-time behavior stay
  consistent.
- OpenTelemetry spans are started in services and exported to Azure Monitor/Application Insights.

Interview phrase:

> CQRS is useful here because write workflows and read projections have different needs. A source
> upload write returns an id or accepted correlation, while the read side can optimize the exact DTO
> shape for the portal without forcing command handlers to build read models.

## Source Upload Flow

Current flow:

1. Portal asks `QnA Portal API` to create an upload intent.
2. API stores pending source metadata and returns a signed upload URL.
3. Browser uploads the file directly to object storage.
4. Portal completes the upload.
5. API publishes `SourceUploadCompletedIntegrationEvent`.
6. QnA Worker consumes the event and verifies the uploaded object.
7. Worker moves the object to verified or quarantined storage state.
8. Worker publishes `SourceUploadStatusChangedIntegrationEvent`.
9. QnA Portal API receives the status event and pushes SignalR notification to the portal.

Azure version:

- Blob Storage private container for source artifacts.
- SAS URLs for upload/download.
- Service Bus topic for source upload events.
- QnA Worker Container App consumes messages.
- Azure SignalR Service scales portal notifications.
- Application Insights traces the whole flow using propagated correlation ids.

## Messaging Design

The repo currently uses MassTransit with RabbitMQ:

- fanout exchange for upload-completed events;
- fanout exchange for upload-status-changed events;
- queues for worker and portal status consumers;
- retry/prefetch/concurrency options in configuration.

Azure recommendation:

- Replace RabbitMQ transport with MassTransit Azure Service Bus transport.
- Use topics for integration events.
- Use subscriptions for each consumer group.
- Use dead-letter queues for poison messages.
- Scale QnA Worker with KEDA based on Service Bus queue/subscription depth.

Interview phrase:

> RabbitMQ is the current local transport. In Azure, I would choose Service Bus for a managed PaaS
> broker because it gives durable queues, topics/subscriptions, DLQ, retries, and operational
> visibility without owning broker infrastructure.

## Storage Design

The current storage abstraction is `IObjectStorage`, implemented with AWS S3 SDK so local MinIO and
S3-compatible providers work.

For Azure, do not leak Azure Blob code into feature handlers. Add a new infrastructure implementation:

- `BlobObjectStorage : IObjectStorage`;
- use private containers;
- create SAS URLs for direct browser upload/download;
- keep the same source upload domain flow;
- keep object keys such as staging, verified, and quarantined paths;
- store only object references in PostgreSQL.

Interview phrase:

> The clean part is that feature code depends on `IObjectStorage`, not a specific cloud SDK. That
> makes the Azure migration a storage adapter change instead of a business workflow rewrite.

## Identity, Security, And Network

Current repo:

- Authenticated portal/back-office APIs use JWT authentication.
- Tenant-scoped portal APIs use `X-Tenant-Id`.
- Public QnA APIs use `X-Client-Key`.
- Tenant public billing webhooks are public ingress but validate provider signatures.
- API failures are normalized through shared API error handling middleware.

Azure target:

- Keep Auth0 if it is the chosen identity provider, or integrate Microsoft Entra ID / Entra External
  ID if the product wants Microsoft-native identity.
- API Management validates JWTs and applies throttling/rate limits.
- Front Door WAF protects public endpoints at the edge.
- Container Apps use managed identities to read Key Vault secrets and access Azure resources.
- Key Vault stores database passwords, Redis credentials, broker credentials, Stripe secrets, Auth0
  settings, Sentry DSN, storage keys, and certificate material when needed.
- App Configuration stores non-secret environment settings and feature flags.
- PostgreSQL, Redis, Service Bus, Storage, and Key Vault should use private endpoints or restricted
  network access in production.
- Production Swagger should stay disabled or restricted, matching the current pattern where Swagger
  is only enabled outside production.

Healthcare/regulatory interview angle:

- If PHI is involved, use Azure Health Data Services FHIR service for FHIR APIs and PHI-focused
  workflows rather than inventing a custom FHIR store.
- Keep audit logs, tenant isolation, RBAC, encryption at rest/in transit, data retention, and least
  privilege as explicit design requirements.
- Querify is not currently a FHIR implementation, but its module boundaries, audit conventions, and
  tenant isolation patterns are compatible with regulated SaaS thinking.

## Observability And Debugging

The repo already has shared OpenTelemetry registration:

- ASP.NET Core instrumentation;
- HTTP client instrumentation;
- EF Core instrumentation;
- MassTransit activity source;
- custom feature activity sources;
- OTLP exporter endpoint configuration.

Azure target:

- Application Insights receives traces, dependencies, exceptions, and request telemetry.
- Log Analytics stores Container Apps logs, API Management logs, Front Door logs, PostgreSQL logs,
  Service Bus metrics, and Key Vault audit logs.
- Use cloud role names per service, for example `querify-qna-portal-api` and
  `querify-qna-worker-api`, so the Application Map is readable.
- Alert on API error rate, latency, queue age, DLQ count, worker failures, PostgreSQL CPU/storage,
  Redis memory, and storage upload failures.
- Keep Sentry if the team wants developer-focused error triage in addition to Azure Monitor.

Interview phrase:

> When debugging a distributed flow, I want one correlation id from the HTTP request through the
> message, worker, database call, storage call, and SignalR notification. The code already uses
> OpenTelemetry, so Azure Monitor/Application Insights is the natural Azure landing place.

## CI/CD And Git Flow

Recommended pipeline:

1. Pull request opens.
2. GitHub Actions restores dependencies.
3. Run `dotnet build Querify.sln`.
4. Run relevant .NET integration tests.
5. Build portal assets.
6. Build Docker images for changed API/worker hosts.
7. Push images to Azure Container Registry with immutable tags.
8. Deploy infrastructure with Bicep or Terraform.
9. Deploy Container Apps revisions.
10. Run tenant-aware EF migrations using `Querify.Tools.Migration`.
11. Run smoke tests against API Management endpoints.
12. Shift traffic to the new revision gradually.
13. Roll back by moving traffic back to the previous revision if smoke tests or alerts fail.

Use GitHub Actions OpenID Connect with Microsoft Entra workload identity federation. That avoids
long-lived Azure client secrets in GitHub.

Infrastructure should be versioned:

- resource groups;
- Container Apps environment and apps;
- ACR;
- PostgreSQL Flexible Server;
- Redis;
- Service Bus;
- Storage Account;
- Key Vault;
- App Configuration;
- Application Insights and Log Analytics;
- Front Door and WAF;
- API Management;
- Azure SignalR;
- private DNS/private endpoints if used.

Interview phrase:

> I would not click-deploy this manually. I would use IaC plus GitHub Actions OIDC, build immutable
> images into ACR, deploy Container Apps revisions, run migrations as a controlled job, and use
> traffic splitting for safer releases.

## Environment Strategy

| Environment | Purpose | Notes |
|---|---|---|
| Dev | Fast validation | Lower-cost PostgreSQL/Redis tiers, smaller Container Apps scale, public access can be looser if no real data. |
| Test/Staging | Production-like validation | Same topology as production, test data, migration dry runs, smoke tests. |
| Production | Customer traffic | Private networking, HA, backups, alerts, WAF, APIM policies, locked-down secrets, restricted diagnostics. |

For Canada-focused work eligibility or healthcare conversations, mention Azure regions explicitly
when relevant, for example Canada Central / Canada East depending on residency and latency
requirements.

## Best Standards Applied In Querify

| Standard | How Querify applies it |
|---|---|
| SOLID | Feature projects are small and handlers own one use case. Shared infrastructure is abstracted behind interfaces and extension methods. |
| CQRS | Commands and queries are separated through MediatR. Writes return simple values; reads shape DTOs. |
| DDD-style boundaries | Tenant, QnA, Direct, Broadcast, Trust, and MCP boundaries are separated by project and persistence responsibility. |
| Thin adapters | Controllers, consumers, hosted services, and Hangfire jobs delegate instead of owning business behavior. |
| Multitenancy | Tenant context is part of the request model through `X-Tenant-Id`, `X-Client-Key`, and tenant-aware connection resolution. |
| Persistence safety | EF contexts own tenant filters, soft delete, audit, UTC normalization, and tenant integrity rules. |
| Async processing | Source upload verification uses integration events and workers instead of blocking the HTTP request. |
| Observability | OpenTelemetry is centralized and feature services add spans. |
| Cloud readiness | APIs/workers are containerized; configuration is external; services are split for independent scale. |
| Testability | Integration test projects exist per major API/worker area and favor production-like infrastructure. |

## What To Say If Asked About Azure Services

**Azure Container Apps**

> I would use Container Apps for Querify because we have several .NET containers and background
> workers. APIs scale by HTTP traffic, workers scale by queue depth, and revisions give a clean
> release model.

**Azure App Service**

> App Service is great for simpler web APIs. For Querify, Container Apps fits better because workers,
> messaging, and microservice-style containers are first-class in the same platform.

**Azure API Management**

> API Management gives a stable API facade. It handles products, policies, JWT validation, throttling,
> versioning, developer docs, and observability without each API reimplementing those concerns.

**Azure Service Bus**

> Service Bus is where I would put integration events. It decouples APIs and workers, supports topics
> and subscriptions, and gives DLQs for operational recovery.

**Azure PostgreSQL Flexible Server**

> PostgreSQL stores the control plane and module data. Flexible Server gives managed patching,
> backups, HA options, private networking, and scaling without managing VMs.

**Key Vault**

> Secrets do not belong in code or pipeline variables when the app is running. Container Apps should
> use managed identity to read Key Vault references.

**Application Insights**

> Application Insights is the main place to debug distributed flows. I want to see the request, SQL
> query, broker publish/consume, storage call, and worker span together.

**FHIR / Healthcare**

> Querify is not currently a FHIR system, but if the domain required FHIR/HL7, I would integrate Azure
> Health Data Services FHIR service instead of building a custom FHIR API from scratch. Querify's
> tenant isolation, audit, and CQRS patterns are still relevant in regulated systems.

## Gaps Before A Real Azure Production Launch

- Add Azure IaC with Bicep or Terraform.
- Add GitHub Actions workflows.
- Add Azure Blob implementation of `IObjectStorage`.
- Decide between keeping RabbitMQ infrastructure or moving MassTransit to Azure Service Bus.
- Add Azure SignalR Service registration for multi-instance portal notifications.
- Add Key Vault/App Configuration provider usage.
- Define production APIM policies, Front Door WAF rules, DNS, and certificates.
- Define migration deployment order and rollback process.
- Add cloud smoke tests.
- Validate data residency, retention, audit, and compliance requirements before handling healthcare
  or other regulated data.

## References

- Azure Container Apps overview: https://learn.microsoft.com/en-us/azure/container-apps/overview
- Azure App Service overview: https://learn.microsoft.com/en-us/azure/app-service/overview
- Azure Static Web Apps overview: https://learn.microsoft.com/en-us/azure/static-web-apps/overview
- Azure Front Door overview: https://learn.microsoft.com/en-us/azure/frontdoor/front-door-overview
- Azure API Management concepts: https://learn.microsoft.com/en-us/azure/api-management/api-management-key-concepts
- Azure Container Registry overview: https://learn.microsoft.com/en-us/azure/container-registry/container-registry-intro
- Azure Database for PostgreSQL overview: https://learn.microsoft.com/en-us/azure/postgresql/overview
- Azure Service Bus overview: https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview
- Azure Blob Storage overview: https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blobs-overview
- Azure Managed Redis overview: https://learn.microsoft.com/en-us/azure/redis/overview
- Azure SignalR Service overview: https://learn.microsoft.com/en-us/azure/azure-signalr/signalr-overview
- Azure Key Vault overview: https://learn.microsoft.com/en-us/azure/key-vault/general/overview
- Azure App Configuration overview: https://learn.microsoft.com/en-us/azure/azure-app-configuration/overview
- Azure Monitor OpenTelemetry for .NET: https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-enable
- GitHub Actions with Azure OpenID Connect: https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure-openid-connect
- Microsoft Entra workload identity federation: https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation
- Azure Communication Services Email: https://learn.microsoft.com/en-us/azure/communication-services/concepts/email/email-overview
- Azure multitenant architecture guide: https://learn.microsoft.com/en-us/azure/architecture/guide/multitenant/overview
- Azure multitenant deployment/configuration guidance: https://learn.microsoft.com/en-us/azure/architecture/guide/multitenant/approaches/deployment-configuration
- Azure Health Data Services FHIR service: https://learn.microsoft.com/en-us/azure/healthcare-apis/fhir/overview
