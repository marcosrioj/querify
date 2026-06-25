# Querify On AWS - Interview Guide

## Purpose

This document is a simple interview study guide for explaining how Querify can run on AWS using AWS
technologies only. It connects the current repository architecture with current AWS services and
gives short talking points for C#/.NET, SaaS, cloud architecture, CQRS, CI/CD, security,
observability, and healthcare-style regulated environments.

Last reviewed against AWS documentation: 2026-06-24.

## The Short Story

Querify is a multi-tenant SaaS platform built with .NET 10. The backend is split into small API and
worker hosts, with business logic organized by module and feature. HTTP APIs, message consumers,
workers, Hangfire jobs, and SignalR notifications all enter through thin adapters and then dispatch
MediatR commands or queries.

In AWS, the best production target architecture is:

- Amazon Route 53 for DNS.
- AWS Certificate Manager for TLS certificates.
- Amazon CloudFront with AWS WAF as the global public edge.
- Amazon S3 with CloudFront origin access control for the React portal.
- Amazon API Gateway HTTP APIs for API facade behavior where API management, auth, throttling, and
  request policies are needed.
- Internal Application Load Balancers for ECS services behind API Gateway private integrations.
- Amazon ECS on AWS Fargate for Querify APIs and workers, because the solution already ships
  containerized .NET services.
- Amazon ECR for container images.
- Amazon Aurora PostgreSQL-Compatible for production persistence, with Amazon RDS for PostgreSQL as
  the simpler lower-cost option for dev/test or early production.
- Amazon RDS Proxy when connection pressure becomes a problem.
- Amazon S3 for source uploads and artifacts. This is the cleanest AWS mapping because Querify
  already uses the AWS S3 SDK behind `IObjectStorage`.
- Amazon SNS plus Amazon SQS for AWS-native publish/subscribe and worker queues. Amazon MQ for
  RabbitMQ is the fastest migration option if the team wants minimal code changes.
- Amazon ElastiCache Serverless for Valkey or Redis OSS for distributed allowed-tenant cache and a
  SignalR Redis backplane.
- Amazon Cognito for AWS-only user authentication and JWT issuing.
- Amazon SES for transactional email.
- AWS Secrets Manager, AWS Systems Manager Parameter Store, and AWS AppConfig for secrets,
  configuration, and feature flags.
- Amazon CloudWatch, AWS X-Ray, and AWS Distro for OpenTelemetry for logs, metrics, traces,
  dashboards, and alerts.
- AWS CDK with CloudFormation for infrastructure as code.
- AWS CodeCommit, CodePipeline, CodeBuild, CodeDeploy, CodeArtifact, and ECR for an AWS-only
  software delivery chain.

Important source-control note: Amazon CodeCatalyst is no longer open to new customers. For an
AWS-only Git story, use CodeCommit when it is available in the AWS account. If a new account or
company policy does not allow CodeCommit, the strict AWS-only fallback is self-hosted Git on AWS,
but that adds operational burden. In real companies, GitHub or GitLab with CodePipeline is common,
but it is not AWS-only.

## Current Querify Runtime Catalog

| Runtime | Responsibility | AWS deployment |
|---|---|---|
| `apps/portal` | Authenticated tenant portal UI | S3 static site bucket behind CloudFront and WAF |
| `Querify.Tenant.BackOffice.Api` | Global tenant, user, billing, and metadata administration | ECS Fargate service behind private ALB/API Gateway |
| `Querify.Tenant.Portal.Api` | Tenant workspace and tenant-member operations | ECS Fargate service behind private ALB/API Gateway |
| `Querify.Tenant.Public.Api` | Public tenant ingress such as Stripe webhooks | ECS Fargate service exposed through CloudFront/WAF/API Gateway |
| `Querify.QnA.Portal.Api` | Authenticated QnA management, source upload flow, SignalR notifications | ECS Fargate service; API routes through API Gateway/ALB; SignalR hub through CloudFront/ALB |
| `Querify.QnA.Public.Api` | Public QnA access, votes, feedback, public client-key flows | ECS Fargate service exposed through CloudFront/WAF/API Gateway |
| `Querify.Tenant.Worker.Api` | Billing and email background processing | ECS Fargate service with no public ingress |
| `Querify.QnA.Worker.Api` | Source upload verification, source generation execution, Hangfire-backed jobs | ECS Fargate service with no public ingress; scale from SQS depth and CPU/memory |
| `Querify.Mcp.Server` | MCP integration surface | ECS Fargate service if enabled for production |
| `Querify.Tools.Migration` | Tenant-aware EF migration runner | CodeBuild job, ECS one-off task, or CodePipeline deployment action |
| `Querify.Tools.Seed` | Environment seed tool | CodeBuild job or ECS one-off task |

