# BaseFaq Tenant Worker

## Purpose

`BaseFaq.Tenant.Worker.Api` is the dedicated control-plane background-processing host for BaseFAQ.

It runs against `TenantDbContext` only. It must not own product-data processing.

Current responsibilities:

- billing webhook inbox polling and dispatch
- email outbox polling and delivery
- future control-plane recurring jobs

## Project structure

The worker follows the same pattern as the rest of the solution: a thin host wires self-contained business modules.

```text
BaseFaq.Tenant.Worker.Api               host entry point only (Program.cs, session service, wiring)
BaseFaq.Tenant.Worker.Business.Billing  billing webhook processing (hosted service, processor, handler, options)
BaseFaq.Tenant.Worker.Business.Email    email outbox processing (hosted service, processor, handler, options)
```

### Worker.Api

Contains only:

- `Program.cs`: generic host bootstrap, telemetry registration, `AddTenantWorkerFeatures(...)`
- `Infrastructure/TenantWorkerSessionService.cs`: session stub with no request-bound tenant context
- `Extensions/ServiceCollectionExtensions.cs`: calls `AddBillingBusiness(...)` and `AddEmailBusiness(...)`

### Business.Billing

Fully self-contained billing processing module:

- `Abstractions/IBillingWebhookInboxProcessor.cs`
- `Abstractions/IBillingProvider.cs`, `IBillingProviderResolver.cs`, `IBillingWebhookDispatcher.cs`, `IBillingWebhookEventHandler.cs`
- `Abstractions/WorkItemExecutionResult.cs`
- `Commands/DispatchBillingWebhookInbox/`: MediatR command and handler
- `EventHandlers/`: normalized billing event handlers for Stripe foundation events
- `HostedServices/BillingWebhookInboxProcessorHostedService.cs`
- `Infrastructure/BillingWorkerTelemetry.cs`: activity source `BaseFaq.Tenant.Worker.Billing`
- `Models/`: provider-agnostic normalized billing webhook event model
- `Options/BillingProcessingOptions.cs`, `StripeBillingOptions.cs`
- `Services/BillingWebhookInboxProcessor.cs`, `BillingStateService.cs`, `BillingTenantResolver.cs`, `StripeBillingProvider.cs`, `StripeWebhookEventMapper.cs`
- `Extensions/ServiceCollectionExtensions.cs`: `AddBillingBusiness(config)` registers everything

### Business.Email

Fully self-contained email processing module:

- `Abstractions/IEmailOutboxProcessor.cs`
- `Abstractions/WorkItemExecutionResult.cs`
- `Commands/SendEmailOutbox/`: MediatR command and handler
- `HostedServices/EmailOutboxProcessorHostedService.cs`
- `Infrastructure/EmailWorkerTelemetry.cs`: activity source `BaseFaq.Tenant.Worker.Email`
- `Options/EmailProcessingOptions.cs`, `EmailDeliveryOptions.cs`
- `Services/EmailOutboxProcessor.cs`
- `Extensions/ServiceCollectionExtensions.cs`: `AddEmailBusiness(config)` registers everything

## Architectural boundaries

- `TenantDbContext` is the global control-plane database
- billing, entitlements, platform email, webhook inboxes, and operational jobs belong to `TenantDbContext`
- QnA creation, workflow, public signals, and similar tenant product features stay outside the worker in the QnA APIs plus feature modules

The worker intentionally uses a non-request session implementation because these jobs are not triggered by HTTP request context.

## Processing model

Each processor follows the same optimistic-locking pattern:

1. Query `Pending` records where `NextAttemptDateUtc` and `LockedUntilDateUtc` are elapsed.
2. Claim items one-by-one with `ExecuteUpdateAsync` using a `ProcessingToken` `Guid` as an optimistic lock.
3. Send a MediatR command for each claimed item.
4. On success, mark `Completed`. On exception, schedule retry with backoff. On terminal failure, mark `Failed`.

The hosted service polls in a loop: if items were processed it immediately loops again; if the batch was empty it waits for `PollingInterval`.

## Runtime model

```bash
dotnet restore dotnet/BaseFaq.Tenant.Worker.Api/BaseFaq.Tenant.Worker.Api.csproj
dotnet build dotnet/BaseFaq.Tenant.Worker.Api/BaseFaq.Tenant.Worker.Api.csproj --no-restore
dotnet run --project dotnet/BaseFaq.Tenant.Worker.Api
```

Run in Docker:

```bash
docker compose -p bf_services -f docker/docker-compose.backend.yml up -d --build basefaq.tenant.worker.api
```

## Feature: Billing webhook inbox processor

Hosted service: `BillingWebhookInboxProcessorHostedService` in `Business.Billing`

Processing flow:

- polls `TenantDbContext.BillingWebhookInboxes`
- checks that the table exists before polling, which is safe before migrations are applied
- claims work in batches using status plus lease fields for multi-instance-safe processing
- sends `DispatchBillingWebhookInboxCommand` via MediatR
- resolves the billing provider and normalizes the persisted webhook payload into internal billing event kinds
- dispatches to idempotent billing handlers that upsert control-plane billing state and reconcile tenant entitlements
- records retry timing, attempt counts, and terminal failure state
- remains disabled by default in `appsettings.json` until the persistence model exists and the environment is configured