The repository currently has Dockerfiles for the API and worker hosts and Docker Compose files for
local dependencies. It does not currently contain production AWS IaC or AWS CI/CD workflows, so the
AWS deployment flow below is the recommended cloud design.

## AWS Topology

```text
Users / Portal / Integrations
        |
        v
Route 53 + ACM
        |
        v
CloudFront + AWS WAF
        |
        +--> S3 portal bucket
        |
        +--> API Gateway HTTP APIs
        |       |
        |       v
        |   VPC Link
        |       |
        |       v
        |   Internal ALB -> ECS Fargate API services
        |
        +--> CloudFront route to ALB for ASP.NET Core SignalR hub

Private VPC subnets
  - Tenant BackOffice API tasks
  - Tenant Portal API tasks
  - Tenant Public API tasks
  - QnA Portal API tasks
  - QnA Public API tasks
  - Tenant Worker tasks
  - QnA Worker tasks
  - MCP Server tasks, if enabled
        |
        +--> Aurora PostgreSQL / RDS PostgreSQL
        +--> RDS Proxy, when needed
        +--> ElastiCache Serverless for Valkey or Redis OSS
        +--> SNS topics + SQS queues, or Amazon MQ for RabbitMQ
        +--> S3 source artifact buckets
        +--> SES
        +--> Cognito
        +--> Secrets Manager / Parameter Store / AppConfig
        +--> CloudWatch / X-Ray / ADOT
```

## Service Mapping

| Current local dependency | Current role | AWS service | Notes |
|---|---|---|---|
| PostgreSQL 16 | EF Core persistence | Aurora PostgreSQL-Compatible or RDS for PostgreSQL | Aurora is the stronger production answer; RDS PostgreSQL is simpler and cheaper. Use private subnets, backups, Multi-AZ, and Performance Insights. |
| RabbitMQ | Source upload events via MassTransit fanout exchanges | SNS + SQS, or Amazon MQ for RabbitMQ | SNS+SQS is the best AWS-native design. Amazon MQ is the lowest-code-change migration. |
| MinIO / S3 API | Source file staging, verified files, signed upload/download URLs | Amazon S3 | Existing AWS S3 SDK abstraction maps directly. Prefer IAM task roles instead of static access keys. |
| Redis | Distributed allowed-tenant cache | Amazon ElastiCache Serverless for Valkey or Redis OSS | Keep `IDistributedCache`; also use Redis backplane for SignalR scale-out. |
| smtp4dev | Local email testing | Amazon SES | Tenant worker already isolates email behavior. |
| Jaeger / Prometheus / Grafana | Local observability | CloudWatch, X-Ray, ADOT, Amazon Managed Prometheus/Grafana if needed | Existing OpenTelemetry OTLP wiring makes ADOT/CloudWatch/X-Ray natural. |
| Sentry | Error reporting | Keep Sentry only if allowed, otherwise CloudWatch/X-Ray | For AWS-only, CloudWatch and X-Ray become the default operational tools. |
| Docker images | Local service packaging | Amazon ECR | CodeBuild builds and pushes immutable image tags. |
| Local Docker Compose | Local platform stack | ECS/Fargate plus managed AWS services | Compose remains local only; production infrastructure should be CDK/CloudFormation. |

## Why ECS Fargate

ECS Fargate is the best default for this repository because:

- Querify already has several containerized .NET API and worker hosts.
- Fargate removes EC2 cluster capacity management.
- APIs and workers can run in private subnets with IAM task roles.
- ECS services can scale by CPU, memory, request load, or SQS queue depth through CloudWatch alarms.
- ECS blue/green deployments with CodeDeploy support safer canary or linear traffic shifting.
- Long-running workers, Hangfire, MassTransit consumers, and SignalR fit better in containers than in
  Lambda functions.

When not to choose it:

- Choose AWS Lambda only for small event handlers or short jobs, not the current long-running API and
  worker model.
- Choose AWS App Runner for a smaller product with fewer private networking and worker requirements.
- Choose Amazon EKS only if the team already needs Kubernetes-level control, custom operators, or a
  multi-tenant platform team. For Querify, EKS adds unnecessary operational weight.

Interview phrase:

> I would start with ECS Fargate, not EKS. The repo already packages independent APIs and workers as
> containers, and Fargate gives me private networking, autoscaling, task roles, blue/green deploys,
> and no EC2 node management.

## Scaling And Scheduling

API services:

- Run each API as its own ECS service so public, portal, back-office, and QnA workloads can scale
  independently.
- Scale API tasks from ALB request count, CPU, memory, latency, and custom application metrics.
- Keep task definitions immutable: image tag, environment, secrets, CPU/memory, and IAM role are
  explicit deployment artifacts.
- Use ECS task roles so each API can access only the AWS services it needs.

Worker services:

- Run tenant and QnA workers as separate ECS services with no public ingress.
- Scale QnA Worker from SQS queue depth, age of oldest message, CPU, and memory.
- Keep worker handlers idempotent because managed queue systems use at-least-once delivery.
- Keep Hangfire storage in PostgreSQL when the job requires durable retries, state, and dashboard
  visibility.

Scheduled work:

- Keep application-level recurring work in Hangfire when it is part of the domain workflow.
- Use EventBridge Scheduler for cloud-operational schedules, such as starting one-off ECS tasks,
  running maintenance commands, or triggering pipeline-controlled jobs.
- Do not let every application task run migrations on startup. Run migrations as a controlled
  CodeBuild job or ECS one-off task before traffic shifts.

Interview phrase:

> I separate API scale from worker scale. HTTP traffic scales from ALB and ECS metrics, while async
> source processing scales from SQS backlog and worker health. Scheduled operational tasks belong in
> EventBridge Scheduler, while domain jobs can stay in Hangfire if they need durable application
> state.

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

AWS production recommendation:

- Use Aurora PostgreSQL-Compatible for production when the priority is managed high availability,
  storage scaling, performance, and failover.
- Use RDS for PostgreSQL for lower-cost dev/test or a simpler early production architecture.
- Keep databases private in VPC subnets.
- Use security groups so only ECS tasks and migration jobs can reach the database.
- Use Secrets Manager for database credentials.
- Use KMS-managed encryption at rest.
- Use automated backups, point-in-time recovery, and AWS Backup where governance requires central
  backup policies.
- Use RDS Proxy if ECS task count or worker concurrency creates too many database connections.
- Keep the tenant-aware migration runner as a controlled deployment step, not something every API
  task runs on startup.

Interview phrase:

> The control plane is `TenantDbContext`; module data is behind module-owned DbContexts. On AWS I
> would run that on Aurora PostgreSQL or RDS PostgreSQL, keep it private, and run tenant-aware
> migrations from the pipeline or a one-off ECS task.

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

How this connects to AWS:

- HTTP requests enter through CloudFront, WAF, API Gateway, ALB, and ECS, but business logic still
  enters through the controller-service-command/query pattern.
- SNS/SQS or Amazon MQ messages enter through consumers, but consumers still delegate to consumer
  services and MediatR.
- Worker tasks use the same commands/queries as request-time flows, so behavior stays consistent.
- OpenTelemetry spans can be exported through ADOT into CloudWatch and X-Ray.