Current Stripe foundation events:

- `checkout.session.completed`
- `customer.subscription.created`
- `customer.subscription.updated`
- `customer.subscription.deleted`
- `invoice.paid`
- `invoice.payment_failed`

Current normalized state written to `TenantDbContext`:

- `BillingWebhookInboxes`
- `BillingCustomers`
- `TenantSubscriptions`
- `BillingProviderSubscriptions`
- `BillingInvoices`
- `BillingPayments`
- `TenantEntitlementSnapshots`

Configuration:

- `TenantWorker:BillingWebhookInbox:Enabled`
- `TenantWorker:BillingWebhookInbox:PollingIntervalSeconds`
- `TenantWorker:BillingWebhookInbox:BatchSize`
- `TenantWorker:BillingWebhookInbox:LeaseDurationSeconds`
- `TenantWorker:BillingWebhookInbox:FailureBackoffSeconds`
- `TenantWorker:BillingWebhookInbox:MaxRetryCount`
- `TenantWorker:Billing:Stripe:ApiKey`
- `TenantWorker:Billing:Stripe:DefaultCurrency`
- `TenantWorker:Billing:Stripe:CheckoutSuccessUrl`
- `TenantWorker:Billing:Stripe:CheckoutCancelUrl`
- `TenantWorker:Billing:Stripe:BillingPortalReturnUrl`

## Feature: Email outbox processor

Hosted service: `EmailOutboxProcessorHostedService` in `Business.Email`

Processing flow:

- polls `TenantDbContext.EmailOutboxes`
- checks that the table exists before polling
- claims work in batches with lease-based processing markers
- sends `SendEmailOutboxCommand` via MediatR
- records retries, failures, and completion timestamps
- disabled by default in `appsettings.json` until the persistence model and real delivery handler are ready

What is intentionally not implemented yet, because the handler is still a placeholder:

- SMTP delivery implementation
- provider failover
- template rendering
- dead-letter routing

Configuration:

- `TenantWorker:EmailOutbox:Enabled`
- `TenantWorker:EmailOutbox:PollingIntervalSeconds`
- `TenantWorker:EmailOutbox:BatchSize`
- `TenantWorker:EmailOutbox:LeaseDurationSeconds`
- `TenantWorker:EmailOutbox:FailureBackoffSeconds`
- `TenantWorker:EmailOutbox:MaxRetryCount`

## Email delivery configuration

The worker is preconfigured for the repo's local `smtp4dev` container:

- host: `host.docker.internal`
- port: `1025`
- SSL: `false`
- username: empty
- password: empty
- UI: `http://localhost:4590`

Relevant settings:

- `TenantWorker:EmailDelivery:Provider`
- `TenantWorker:EmailDelivery:DefaultFromAddress`
- `TenantWorker:EmailDelivery:DefaultFromName`
- `TenantWorker:EmailDelivery:Smtp:Host`
- `TenantWorker:EmailDelivery:Smtp:Port`
- `TenantWorker:EmailDelivery:Smtp:Username`
- `TenantWorker:EmailDelivery:Smtp:Password`
- `TenantWorker:EmailDelivery:Smtp:EnableSsl`

## Sample data

The seed tool, `BaseFaq.Tools.Seed`, includes realistic billing sample data for local development, demos, QA, and integration tests.

Running option `1` or `3` from the seed tool menu seeds five billing scenarios in `TenantDb`:

- **NorthPeak Analytics**: Pro monthly, Active. Two paid invoices. Use to validate healthy subscription flows and BackOffice billing views.
- **Pacific Trail Studio**: Starter monthly, Trialing. No payment yet, trial active. Use to validate trial entitlement logic.
- **MapleForge Media**: Pro monthly, PastDue. Latest payment failed, in grace period. Use to validate past-due flows and failed payment screens.
- **Aurora Clinic Systems**: Pro yearly, Canceled. Historical invoice and payment exist. Entitlement inactive. Use to validate cancellation flows.
- **BlueHarbor Legal**: Business monthly, Active. Primary tenant for webhook and email outbox demo rows.

Webhook inbox sample rows:

- `checkout.session.completed`: Completed
- `customer.subscription.updated`: Completed
- `invoice.payment_failed`: Failed with error for BackOffice troubleshooting
- `invoice.paid`: Pending for worker processing demos
- `customer.subscription.deleted`: Completed

Email outbox sample rows:

- Invoice receipt for BlueHarbor: Pending
- Payment failure notification for MapleForge: Completed
- Trial payment method reminder for Pacific Trail: Failed and retryable

All billing seed ids are deterministic fixed `Guid`s, making the data suitable for integration tests.

## Manual migration requirements

Database migrations were intentionally not created as part of this implementation. Manual `TenantDbContext` migration work is required before production use:

- create or update `BillingWebhookInboxes`
- create `BillingCustomers`
- create `TenantSubscriptions`
- create `BillingProviderSubscriptions`
- create `BillingInvoices`
- create `BillingPayments`
- create `TenantEntitlementSnapshots`
- create `EmailOutboxes`
- create the configured billing and email indexes, including unique external-id indexes and the `(Provider, ExternalEventId)` uniqueness on `BillingWebhookInboxes`

Until those tables exist, both processors log and safely skip work.