Interview phrase:

> CQRS is useful here because writes and reads have different needs. A source upload command returns
> an id or correlation, while the read side can optimize the exact DTO shape for the portal without
> making command handlers build read models.

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

AWS version:

- S3 private bucket for source artifacts.
- S3 pre-signed URLs for direct browser upload/download.
- SSE-KMS encryption for source objects.
- S3 lifecycle rules for failed staging uploads and retention policies.
- SNS topic for `SourceUploadCompletedIntegrationEvent`.
- SQS queue subscribed for the QnA Worker consumer.
- QnA Worker ECS task verifies the object and updates QnA state in PostgreSQL.
- SNS topic for `SourceUploadStatusChangedIntegrationEvent`.
- SQS queue subscribed for QnA Portal notification consumer.
- ASP.NET Core SignalR hub sends the portal notification through ECS/ALB.
- ElastiCache Redis backplane keeps SignalR notifications working across multiple QnA Portal API
  tasks.
- Macie and GuardDuty Malware Protection for S3 can be evaluated for sensitive-data discovery and
  uploaded-object risk monitoring.

## Messaging Design

The repo currently uses MassTransit with RabbitMQ:

- fanout exchange for upload-completed events;
- fanout exchange for upload-status-changed events;
- queues for worker and portal status consumers;
- retry/prefetch/concurrency options in configuration.

AWS-native target:

- Use SNS topics for publish/subscribe event fanout.
- Use SQS queues for each consumer group.
- Use SQS dead-letter queues for poison messages.
- Encrypt topics and queues with KMS.
- Scale QnA Worker ECS tasks from SQS queue depth, age of oldest message, CPU, and memory.
- Keep idempotent command handlers because SQS standard queues use at-least-once delivery.
- Use FIFO queues only when ordering is a business requirement.

Lowest-code-change target:

- Use Amazon MQ for RabbitMQ.
- Keep MassTransit RabbitMQ transport and configure the broker endpoint.
- Use private broker endpoints in the VPC.
- Accept that you still operate broker capacity, broker upgrades, and RabbitMQ topology more directly
  than with SNS/SQS.

Interview phrase:

> For AWS-native architecture I would move the fanout events to SNS topics and SQS queues. If the
> project needed a faster lift-and-shift, Amazon MQ for RabbitMQ would preserve the current
> MassTransit setup with fewer code changes.

## Storage Design

Querify already has an AWS-friendly storage design:

- `Querify.Common.Infrastructure.Storage` owns `IObjectStorage`.
- `S3ObjectStorage` uses `AWSSDK.S3`.
- Local MinIO exists because it speaks an S3-compatible API.
- Production can use real Amazon S3.

AWS hardening:

- Use a private S3 bucket, not public object access.
- Use CloudFront only for portal static files, not private source artifacts by default.
- Use pre-signed S3 URLs for source upload/download.
- Use SSE-KMS for encryption at rest.
- Use bucket policies to require TLS and KMS.
- Use lifecycle rules to expire stale `staging/` objects.
- Use object prefixes such as `staging/`, `verified/`, and `quarantined/`.
- Use IAM task roles instead of static `AccessKey` and `SecretKey` configuration.
- Keep only object keys and metadata in PostgreSQL.

Required code improvement for production AWS:

The current S3 registration creates `BasicAWSCredentials` from configuration. In production AWS, the
better pattern is to use the ECS task role and the AWS SDK default credential chain. That removes
long-lived S3 access keys from application configuration.

Interview phrase:

> AWS is actually simpler for storage because the code already uses the S3 SDK behind
> `IObjectStorage`. The production change I would make is to stop using static access keys and rely
> on ECS task roles.

## SignalR On AWS

AWS does not have a direct managed equivalent to Azure SignalR Service for ASP.NET Core SignalR.
The practical AWS design is:

- Run the QnA Portal API SignalR hub in ECS Fargate.
- Put it behind an Application Load Balancer that supports long-lived WebSocket traffic.
- Put CloudFront and WAF in front of the ALB if the hub is public.
- Use ElastiCache for Redis OSS or Valkey as the SignalR backplane when multiple QnA Portal API
  tasks are running.
- Do not force ASP.NET Core SignalR through API Gateway unless the specific integration is tested for
  WebSocket behavior. If the team wants API Gateway WebSocket APIs, that is a rewrite of the
  notification transport rather than a drop-in SignalR hosting change.

Interview phrase:

> On AWS I would host SignalR in ECS behind ALB and use ElastiCache as the backplane. There is no
> first-party managed ASP.NET SignalR service equivalent, so I would be careful not to promise a
> drop-in managed replacement.

## Identity, Security, And Network

Current repo:

- Authenticated portal/back-office APIs use JWT authentication.
- Tenant-scoped portal APIs use `X-Tenant-Id`.
- Public QnA APIs use `X-Client-Key`.
- Tenant public billing webhooks are public ingress but validate provider signatures.
- API failures are normalized through shared API error handling middleware.

AWS-only target:

- Use Amazon Cognito user pools as the JWT issuer for portal and back-office users.
- API Gateway can validate JWTs for API routes where appropriate.
- Keep tenant authorization in Querify because `X-Tenant-Id`, tenant memberships, and module
  connection routing are business rules.
- Use IAM task roles for ECS services instead of static AWS keys.
- Use Secrets Manager for database passwords, Stripe secrets, JWT settings, broker credentials, and
  other secrets.
- Use Parameter Store for stable non-secret configuration.
- Use AWS AppConfig for feature flags, runtime throttles, allow/block lists, and operational levers.
- Use KMS keys for S3, SNS/SQS, RDS/Aurora, Secrets Manager, and log encryption where required.
- Use private subnets for ECS tasks, databases, caches, brokers, and internal load balancers.
- Use VPC endpoints for S3, ECR, CloudWatch Logs, Secrets Manager, SQS, SNS, and other AWS APIs that
  private workloads call.
- Use CloudTrail for account/API audit.
- Use GuardDuty for threat detection, including ECS/Fargate and S3 protection plans where appropriate.
- Use Security Hub CSPM to aggregate findings and check standards.
- Use Macie for sensitive data discovery in S3 if source artifacts may contain PII or PHI.
- Keep production Swagger disabled or internal-only, matching the current pattern where Swagger is
  enabled only outside production.

Healthcare/regulatory interview angle:

- If FHIR/HL7 is required, use AWS HealthLake for a HIPAA-eligible FHIR R4 data store instead of
  building a custom FHIR server from scratch.
- Use AWS Organizations and separate accounts for dev, test, prod, security, logging, and shared
  services.
- Sign a BAA with AWS before handling PHI.
- Confirm every AWS service that processes PHI is in scope for the required compliance program.
- Keep audit logs, tenant isolation, RBAC, encryption, retention, backups, and incident response as
  explicit architecture requirements.
- Querify is not currently a FHIR implementation, but its tenant isolation, audit conventions, and
  CQRS boundaries are compatible with regulated SaaS thinking.

## Observability And Debugging

The repo already has shared OpenTelemetry registration:

- ASP.NET Core instrumentation;
- HTTP client instrumentation;
- EF Core instrumentation;
- MassTransit activity source;
- custom feature activity sources;
- OTLP exporter endpoint configuration.

AWS target:

- ECS task logs go to CloudWatch Logs.
- Application metrics and alarms go to CloudWatch.
- Distributed traces go to X-Ray through ADOT or the CloudWatch OpenTelemetry collector path.
- CloudWatch dashboards show API latency, error rate, SQS queue depth, DLQ count, worker failures,
  Aurora CPU/storage/connections, Redis memory, S3 upload failures, and ALB target health.
- CloudWatch Synthetics can run external smoke tests against portal/API routes.
- CloudTrail records management and data events where audit requires it.
- GuardDuty, Security Hub, and Macie findings feed EventBridge/SNS alerts.
- Keep Sentry only if the company allows non-AWS tooling; otherwise use CloudWatch/X-Ray as the
  AWS-only operational base.

Interview phrase:

> For a source upload issue, I want one trace from the HTTP request, through the SNS/SQS message, the
> worker, the S3 object operation, EF Core database writes, and the SignalR notification. The code
> already has OpenTelemetry, so AWS Distro for OpenTelemetry plus CloudWatch and X-Ray is the clean
> landing zone.

## CI/CD And Git Flow

Strict AWS-only delivery chain:

1. Source code lives in CodeCommit when available for the account.
2. Pull request opens in CodeCommit.
3. CodePipeline starts from the repository change.
4. CodeBuild restores NuGet/npm dependencies, optionally through CodeArtifact.
5. CodeBuild runs `dotnet build Querify.sln`.
6. CodeBuild runs relevant .NET integration tests.
7. CodeBuild builds portal assets.
8. CodeBuild builds Docker images for changed API/worker hosts.
9. CodeBuild pushes immutable image tags to ECR.
10. CDK synthesizes CloudFormation templates.
11. CloudFormation deploys or updates infrastructure.
12. CodeDeploy performs blue/green or canary ECS deployments.
13. `Querify.Tools.Migration` runs as a controlled CodeBuild job or ECS one-off task.
14. Smoke tests run against CloudFront/API Gateway routes.
15. CloudWatch alarms can stop or roll back a deployment.

If CodeCommit is not available:

- Do not recommend CodeCatalyst for new adoption; AWS documentation states it is no longer open to
  new customers.
- If "AWS-only" is strict, self-host Git on AWS, for example Gitea or GitLab on ECS/EC2 with managed
  storage and backups. This is operationally heavier.
- If "best engineering outcome" matters more than AWS-only purity, use GitHub or GitLab as source
  and CodePipeline/CodeBuild/CodeDeploy for AWS deployment. That is common, but not AWS-only.

Infrastructure should be versioned with CDK/CloudFormation:

- AWS Organizations accounts and OUs;
- VPC, subnets, route tables, NAT, VPC endpoints;
- Route 53 hosted zones and records;
- ACM certificates;
- CloudFront distributions;
- WAF web ACLs;
- S3 portal and artifact buckets;
- API Gateway APIs and VPC links;
- ALBs, target groups, listeners;
- ECS clusters, services, task definitions, task roles;
- ECR repositories and policies;
- Aurora/RDS databases, subnet groups, parameter groups;
- RDS Proxy, if used;
- ElastiCache Serverless cache;
- SNS topics, SQS queues, DLQs;
- Amazon MQ broker, if used instead of SNS/SQS;
- Cognito user pools and app clients;
- SES identities and configuration sets;
- Secrets Manager secrets;
- Parameter Store parameters;
- AppConfig applications and environments;
- CloudWatch log groups, dashboards, alarms, synthetics;
- X-Ray sampling rules;
- CloudTrail, GuardDuty, Security Hub, Macie;
- backup plans and KMS keys.

Interview phrase:

> I would avoid click-deploying this. The AWS-native path is CDK into CloudFormation, CodePipeline
> orchestration, CodeBuild for build/test/image push, ECR for images, CodeDeploy for ECS blue/green,
> and a controlled migration job before traffic shift.

## Environment Strategy

| Environment | Purpose | Notes |
|---|---|---|
| Dev | Fast validation | Lower-cost RDS/PostgreSQL or small Aurora, smaller ECS task sizes, smaller cache, no real customer data. |
| Test/Staging | Production-like validation | Same topology as production, test data, migration dry runs, smoke tests, WAF/APIGW policies close to prod. |
| Production | Customer traffic | Multi-AZ database, private networking, backups, WAF, CloudTrail, GuardDuty, Security Hub, alarms, restricted diagnostics. |

For Canada-focused interviews, mention AWS Canada regions when relevant, especially Canada Central
(`ca-central-1`) and Canada West (`ca-west-1`) if data residency, latency, or procurement requires
Canadian regions. Always verify service availability per region before committing to an architecture.

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
| AWS least privilege | The target design uses task roles, security groups, private subnets, KMS, Secrets Manager, and VPC endpoints. |
| Testability | Integration test projects exist per major API/worker area and favor production-like infrastructure. |

## What To Say If Asked About AWS Services

**ECS Fargate**

> I would use ECS Fargate because Querify is already split into containerized APIs and workers. It
> gives me container isolation, private networking, task roles, autoscaling, and no EC2 node
> management.

**EKS**

> EKS is powerful, but I would not start there unless the team already needs Kubernetes. ECS Fargate
> is simpler and fits this application's deployment model.

**Aurora PostgreSQL**

> Aurora PostgreSQL gives a managed PostgreSQL-compatible production database with high availability,
> automated storage growth, backups, monitoring, and failover. RDS PostgreSQL is a simpler lower-cost
> alternative.

**SNS And SQS**

> SNS plus SQS is the AWS-native way to model fanout events and durable worker queues. It replaces
> RabbitMQ exchanges and queues with managed topics, queues, DLQs, encryption, and autoscaling
> signals.

**Amazon MQ**

> Amazon MQ for RabbitMQ is the fastest migration if we want to keep the current MassTransit
> RabbitMQ transport. For a new AWS-native architecture, I prefer SNS and SQS.

**S3**

> S3 is the cleanest storage fit because Querify already uses the AWS S3 SDK. The production
> improvement is using IAM task roles and KMS rather than static access keys.

**Cognito**

> If the requirement is AWS-only identity, Cognito user pools replace Auth0 as the JWT issuer. Tenant
> authorization still belongs in Querify because tenant membership and module routing are business
> rules.

**CloudWatch And X-Ray**

> CloudWatch gives logs, metrics, dashboards, and alarms. X-Ray and ADOT give distributed tracing.
> With OpenTelemetry already in the repo, this is mostly an exporter and collector decision.

**HealthLake / FHIR**

> Querify is not currently a FHIR product. If FHIR/HL7 becomes a requirement, I would integrate AWS
> HealthLake for a HIPAA-eligible FHIR R4 data store instead of building custom FHIR persistence from
> scratch.

## Gaps Before A Real AWS Production Launch

- Add AWS CDK or CloudFormation IaC.
- Add AWS CodePipeline/CodeBuild/CodeDeploy workflows.
- Decide whether source control must be AWS-only. If yes, use CodeCommit if available or accept the
  operational cost of self-hosted Git on AWS.
- Replace static S3 credentials with ECS task-role credentials.
- Decide between SNS/SQS migration and Amazon MQ for RabbitMQ.
- Add MassTransit Amazon SQS/SNS transport if choosing AWS-native messaging.
- Add ElastiCache Redis backplane for SignalR scale-out.
- Validate SignalR WebSocket routing through CloudFront/ALB under production timeout and scale
  settings.
- Add Cognito integration if moving away from Auth0 for AWS-only identity.
- Add Secrets Manager, Parameter Store, and AppConfig providers.
- Define production WAF, API Gateway, CloudFront, DNS, and certificate rules.
- Define migration deployment order and rollback process.
- Add cloud smoke tests and CloudWatch alarms that can block deployments.
- Validate AWS service regional availability for Canada-focused deployment.
- Validate compliance scope, data residency, retention, audit, and BAA requirements before handling
  healthcare or other regulated data.

## References

- Amazon ECS: https://docs.aws.amazon.com/AmazonECS/latest/developerguide/Welcome.html
- AWS Fargate for Amazon ECS: https://docs.aws.amazon.com/AmazonECS/latest/developerguide/AWS_Fargate.html
- Amazon ECR: https://docs.aws.amazon.com/AmazonECR/latest/userguide/what-is-ecr.html
- Application Load Balancer: https://docs.aws.amazon.com/elasticloadbalancing/latest/application/introduction.html
- Amazon API Gateway: https://docs.aws.amazon.com/apigateway/latest/developerguide/welcome.html
- API Gateway VPC links: https://docs.aws.amazon.com/apigateway/latest/developerguide/apigateway-vpc-links-v2.html
- Amazon CloudFront: https://docs.aws.amazon.com/AmazonCloudFront/latest/DeveloperGuide/Introduction.html
- AWS WAF: https://docs.aws.amazon.com/waf/latest/developerguide/what-is-aws-waf.html
- Amazon S3: https://docs.aws.amazon.com/AmazonS3/latest/userguide/Welcome.html
- Amazon Aurora: https://docs.aws.amazon.com/AmazonRDS/latest/AuroraUserGuide/CHAP_AuroraOverview.html
- Amazon RDS: https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/Welcome.html
- Amazon RDS Proxy: https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/rds-proxy.html
- Amazon SNS: https://docs.aws.amazon.com/sns/latest/dg/welcome.html
- Amazon SQS: https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/welcome.html
- Amazon MQ: https://docs.aws.amazon.com/amazon-mq/latest/developer-guide/welcome.html
- Amazon ElastiCache: https://docs.aws.amazon.com/AmazonElastiCache/latest/dg/WhatIs.html
- Amazon Cognito: https://docs.aws.amazon.com/cognito/latest/developerguide/what-is-amazon-cognito.html
- Amazon SES: https://docs.aws.amazon.com/ses/latest/dg/Welcome.html
- AWS Secrets Manager: https://docs.aws.amazon.com/secretsmanager/latest/userguide/intro.html
- AWS Systems Manager Parameter Store: https://docs.aws.amazon.com/systems-manager/latest/userguide/systems-manager-parameter-store.html
- AWS AppConfig: https://docs.aws.amazon.com/appconfig/latest/userguide/what-is-appconfig.html
- Amazon CloudWatch: https://docs.aws.amazon.com/AmazonCloudWatch/latest/monitoring/WhatIsCloudWatch.html
- AWS X-Ray: https://docs.aws.amazon.com/xray/latest/devguide/aws-xray.html
- AWS Distro for OpenTelemetry: https://aws-otel.github.io/docs/introduction/
- AWS CDK: https://docs.aws.amazon.com/cdk/v2/guide/home.html
- AWS CloudFormation: https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/Welcome.html
- AWS CodeCommit: https://docs.aws.amazon.com/codecommit/latest/userguide/welcome.html
- AWS CodePipeline: https://docs.aws.amazon.com/codepipeline/latest/userguide/welcome.html
- AWS CodeBuild: https://docs.aws.amazon.com/codebuild/latest/userguide/welcome.html
- AWS CodeDeploy: https://docs.aws.amazon.com/codedeploy/latest/userguide/welcome.html
- AWS CodeArtifact: https://docs.aws.amazon.com/codeartifact/latest/ug/welcome.html
- CodeCatalyst source repositories: https://docs.aws.amazon.com/codecatalyst/latest/userguide/source-repositories.html
- Amazon EventBridge: https://docs.aws.amazon.com/eventbridge/latest/userguide/eb-what-is.html
- EventBridge Scheduler: https://docs.aws.amazon.com/scheduler/latest/UserGuide/what-is-scheduler.html
- AWS CloudTrail: https://docs.aws.amazon.com/awscloudtrail/latest/userguide/cloudtrail-user-guide.html
- Amazon GuardDuty: https://docs.aws.amazon.com/guardduty/latest/ug/what-is-guardduty.html
- AWS Security Hub CSPM: https://docs.aws.amazon.com/securityhub/latest/userguide/what-is-securityhub.html
- AWS KMS: https://docs.aws.amazon.com/kms/latest/developerguide/overview.html
- Amazon Macie: https://docs.aws.amazon.com/macie/latest/user/what-is-macie.html
- AWS HealthLake: https://docs.aws.amazon.com/healthlake/latest/devguide/what-is.html
- AWS services in scope by compliance program: https://aws.amazon.com/compliance/services-in-scope/
